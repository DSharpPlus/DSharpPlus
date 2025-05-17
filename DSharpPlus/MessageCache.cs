using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DSharpPlus;

public class MessageCache : IMessageCacheProvider
{
    private readonly IMemoryCache cache;
    private readonly MemoryCacheEntryOptions entryOptions;

    public MessageCache(IMemoryCache cache, IOptions<DiscordConfiguration> config)
    {
        this.cache = cache;

        this.entryOptions = new MemoryCacheEntryOptions()
        {
            Size = 1,
            SlidingExpiration = config.Value.SlidingMessageCacheExpiration,
            AbsoluteExpirationRelativeToNow = config.Value.AbsoluteMessageCacheExpiration
        };
    }

    /// <inheritdoc/>
    public void Add(DiscordMessage message) 
        => this.cache.Set(message.Id, message, this.entryOptions);

    /// <inheritdoc/>
    public void Remove(ulong messageId) => this.cache.Remove(messageId);

    /// <inheritdoc/>
    public bool TryGet(ulong messageId, out DiscordMessage? message) => this.cache.TryGetValue(messageId, out message);
}
