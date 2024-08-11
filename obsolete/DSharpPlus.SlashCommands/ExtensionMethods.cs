using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DSharpPlus.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Defines various extension methods for slash commands.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Adds the slash commands extension to the provided service collection.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="setup">Any setup code you want to run on the extension, such as registering commands.</param>
    /// <param name="configuration">The configuration to initialize the extension with.</param>
    /// <returns>The same service collection for chaining.</returns>
    [Obsolete("DSharpPlus.SlashCommands is obsolete. Please consider using the new DSharpPlus.Commands extension instead.")]
    public static IServiceCollection AddSlashCommandsExtension
    (
        this IServiceCollection services,
        Action<SlashCommandsExtension> setup,
        SlashCommandsConfiguration? configuration = null
    )
    {
        services.ConfigureEventHandlers(b => b.AddEventHandlers<SlashCommandsEventHandler>())
            .AddSingleton(provider =>
            {
                DiscordClient client = provider.GetRequiredService<DiscordClient>();

                SlashCommandsExtension extension = new(configuration ?? new());
                extension.Setup(client);
                setup(extension);

                return extension;
            });

        return services;
    }

    /// <summary>
    /// Adds the slash commands extension to the provided DiscordClientBuilder.
    /// </summary>
    /// <param name="builder">The builder to register into.</param>
    /// <param name="setup">Any setup code you want to run on the extension, such as registering commands.</param>
    /// <param name="configuration">The configuration to initialize the extension with.</param>
    /// <returns>The same builder for chaining.</returns>
    [Obsolete("DSharpPlus.SlashCommands is obsolete. Please consider using the new DSharpPlus.Commands extension instead.")]
    public static DiscordClientBuilder UseSlashCommands
    (
        this DiscordClientBuilder builder,
        Action<SlashCommandsExtension> setup,
        SlashCommandsConfiguration? configuration = null
    )
        => builder.ConfigureServices(services => services.AddSlashCommandsExtension(setup, configuration));

    /// <summary>
    /// Gets the name from the <see cref="ChoiceNameAttribute"/> for this enum value.
    /// </summary>
    /// <returns>The name.</returns>
    public static string GetName<T>(this T e) where T : IConvertible
    {
        if (e is Enum)
        {
            Type type = e.GetType();
            Array values = Enum.GetValues(type);

            foreach (int val in values)
            {
                if (val == e.ToInt32(CultureInfo.InvariantCulture))
                {
                    System.Reflection.MemberInfo[] memInfo = type.GetMember(type.GetEnumName(val));

                    return memInfo[0].GetCustomAttributes(typeof(ChoiceNameAttribute), false).FirstOrDefault() is ChoiceNameAttribute nameAttribute
                        ? nameAttribute.Name
                        : type.GetEnumName(val);
                }
            }
        }
        return null;
    }
}
