namespace DSharpPlus.Caching;

public readonly record struct UserPresenceCacheKey(ulong Id) : ICacheKey
{
    public override string ToString() => $"{ICacheKey.KeyPrefix}-userPresence-{this.Id}";
}
