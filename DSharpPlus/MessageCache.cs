using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace DSharpPlus;

internal class MessageCache : IMessageCacheProvider
{
    private readonly MemoryCache _cache;
    private readonly MemoryCacheEntryOptions _entryOptions;

    internal MessageCache(int capacity)
    {
        _cache = new MemoryCache(new MemoryCacheOptions()
        {
            SizeLimit = capacity,
        });

        _entryOptions = new MemoryCacheEntryOptions()
        {
            Size = 1,
        };
    }

    /// <inheritdoc/>
    public void Add(DiscordMessage message) => _cache.Set(message.Id, message, _entryOptions);

    /// <inheritdoc/>
    public void Remove(ulong messageId) => _cache.Remove(messageId);

    /// <inheritdoc/>
    public bool TryGet(ulong messageId, out DiscordMessage? message) => _cache.TryGetValue(messageId, out message);
}
