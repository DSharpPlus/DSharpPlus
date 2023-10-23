namespace DSharpPlus.Caching;

public readonly record struct ChannelCacheKey(ulong Id) : ICacheKey
{
    //key format is "{keyPrefix}-channel-{channelId}"
    public override string ToString() => $"{ICacheKey.KeyPrefix}-channel-{this.Id}";
}
