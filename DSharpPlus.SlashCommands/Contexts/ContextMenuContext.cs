using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Respresents a context for a context menu.
    /// </summary>
    public sealed class ContextMenuContext : BaseContext
    {
        /// <summary>
        /// The user this command targets, if applicable.
        /// </summary>
        public DiscordUser TargetUser { get; internal set; }

        /// <summary>
        /// The member this command targets, if applicable.
        /// </summary>
        public DiscordMember TargetMember { get; internal set; }

        /// <summary>
        /// The message this command targets, if applicable.
        /// </summary>
        public DiscordMessage TargetMessage { get; internal set; }
    }
}
