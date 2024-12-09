using System;
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
    /// <returns>The same service collection for chaining.</returns>
    [Obsolete("DSharpPlus.SlashCommands is obsolete. Please consider using the new DSharpPlus.Commands extension instead.")]
    public static IServiceCollection AddSlashCommandsExtension
    (
        this IServiceCollection services,
        Action<SlashCommandsExtension> setup
    )
    {
        services.ConfigureEventHandlers(b => b.AddEventHandlers<SlashCommandsEventHandler>())
            .AddSingleton(provider =>
            {
                DiscordClient client = provider.GetRequiredService<DiscordClient>();

                SlashCommandsExtension extension = new(provider);
                extension.Setup(client);
                setup(extension);

                return extension;
            });

        return services;
    }

    /// <summary>
    /// Adds the slash commands extension to the provided DiscordClientBuilder.
    /// </summary>
    /// <param name="builder">The client builder to register with.</param>
    /// <param name="setup">Any setup code you want to run on the extension, such as registering commands.</param>
    /// <returns>The same client builder for chaining.</returns>
    [Obsolete("DSharpPlus.SlashCommands is obsolete. Please consider using the new DSharpPlus.Commands extension instead.")]
    public static DiscordClientBuilder UseSlashCommands
    (
        this DiscordClientBuilder builder,
        Action<SlashCommandsExtension> setup
    )
        => builder.ConfigureServices(s => s.AddSlashCommandsExtension(setup));

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
