using System;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Sets a IChoiceProvider for a command options. ChoiceProviders can be used to provide
/// DiscordApplicationCommandOptionChoice from external sources such as a database.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class ChoiceProviderAttribute : Attribute
{
    /// <summary>
    /// The type of the provider.
    /// </summary>
    public Type ProviderType { get; }

    /// <summary>
    /// Adds a choice provider to this command.
    /// </summary>
    /// <param name="providerType">The type of the provider.</param>
    public ChoiceProviderAttribute(Type providerType) => ProviderType = providerType;
}
