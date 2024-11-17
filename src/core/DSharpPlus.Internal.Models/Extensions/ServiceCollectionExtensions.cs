// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0058

using System.Text.Json;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Models.Serialization.Converters;
using DSharpPlus.Internal.Models.Serialization.Resolvers;
using DSharpPlus.Serialization;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Internal.Models.Extensions;

/// <summary>
/// Provides extensions on IServiceCollection to register our JSON serialization of Discord models.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers converters for Discord's API models.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="name">The name under which the serialization options should be accessible.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection RegisterDiscordModelSerialization
    (
        this IServiceCollection services,
        string? name = "dsharpplus"
    )
    {
        services.Configure<JsonSerializerOptions>
        (
            name,
            options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

                options.Converters.Add(new OptionalConverterFactory());
                options.Converters.Add(new SnowflakeConverter());
                options.Converters.Add(new OneOfConverterFactory());
                options.Converters.Add(new ImageDataConverter());

                options.Converters.Add(new AuditLogChangeConverter());
                options.Converters.Add(new AutoModerationActionConverter());
                options.Converters.Add(new DiscordPermissionConverter());
                options.Converters.Add(new ComponentConverter());
                options.Converters.Add(new ApplicationIntegrationTypeKeyConverter());

                options.TypeInfoResolverChain.Add(OptionalTypeInfoResolver.Default);
                options.TypeInfoResolverChain.Add(NullBooleanTypeInfoResolver.Default);
                // this needs to be below OptionalTypeInfoResolver so as to avoid the former overwriting this
                options.TypeInfoResolverChain.Add(AttachmentDataTypeInfoResolver.Default);
            }
        );

        RegisterSerialization(services);

        return services;
    }
}
