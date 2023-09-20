using System.Collections.Generic;
using DSharpPlus.Entities;
namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents event args for the <see cref="DiscordClient.GuildStickersUpdated"/> event.
    /// </summary>
    public class GuildStickersUpdateEventArgs : DiscordEventArgs
    {
            /// <summary>
            /// Gets the list of stickers after the change.
            /// </summary>
            public IReadOnlyDictionary<ulong, DiscordMessageSticker> StickersAfter { get; internal set; }

            /// <summary>
            /// Gets the list of stickers before the change.
            /// </summary>
            public IReadOnlyDictionary<ulong, DiscordMessageSticker> StickersBefore { get; internal set; }

            /// <summary>
            /// Gets the guild in which the update occurred.
            /// </summary>
            public DiscordGuild Guild { get; internal set; }

            internal GuildStickersUpdateEventArgs() : base() { }
    }
}
