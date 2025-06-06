using DSharpPlus.Commands.Processors.SlashCommands;

namespace DSharpPlus.Commands.Processors.MessageCommands;

/// <summary>
/// Indicates that the command was invoked via a message interaction.
/// </summary>
public record MessageCommandContext : SlashCommandContext;
