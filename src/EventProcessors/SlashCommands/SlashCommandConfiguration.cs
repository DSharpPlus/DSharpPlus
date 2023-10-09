namespace DSharpPlus.CommandAll.EventProcessors.SlashCommands
{
    public sealed record SlashCommandConfiguration
    {
        public required bool Enabled { get; set; }
        //public required SlashCommandOptions Options { get; set; }
    }
}
