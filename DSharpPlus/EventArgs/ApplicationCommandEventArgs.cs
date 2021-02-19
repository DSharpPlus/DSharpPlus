using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    public sealed class ApplicationCommandEventArgs : DiscordEventArgs
    {
        public DiscordApplicationCommand Command { get; internal set; }

        public DiscordGuild Guild { get; internal set; }
    }
}
