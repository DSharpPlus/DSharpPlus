// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json;

using DSharpPlus.Core.Models.Converters;

using Microsoft.Extensions.DependencyInjection;

using Remora.Rest.Json;

namespace DSharpPlus.Core.Models.Extensions;

/// <summary>
/// Provides extensions on IServiceCollection to register our JSON serialization of Discord models.
/// </summary>
public static class ServiceCollectionExtensions
{
    const ulong DiscordEpoch = 1420070400000ul;

    /// <summary>
    /// Registers converters for Discord's API models.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="name">The name under which the serialization options should be accessible.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection RegisterDiscordModelSerialization
    (
        this IServiceCollection services,
        string? name = "DSharpPlus"
    )
    {
        services.Configure<JsonSerializerOptions>
        (
            name,
            options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

                options.Converters.Add(new AuditLogChangeConverter());
                options.Converters.Add(new AutoModerationActionConverter());
                options.Converters.Add(new DiscordPermissionConverter());
                options.Converters.Add(new MessageComponentConverter());

                options.RegisterApplicationCommands();
                options.RegisterApplications();
                options.RegisterAuditLogs();
                options.RegisterAutoModeration();
                options.RegisterChannels();
                options.RegisterEmojis();
                options.RegisterGuilds();
                options.RegisterGuildTemplates();
                options.RegisterInteractions();
                options.RegisterInvites();
                options.RegisterMessageComponents();
                options.RegisterRoleConnections();
                options.RegisterScheduledEvents();
                options.RegisterStageInstances();
                options.RegisterStickers();
                options.RegisterTeams();
                options.RegisterUsers();
                options.RegisterVoice();
                options.RegisterWebhooks();

                options.Converters.Insert(0, new SnowflakeConverter(DiscordEpoch));
            }
        );

        return services;
    }
}
