using System;

using DSharpPlus.Entities;
using DSharpPlus.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DSharpPlus.Hosting;

/// <summary>
/// Provides a simple and easy way to register DSharpPlus with a host builder.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Adds DSharpPlus' DiscordClient and all its dependent services to the current host builder.
    /// </summary>
    /// <param name="builder">The current host builder.</param>
    /// <param name="token">The bot token to use to connect to Discord.</param>
    /// <param name="intents">The intents to use to connect to Discord.</param>
    /// <returns>The current instance for chaining.</returns>
    public static IHostBuilder AddDiscordClient(this IHostBuilder builder, string token, DiscordIntents intents)
    {
        builder.ConfigureServices((_, s) =>
        {
            s.AddDiscordClient(token, intents)
                .AddSingleton<DiscordClientService>()
                .AddHostedService(provider => provider.GetRequiredService<DiscordClientService>());
        });

        return builder;
    }

    /// <summary>
    /// Adds DSharpPlus' DiscordClient and all its dependent services to the current host builder, initialized
    /// for running multiple shards.
    /// </summary>
    /// <param name="builder">The current host builder.</param>
    /// <param name="token">The bot token to use to connect to Discord.</param>
    /// <param name="intents">The intents to use to connect to Discord.</param>
    /// <returns>The current instance for chaining.</returns>
    public static IHostBuilder AddShardedDiscordClient(this IHostBuilder builder, string token, DiscordIntents intents)
    {
        builder.ConfigureServices((_, s) =>
        {
            s.AddShardedDiscordClient(token, intents)
                .AddSingleton<DiscordClientService>()
                .AddHostedService(provider => provider.GetRequiredService<DiscordClientService>());
        });

        return builder;
    }

    /// <summary>
    /// Sets an activity for your bot to be registered on startup, comprising of an activity, a status indicator and an optional idle-since timestamp.
    /// </summary>
    /// <param name="builder">The current host builder.</param>
    /// <param name="activity">The activity text for your bot.</param>
    /// <param name="status">The status indicator for your bot.</param>
    /// <param name="idleSince">If <paramref name="status"/> is <see cref="DiscordUserStatus.Idle"/>, the timestamp starting from which your bot is considered idle.</param>
    /// <returns>The current instance for chaining.</returns>
    public static IHostBuilder SetDiscordActivity
    (
        this IHostBuilder builder,
        DiscordActivity? activity = null,
        DiscordUserStatus? status = null,
        DateTimeOffset? idleSince = null
    )
    {
        builder.ConfigureServices((_, s) =>
        {
            s.Configure<DiscordClientStartupOptions>(options =>
            {
                options.Activity = activity;
                options.Status = status;
                options.IdleSince = idleSince;
            });
        });

        return builder;
    }
}
