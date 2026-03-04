using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Voice.Protocol.Gateway;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.Bidirectional;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.Clientbound;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.DaveV1.Clientbound;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.DaveV1.Serverbound;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.Serverbound;

namespace DSharpPlus.Voice.Protocol;

internal class VoiceGatewayMessageConverter : JsonConverter<VoiceGatewayMessage>
{
    public override VoiceGatewayMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);

        VoiceGatewayOpcode opcode = (VoiceGatewayOpcode)doc.RootElement.GetProperty("op").GetInt32();

        // discord's docs offhandedly mention that -1 is used for "no sequence", which is legal for some reason
        if (!doc.RootElement.TryGetProperty("seq", out JsonElement sequenceElement) || !sequenceElement.TryGetInt32(out int sequence))
        {
            sequence = -1;
        }

        JsonElement payload = doc.RootElement.GetProperty("d");

        // [TODO] if discord ever releases DAVE v2, how the *fuck* do we handle that here
        return new()
        {
            Opcode = opcode,
            Sequence = sequence,
            Payload = opcode switch
            {
                VoiceGatewayOpcode.Identify => JsonSerializer.Deserialize<VoiceIdentifyPayload>(payload, options),
                VoiceGatewayOpcode.SelectProtocol => JsonSerializer.Deserialize<VoiceSelectProtocolPayload>(payload, options),
                VoiceGatewayOpcode.Ready => JsonSerializer.Deserialize<VoiceReadyPayload>(payload, options),
                VoiceGatewayOpcode.Heartbeat => JsonSerializer.Deserialize<VoiceHeartbeatPayload>(payload, options),
                VoiceGatewayOpcode.SessionDescription => JsonSerializer.Deserialize<VoiceSessionDescriptionPayload>(payload, options),
                VoiceGatewayOpcode.Speaking => JsonSerializer.Deserialize<VoiceSpeakingPayload>(payload, options),
                VoiceGatewayOpcode.HeartbeatAck => JsonSerializer.Deserialize<VoiceHeartbeatAcknowledgedPayload>(payload, options),
                VoiceGatewayOpcode.Resume => JsonSerializer.Deserialize<VoiceResumePayload>(payload, options),
                VoiceGatewayOpcode.Hello => JsonSerializer.Deserialize<VoiceHelloPayload>(payload, options),
                VoiceGatewayOpcode.Resumed => null, // yes, this sends "d": null
                VoiceGatewayOpcode.ClientsConnected => JsonSerializer.Deserialize<VoiceClientsConnectedPayload>(payload, options),
                VoiceGatewayOpcode.ClientDisconnected => JsonSerializer.Deserialize<VoiceClientDisconnectedPayload>(payload, options),
                VoiceGatewayOpcode.PrepareTransition => JsonSerializer.Deserialize<DavePrepareTransitionPayload>(payload, options),
                VoiceGatewayOpcode.ExecuteTransition => JsonSerializer.Deserialize<DaveExecuteTransitionPayload>(payload, options),
                VoiceGatewayOpcode.TransitionReady => JsonSerializer.Deserialize<DaveTransitionReadyPayload>(payload, options),
                VoiceGatewayOpcode.PrepareEpoch => JsonSerializer.Deserialize<DavePrepareEpochPayload>(payload, options),
                VoiceGatewayOpcode.MlsInvalidCommitWelcome => JsonSerializer.Deserialize<MlsInvalidCommitWelcomePayload>(payload, options),
                _ => throw new JsonException($"Unknown voice gateway opcode {opcode}.")
            }
        };
    }

    public override void Write(Utf8JsonWriter writer, VoiceGatewayMessage value, JsonSerializerOptions options)
    {
        writer.WritePropertyName("op");
        writer.WriteNumberValue((int)value.Opcode);
        writer.WritePropertyName("d");
        // writing as object prompts it to write out all properties without having us specify the type
        JsonSerializer.Serialize<object>(writer, value.Payload, options);
    }
}