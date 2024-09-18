using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.VoiceNext;

public static class DiscordClientExtensions
{
    /// <summary>
    /// Registers a new VoiceNext client to the service collection.
    /// </summary>
    /// <param name="services">The service collection to register to.</param>
    /// <param name="configuration">Configuration for this VoiceNext client.</param>
    /// <returns>The same service collection for chaining</returns>
    public static IServiceCollection AddVoiceNextExtension
    (
        this IServiceCollection services,
        VoiceNextConfiguration configuration
    )
    {
        services.ConfigureEventHandlers(b => b.AddEventHandlers<VoiceNextEventHandler>())
            .AddSingleton(provider =>
            {
                DiscordClient client = provider.GetRequiredService<DiscordClient>();

                VoiceNextExtension extension = new(configuration ?? new());
                extension.Setup(client);

                return extension;
            });

        return services;
    }

    /// <summary>
    /// Registers a new VoiceNext client to the specified client builder.
    /// </summary>
    /// <param name="builder">The builder to register to.</param>
    /// <param name="configuration">Configuration for this VoiceNext client.</param>
    /// <returns>The same builder for chaining</returns>
    public static DiscordClientBuilder UseVoiceNext
    (
        this DiscordClientBuilder builder,
        VoiceNextConfiguration configuration
    )
        => builder.ConfigureServices(services => services.AddVoiceNextExtension(configuration));

    /// <summary>
    /// Connects to this voice channel using VoiceNext.
    /// </summary>
    /// <param name="channel">Channel to connect to.</param>
    /// <returns>If successful, the VoiceNext connection.</returns>
    public static Task<VoiceNextConnection> ConnectAsync(this DiscordChannel channel)
    {
        if (channel == null)
        {
            throw new NullReferenceException();
        }

        if (channel.Guild == null)
        {
            throw new InvalidOperationException("VoiceNext can only be used with guild channels.");
        }

        if (channel.Type is not DiscordChannelType.Voice and not DiscordChannelType.Stage)
        {
            throw new InvalidOperationException("You can only connect to voice or stage channels.");
        }

        if (channel.Discord is not DiscordClient discord || discord == null)
        {
            throw new NullReferenceException();
        }

        VoiceNextExtension vnext = discord.ServiceProvider.GetService<VoiceNextExtension>()
            ?? throw new InvalidOperationException("VoiceNext is not initialized for this Discord client.");
        VoiceNextConnection? vnc = vnext.GetConnection(channel.Guild);
        return vnc != null
            ? throw new InvalidOperationException("VoiceNext is already connected in this guild.")
            : vnext.ConnectAsync(channel);
    }
}
