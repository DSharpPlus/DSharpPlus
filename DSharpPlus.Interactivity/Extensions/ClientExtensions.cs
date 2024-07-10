using System;

namespace DSharpPlus.Interactivity.Extensions;

/// <summary>
/// Interactivity extension methods for <see cref="DiscordClient"/>.
/// </summary>
public static class ClientExtensions
{
    /// <summary>
    /// Enables interactivity for this <see cref="DiscordClient"/> instance.
    /// </summary>
    /// <param name="client">The client to enable interactivity for.</param>
    /// <param name="configuration">A configuration instance. Default configuration values will be used if none is provided.</param>
    /// <returns>A brand new <see cref="InteractivityExtension"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if interactivity has already been enabled for the client instance.</exception>
    public static InteractivityExtension UseInteractivity(this DiscordClient client, InteractivityConfiguration configuration = null)
    {
        if (client.GetExtension<InteractivityExtension>() != null)
        {
            throw new InvalidOperationException($"Interactivity is already enabled for this {(client.isShard ? "shard" : "client")}.");
        }

        configuration ??= new InteractivityConfiguration();
        InteractivityExtension extension = new(configuration);
        client.AddExtension(extension);

        return extension;
    }

    /// <summary>
    /// Retrieves the registered <see cref="InteractivityExtension"/> instance for this client.
    /// </summary>
    /// <param name="client">The client to retrieve an <see cref="InteractivityExtension"/> instance from.</param>
    /// <returns>An existing <see cref="InteractivityExtension"/> instance, or <see langword="null"/> if interactivity is not enabled for the <see cref="DiscordClient"/> instance.</returns>
    public static InteractivityExtension GetInteractivity(this DiscordClient client)
        => client.GetExtension<InteractivityExtension>();
}
