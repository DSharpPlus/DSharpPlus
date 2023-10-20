namespace DSharpPlus.Cache;

public readonly record struct MessageCacheKey(ulong Id) : ICacheKey
{
    //key format is "{keyPrefix}-message-{messageId}"
    public override string ToString() => $"{ICacheKey.KeyPrefix}-message-{this.Id}";
}
