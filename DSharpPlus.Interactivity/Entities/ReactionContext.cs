using DSharpPlus.Entities;

// ReSharper disable once CheckNamespace
namespace DSharpPlus.Interactivity
{
    public class ReactionContext
    {
        public DiscordChannel Channel { get; internal set; }

        public DiscordUser User { get; internal set; }

        public DiscordEmoji Emoji { get; internal set; }

        public DiscordMessage Message { get; internal set; }

        public DiscordGuild Guild => Channel.Guild;

        public InteractivityExtension Interactivity { get; internal set; }

        public DiscordClient Client => Interactivity.Client;
    }
}
