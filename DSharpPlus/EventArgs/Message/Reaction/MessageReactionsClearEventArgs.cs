using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

using System.Threading.Channels;
using Caching;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessageReactionsCleared"/> event.
/// </summary>
public class MessageReactionsClearEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the message for which the update occurred.
    /// </summary>
    public CachedEntity<ulong, DiscordMessage> Message { get; internal set; }

    /// <summary>
    /// Gets the channel to which this message belongs.
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the guild in which the reactions were cleared. This value is <c>null</c> if message was in DMs.
    /// </summary> 
    public CachedEntity<ulong, DiscordGuild>? Guild { get; internal set; }


    internal MessageReactionsClearEventArgs() : base() { }
}
