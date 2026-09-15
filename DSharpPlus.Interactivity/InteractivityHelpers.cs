using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Interactivity;

public static class InteractivityHelpers
{
    internal static InteractivityOptions GetInteractivityOptions(this DiscordClient client) 
        => client.ServiceProvider.GetRequiredService<IOptions<InteractivityOptions>>().Value;

    internal static TimeSpan GetDefaultInteractivityTimeout(this DiscordClient client)
        => client.ServiceProvider.GetRequiredService<IOptions<InteractivityOptions>>().Value.Timeout;

    internal static InteractivityOptions GetInteractivityOptions(this BaseDiscordClient client)
    {
        return client is not DiscordClient discordClient
            ? throw new InvalidOperationException("Interactivity must be used on a DiscordClient, other client types are not supported.")
            : discordClient.GetInteractivityOptions();
    }

    internal static TimeSpan GetDefaultInteractivityTimeout(this BaseDiscordClient client)
    {
        return client is not DiscordClient discordClient
            ? throw new InvalidOperationException("Interactivity must be used on a DiscordClient, other client types are not supported.")
            : discordClient.GetDefaultInteractivityTimeout();
    }

    internal static DiscordClient AsDiscordClient(this BaseDiscordClient client)
    {
        return client is not DiscordClient discordClient
            ? throw new InvalidOperationException("Interactivity must be used on a DiscordClient, other client types are not supported.")
            : discordClient;
    }

    /// <summary>
    /// Returns whether this operation failed due to timing out.
    /// </summary>
    public static bool IsTimedOut<T>(this Result<T> result)
        => result.Error is TimeoutError;

    /// <summary>
    /// Returns whether this operation failed due to timing out.
    /// </summary>
    public static bool IsTimedOut(this Result result)
        => result.Error is TimeoutError;
}
