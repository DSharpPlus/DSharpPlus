using DSharpPlus.Commands.Processors.SlashCommands.NamingPolicies;

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
    public IInteractionNamingPolicy NamingPolicy { get; init; } = new SnakeCaseNamingPolicy();

    /// <summary>
    /// Instructs DSharpPlus to always overwrite the command records Discord has of our bot on startup.
    /// </summary>
    /// <remarks>
    /// This skips the startup procedure of fetching commands and overwriting only if additions are detected. While
    /// this may save time on startup, it also makes the library less resistant to unrecognized command types or
    /// structures it cannot correctly handle. <br/>
    /// Currently, removals are <i>not</i> considered a reason to overwrite by default so as to work around an issue
    /// where certain commands will cause bulk overwrites to fail.
    /// </remarks>
    public bool UnconditionallyOverwriteCommands { get; init; } = false;
}
