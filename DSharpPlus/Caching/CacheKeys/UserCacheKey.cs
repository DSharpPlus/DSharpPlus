namespace DSharpPlus.Caching;

public readonly record struct UserCacheKey(ulong Id) : ICacheKey
{
    public override string ToString() => $"{ICacheKey.KeyPrefix}-user-{this.Id}";
}
