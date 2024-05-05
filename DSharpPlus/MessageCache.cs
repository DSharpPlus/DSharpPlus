using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace DSharpPlus;

internal class MessageCache : IMessageCacheProvider
{
    private readonly MemoryCache cache;
    private readonly MemoryCacheEntryOptions entryOptions;

    internal MessageCache(int capacity)
    {
        this.cache = new MemoryCache(new MemoryCacheOptions()
        {
            SizeLimit = capacity,
        });

        this.entryOptions = new MemoryCacheEntryOptions()
        {
            Size = 1,
        };
    }

    /// <inheritdoc/>
    public void Add(DiscordMessage message) => this.cache.Set(message.Id, message, this.entryOptions);

    /// <inheritdoc/>
    public void Remove(ulong messageId) => this.cache.Remove(messageId);

    /// <inheritdoc/>
    public bool TryGet(ulong messageId, out DiscordMessage? message) => this.cache.TryGetValue(messageId, out message);
}
