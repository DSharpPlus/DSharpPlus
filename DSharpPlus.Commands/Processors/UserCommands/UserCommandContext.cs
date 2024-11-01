using DSharpPlus.Commands.Processors.SlashCommands;

namespace DSharpPlus.Commands.Processors.UserCommands;

/// <summary>
/// Indicates that the command was invoked via a user interaction.
/// </summary>
public record UserCommandContext : SlashCommandContext;
