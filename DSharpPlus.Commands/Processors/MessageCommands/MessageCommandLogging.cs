using System;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors.MessageCommands;

internal static class MessageCommandLogging
{
    internal static readonly Action<ILogger, Exception?> interactionReceivedBeforeConfigured = LoggerMessage.Define(LogLevel.Warning, new EventId(1, "Message Commands Startup"), "Received an interaction before the message commands processor was configured. This interaction will be ignored.");
    internal static readonly Action<ILogger, string, Exception?> messageCommandCannotHaveSubcommands = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(4, "Message Commands Startup"), "The message context menu command '{CommandName}' cannot have subcommands.");
    internal static readonly Action<ILogger, string, Exception?> messageCommandContextParameterType = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(5, "Message Commands Startup"), $"The first parameter of '{{CommandName}}' does not implement {nameof(SlashCommandContext)}. Since this command is being registered as a message context menu command, it's first parameter must inherit the {nameof(SlashCommandContext)} class.");
    internal static readonly Action<ILogger, string, Exception?> invalidParameterType = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(2, "Message Commands Startup"), "The second parameter of '{CommandName}' is not a DiscordMessage. Since this command is being registered as a message context menu command, it's second parameter must be a DiscordMessage.");
    internal static readonly Action<ILogger, int, string, Exception?> invalidParameterMissingDefaultValue = LoggerMessage.Define<int, string>(LogLevel.Warning, new EventId(3, "Message Commands Startup"), "Parameter {ParameterIndex} of '{CommandName}' does not have a default value. Since this command is being registered as a message context menu command, any additional parameters must have a default value.");
}
