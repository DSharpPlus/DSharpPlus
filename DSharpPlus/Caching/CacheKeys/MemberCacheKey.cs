namespace DSharpPlus.Caching;

public readonly record struct MemberCacheKey(ulong Id, ulong GuildId) : ICacheKey
{
    //key format is "{keyPrefix}-guild-{this.GuildId}-member-{this.Id}"
    public override string ToString() => $"{ICacheKey.KeyPrefix}-guild-{this.GuildId}-member-{this.Id}";
}
