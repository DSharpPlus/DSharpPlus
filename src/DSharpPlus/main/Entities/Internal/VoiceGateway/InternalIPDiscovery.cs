namespace DSharpPlus.Core.Entities.VoiceGateway
{
    public sealed record InternalIPDiscovery
    {
        public ushort Type { get; init; }
        public ushort Length { get; init; } = 70;
        public uint SSRC { get; init; }
        public string Address { get; init; } = null!;
        public ushort Port { get; init; }
    }
}
