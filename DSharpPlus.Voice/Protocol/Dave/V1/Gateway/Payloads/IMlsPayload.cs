namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads;

internal interface IMlsPayload : IVoicePayload
{
    /// <summary>
    /// The MLS message passed down from the gateway. This is always the last field in a binary payload.
    /// </summary>
    public byte[] MlsMessage { get; }
}
