namespace DSharpPlus.Caching;

public readonly record struct GuildCacheKey(ulong Id) : ICacheKey
{
    //key format is "{keyPrefix}-guild-{guildId}"
    public override string ToString() => $"{ICacheKey.KeyPrefix}-guild-{this.Id}";
}
