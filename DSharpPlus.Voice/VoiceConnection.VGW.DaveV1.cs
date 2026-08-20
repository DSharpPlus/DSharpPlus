#pragma warning disable IDE0040

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.Gateway;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.DaveV1.Clientbound;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.DaveV1.Serverbound;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice;

partial class VoiceConnection
{
    private async Task HandleDaveV1JsonPayloadsAsync(VoiceGatewayMessage message)
    {
        switch (message.Opcode)
        {
            case VoiceGatewayOpcode.PrepareTransition:

                DavePrepareTransitionPayload prepareTransition = (DavePrepareTransitionPayload)message.Payload;

                // zero means we just ignore
                if (prepareTransition.TransitionId == 0)
                {
                    break;
                }

                this.pendingTransitionId = prepareTransition.TransitionId;
                this.pendingTransitionProtocolVersion = prepareTransition.ProtocolVersion;

                // we're ready
                await this.voiceGateway.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(new VoiceGatewayMessage()
                {
                    Opcode = VoiceGatewayOpcode.TransitionReady,
                    Payload = new DaveTransitionReadyPayload
                    {
                        TransitionId = prepareTransition.TransitionId
                    }
                }));

                break;

            case VoiceGatewayOpcode.ExecuteTransition:

                DaveExecuteTransitionPayload executeTransition = (DaveExecuteTransitionPayload)message.Payload;

                if (this.pendingTransitionId.HasValue && this.pendingTransitionId != executeTransition.TransitionId)
                {
                    _ = ReconnectInternalAsync(false);
                }

                if (this.pendingTransitionProtocolVersion.HasValue)
                {
                    this.daveVersion = this.pendingTransitionProtocolVersion.Value;
                }

                this.pendingTransitionProtocolVersion = null;
                this.pendingTransitionId = null;

                if (this.daveVersion != 0)
                {
                    this.e2ee.ReinitializeE2EESession((ushort)this.daveVersion);
                }

                break;

            case VoiceGatewayOpcode.PrepareEpoch:

                DavePrepareEpochPayload prepareEpoch = (DavePrepareEpochPayload)message.Payload;

                if (prepareEpoch.EpochId == 1)
                {
                    this.logger.LogTrace("Initializing DAVE session");

                    this.daveVersion = prepareEpoch.ProtocolVersion;
                    this.e2ee.ReinitializeE2EESession(prepareEpoch.ProtocolVersion);

                    await DaveV1AnnounceKeyPackageAsync();
                }

                break;

            default:

                this.logger.LogWarning("Opcode {opcode} is not defined for DAVE v1.", message.Opcode);
                break;
        }
    }

    private async Task HandleDaveV1BinaryPayloadsAsync(byte[] payload)
    {
        // here, the sequence is guaranteed
        this.lastSequence = BinaryPrimitives.ReadUInt16BigEndian(payload);
        VoiceGatewayOpcode opcode = (VoiceGatewayOpcode)payload[2];

        switch (opcode)
        {
            case VoiceGatewayOpcode.MlsExternalSender:
                
                this.e2ee.SetExternalSender(payload.AsSpan(3));
                this.mlsReady?.TrySetResult();

                break;

            case VoiceGatewayOpcode.MlsProposals:

                byte[] response = this.e2ee.ProcessProposals(payload.AsSpan(3), [.. this.connectedUsers]);

                if (response is { Length: > 0 })
                {
                    await DaveV1CommitWelcomeAsync(response);
                }

                break;

            case VoiceGatewayOpcode.MlsAnnounceCommitTransition:

                if (payload.Length <= 5)
                {
                    this.logger.LogTrace("Received invalid commit transition, reinitializing");

                    ushort transitionId = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(3));
                    await DaveV1SendInvalidCommitAsync(transitionId);

                    this.e2ee.ReinitializeE2EESession((ushort)this.daveVersion);

                    await DaveV1AnnounceKeyPackageAsync();
                }

                bool success = this.e2ee.ProcessCommit(payload.AsSpan(5));

                if (!success)
                {
                    this.logger.LogTrace("Failed to process commit transition, reinitializing");

                    ushort transitionId = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(3));
                    await DaveV1SendInvalidCommitAsync(transitionId);

                    this.e2ee.ReinitializeE2EESession((ushort)this.daveVersion);

                    await DaveV1AnnounceKeyPackageAsync();
                }

                break;

            case VoiceGatewayOpcode.MlsWelcome:

                this.e2ee.ProcessWelcome(payload.AsSpan(5), [.. this.connectedUsers]);
                this.mlsReady?.TrySetResult();

                break;

            default:

                // we don't really need to reconnect here, discord tests in prod all the time
                this.logger.LogWarning("Opcode {opcode} is not defined for DAVE v1.", opcode);
                break;
        }
    }

    private async Task DaveV1AnnounceKeyPackageAsync()
    {
        using ArrayPoolBufferWriter<byte> writer = new();

        writer.Write((byte)VoiceGatewayOpcode.MlsKeyPackage);
        this.e2ee.WriteKeyPackage(writer);

        await this.voiceGateway.WriteAsync(writer.WrittenMemory, WebSocketMessageType.Binary);
    }

    private async Task DaveV1CommitWelcomeAsync(ReadOnlyMemory<byte> message)
    {
        using ArrayPoolBufferWriter<byte> writer = new();

        writer.Write((byte)VoiceGatewayOpcode.MlsCommitWelcome);
        writer.Write(message.Span);

        await this.voiceGateway.WriteAsync(writer.WrittenMemory, WebSocketMessageType.Binary);
    }

    private async Task DaveV1SendInvalidCommitAsync(ushort transitionId)
    {
        VoiceGatewayMessage message = new()
        {
            Opcode = VoiceGatewayOpcode.MlsInvalidCommitWelcome,
            Payload = new MlsInvalidCommitWelcomePayload()
            {
                TransitionId = transitionId
            }
        };

        await this.voiceGateway.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(message));
    }
}
