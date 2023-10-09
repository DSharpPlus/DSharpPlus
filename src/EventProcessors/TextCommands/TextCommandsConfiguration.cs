namespace DSharpPlus.CommandAll.EventProcessors
{
    public sealed record TextCommandsConfiguration
    {
        public required bool Enabled { get; set; }
        public required TextCommandOptions Options { get; set; }
    }
}
