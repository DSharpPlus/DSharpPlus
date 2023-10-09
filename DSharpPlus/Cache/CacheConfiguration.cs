namespace DSharpPlus.Cache;

using System;

public record CacheConfiguration
{
    public TimeSpan GuildLifetime { get; init; } = TimeSpan.FromMinutes(30);
    public TimeSpan ChannelLifetime { get; init; } = TimeSpan.FromMinutes(30);
    public TimeSpan ThreadLifetime { get; init; } = TimeSpan.FromMinutes(30);
    public TimeSpan UserLifetime { get; init; } = TimeSpan.FromMinutes(30);
    public TimeSpan MessageLifetime { get; init; } = TimeSpan.FromMinutes(30);
}
