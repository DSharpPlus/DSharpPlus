using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DSharpPlus;

public class MessageCache : IMessageCacheProvider
{
    private readonly IMemoryCache cache;
    private readonly MemoryCacheEntryOptions entryOptions;

    public MessageCache(IMemoryCache cache)
    {
        this.cache = cache;

        this.entryOptions = new MemoryCacheEntryOptions()
        {
            Size = 1,
        };
    }

    internal MessageCache(int capacity)
    {
        this.cache = new MemoryCache(Options.Create(new MemoryCacheOptions
        {
            SizeLimit = capacity
        }));

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
