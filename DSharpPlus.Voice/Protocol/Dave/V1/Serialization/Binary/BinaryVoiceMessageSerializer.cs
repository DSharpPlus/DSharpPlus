#pragma warning disable CA1859 // the analyzer can't see through what we're doing with this

using System;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Collections.Generic;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.Dave.V1.Gateway;
using DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads;
using DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;
using DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Serialization.Binary;

internal sealed class BinaryVoiceMessageSerializer
{
    private readonly FrozenDictionary<VoiceGatewayOpcode, Func<ReadOnlySpan<byte>, IVoicePayload>> deserializers;
    private readonly FrozenDictionary<VoiceGatewayOpcode, Action<IVoicePayload, ArrayPoolBufferWriter<byte>>> serializers;

    public BinaryVoiceMessageSerializer()
    {
        Dictionary<VoiceGatewayOpcode, Func<ReadOnlySpan<byte>, IVoicePayload>> deserializersInProgress = new(4)
        {
            [VoiceGatewayOpcode.MlsExternalSender] = MlsExternalSenderPayloadDeserializer,
            [VoiceGatewayOpcode.MlsProposals] = MlsProposalsPayloadDeserializer,
            [VoiceGatewayOpcode.MlsAnnounceCommitTransition] = MlsAnnounceCommitTransitionPayloadDeserializer,
            [VoiceGatewayOpcode.MlsWelcome] = MlsWelcomePayloadDeserializer
        };

        this.deserializers = deserializersInProgress.ToFrozenDictionary();

        Dictionary<VoiceGatewayOpcode, Action<IVoicePayload, ArrayPoolBufferWriter<byte>>> serializersInProgress = new(2)
        {
            [VoiceGatewayOpcode.MlsKeyPackage] = MlsKeyPackagePayloadSerializer,
            [VoiceGatewayOpcode.MlsCommitWelcome] = MlsCommitWelcomePayloadSerializer
        };

        this.serializers = serializersInProgress.ToFrozenDictionary();
    }

    public VoiceGatewayEvent Deserialize(ReadOnlySpan<byte> message)
    {
        ushort sequence = BinaryPrimitives.ReadUInt16BigEndian(message[0..2]);
        VoiceGatewayOpcode opcode = (VoiceGatewayOpcode)message[2];
        IVoicePayload payload = this.deserializers[opcode](message[3..]);

        return new()
        {
            Opcode = opcode,
            Sequence = sequence,
            Payload = payload
        };
    }

    public void Serialize(VoiceGatewayEvent @event, ArrayPoolBufferWriter<byte> writer)
    {
        writer.Write((byte)@event.Opcode);
        this.serializers[@event.Opcode](@event.Payload, writer);
    }

    private static IVoicePayload MlsExternalSenderPayloadDeserializer(ReadOnlySpan<byte> buffer)
    {
        return new MlsExternalSenderPayload
        {
            MlsMessage = buffer.ToArray()
        };
    }

    private static IVoicePayload MlsProposalsPayloadDeserializer(ReadOnlySpan<byte> buffer)
    {
        return new MlsProposalsPayload
        {
            MlsMessage = buffer.ToArray()
        };
    }

    private static IVoicePayload MlsAnnounceCommitTransitionPayloadDeserializer(ReadOnlySpan<byte> buffer)
    {
        return new MlsAnnounceCommitTransitionPayload
        {
            TransitionId = BinaryPrimitives.ReadUInt16BigEndian(buffer),
            MlsMessage = buffer[2..].ToArray()
        };
    }

    private static IVoicePayload MlsWelcomePayloadDeserializer(ReadOnlySpan<byte> buffer)
    {
        return new MlsWelcomePayload
        {
            TransitionId = BinaryPrimitives.ReadUInt16BigEndian(buffer),
            MlsMessage = buffer[2..].ToArray()
        };
    }

    private void MlsKeyPackagePayloadSerializer(IVoicePayload payload, ArrayPoolBufferWriter<byte> writer)
    {
        MlsKeyPackagePayload typed = (MlsKeyPackagePayload)payload;

        writer.Write<byte>(typed.MlsMessage);
    }

    private void MlsCommitWelcomePayloadSerializer(IVoicePayload payload, ArrayPoolBufferWriter<byte> writer)
    {
        MlsCommitWelcomePayload typed = (MlsCommitWelcomePayload)payload;

        writer.Write<byte>(typed.MlsMessage);
    }
}
