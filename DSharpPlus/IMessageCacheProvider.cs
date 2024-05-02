using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;

namespace DSharpPlus;

public interface IMessageCacheProvider
{
    /// <summary>
    /// Add a <see cref="DiscordMessage"/> object to the cache.
    /// </summary>
    /// <param name="message">The <see cref="DiscordMessage"/> object to add to the cache.</param>
    void Add(DiscordMessage message);

    /// <summary>
    /// Remove the <see cref="DiscordMessage"/> object associated with the message ID from the cache. 
    /// </summary>
    /// <param name="messageId">The ID of the message to remove from the cache.</param>
    void Remove(ulong messageId);

    /// <summary>
    /// Try to get a <see cref="DiscordMessage"/> object associated with the message ID from the cache.
    /// </summary>
    /// <param name="messageId">The ID of the message to retrieve from the cache.</param>
    /// <param name="message">The <see cref="DiscordMessage"/> object retrieved from the cache, if it exists; null otherwise.</param>
    /// <returns><see langword="true"/> if the message can be retrieved from the cache, <see langword="false"/> otherwise.</returns>
    bool TryGet(ulong messageId, [NotNullWhen(true)] out DiscordMessage? message);
}
