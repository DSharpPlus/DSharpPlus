namespace DSharpPlus.CommandAll.Processors.HttpCommands
{
    public record HttpCommandConfiguration
    {
        public required byte[] PublicKey { get; init; } = new byte[32];
        public required string[] Prefixes { get; init; } = ["http://0.0.0.0/", "http://0.0.0.0:8080/"];
    }
}
