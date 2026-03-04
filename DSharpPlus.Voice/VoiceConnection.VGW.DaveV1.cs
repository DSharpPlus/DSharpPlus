#pragma warning disable IDE0040

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.Gateway;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.DaveV1.Clientbound;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.DaveV1.Serverbound;
using DSharpPlus.Voice.Transport;

namespace DSharpPlus.Voice;

partial class VoiceConnection
{
    private async Task HandleDaveV1JsonPayloadsAsync(VoiceGatewayMessage message)
    {
        switch (message.Opcode)
        {
            case VoiceGatewayOpcode.PrepareTransition:

                DavePrepareTransitionPayload prepareTransition = (DavePrepareTransitionPayload)message.Payload;

                // zero means we just reinitialize
                if (prepareTransition.TransitionId == 0)
                {
                    this.e2ee.ReinitializeE2EESession((ushort)this.daveVersion);
                    break;
                }

                this.pendingTransitionId = prepareTransition.TransitionId;
                this.pendingTransitionProtocolVersion = prepareTransition.ProtocolVersion;

                // we're ready
                await this.voiceGateway.SendTextAsync(new()
                {
                    Opcode = VoiceGatewayOpcode.TransitionReady,
                    Payload = new DaveTransitionReadyPayload
                    {
                        TransitionId = prepareTransition.TransitionId
                    }
                });

                break;

            case VoiceGatewayOpcode.ExecuteTransition:

                DaveExecuteTransitionPayload executeTransition = (DaveExecuteTransitionPayload)message.Payload;

                if (this.pendingTransitionId != executeTransition.TransitionId)
                {
                    // [TODO] something insane happened, reconnect?
                }

                this.daveVersion = this.pendingTransitionProtocolVersion;
                this.e2ee.ReinitializeE2EESession((ushort)this.daveVersion);

                break;

            case VoiceGatewayOpcode.PrepareEpoch:

                DavePrepareEpochPayload prepareEpoch = (DavePrepareEpochPayload)message.Payload;

                this.daveVersion = prepareEpoch.ProtocolVersion;
                this.e2ee.ReinitializeE2EESession(prepareEpoch.ProtocolVersion);

                // epoch id 1 means we're fully recreating the MLS group
                // [TODO] figure out what happens to the actual e2ee part of e2ee when we do this
                if (prepareEpoch.EpochId == 1)
                {
                    ArrayPoolBufferWriter<byte> writer = new();

                    writer.Write((byte)VoiceGatewayOpcode.MlsKeyPackage);
                    this.e2ee.WriteKeyPackage(writer);

                    await this.voiceGateway.SendBinaryAsync(writer.WrittenMemory);
                }

                break;

            default:

                throw new InvalidOperationException($"Opcode {message.Opcode} is not defined for DAVE v1.");
        }
    }

    private async Task HandleDaveV1BinaryPayloadsAsync(VoiceGatewayTransportFrame frame)
    {
        Debug.Assert(frame.IsBinary);

        // here, the sequence is guaranteed
        this.lastSequence = BinaryPrimitives.ReadUInt16BigEndian(frame.Payload);

        switch (frame.Opcode)
        {
            case VoiceGatewayOpcode.MlsExternalSender:
                
                this.e2ee.SetExternalSender(frame.Payload.AsSpan(3));

                break;

            case VoiceGatewayOpcode.MlsProposals:

                byte[] response = this.e2ee.ProcessProposals(frame.Payload.AsSpan(3), [.. this.connectedUsers]);

                if (response is { Length: > 1 })
                {
                    ArrayPoolBufferWriter<byte> writer = new();

                    writer.Write((byte)VoiceGatewayOpcode.MlsCommitWelcome);
                    writer.Write(response);

                    await this.voiceGateway.SendBinaryAsync(writer.WrittenMemory);
                }

                break;

            case VoiceGatewayOpcode.MlsAnnounceCommitTransition:

                this.e2ee.ProcessCommit(frame.Payload.AsSpan(3));

                break;

            case VoiceGatewayOpcode.MlsCommitWelcome:

                this.e2ee.ProcessWelcome(frame.Payload.AsSpan(3), [.. this.connectedUsers]);

                break;

            default:

                throw new InvalidOperationException($"Opcode {frame.Opcode} is not defined for DAVE v1.");
        }
    }
}