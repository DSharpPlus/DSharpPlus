namespace DSharpPlus.Caching;

public readonly record struct UserCacheKey(ulong Id) : ICacheKey
{
    //key format is "{keyPrefix}-user-{userId}"
    public override string ToString() => $"user-{this.Id}";
}
