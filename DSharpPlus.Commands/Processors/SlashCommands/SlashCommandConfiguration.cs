using DSharpPlus.Commands.Processors.SlashCommands.ParameterNamingPolicies;

namespace DSharpPlus.Commands.Processors.SlashCommands;

/// <summary>
/// The configuration for the <see cref="SlashCommandProcessor"/>.
/// </summary>
public sealed class SlashCommandConfiguration
{
    /// <summary>
    /// Whether to register <see cref="CommandsExtension.Commands"/> in their
    /// application command form and map them back to their original commands.
    /// </summary>
    /// <remarks>
    /// Set this to <see langword="false"/> if you want to manually register
    /// commands or obtain your application commands from a different source.
    /// </remarks>
    public bool RegisterCommands { get; init; } = true;

    /// <summary>
    /// How to name parameters when registering or receiving interaction data.
    /// </summary>
    public IInteractionNamingPolicy ParameterNamePolicy { get; init; } = new SnakeCaseNamingPolicy();
}
