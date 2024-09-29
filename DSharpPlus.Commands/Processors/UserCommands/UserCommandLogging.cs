using System;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors.UserCommands;

internal static class UserCommandLogging
{
    internal static readonly Action<ILogger, Exception?> interactionReceivedBeforeConfigured =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(1, "User Commands Startup"),
            "Received an interaction before the user commands processor was configured. This interaction will be ignored."
        );
    internal static readonly Action<ILogger, string, Exception?> userCommandCannotHaveSubcommands =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(4, "User Commands Startup"),
            "The user context menu command '{CommandName}' cannot have subcommands."
        );
    internal static readonly Action<ILogger, string, Exception?> userCommandContextParameterType =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(5, "User Commands Startup"),
            $"The first parameter of '{{CommandName}}' does not implement {nameof(SlashCommandContext)}. Since this command is being registered as a user context menu command, it's first parameter must inherit the {nameof(SlashCommandContext)} class."
        );
    internal static readonly Action<ILogger, string, Exception?> invalidParameterType =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2, "User Commands Startup"),
            "The second parameter of '{CommandName}' is not a DiscordUser or DiscordMember. Since this command is being registered as a user context menu command, it's second parameter must be a DiscordUser or a DiscordMember."
        );
    internal static readonly Action<
        ILogger,
        int,
        string,
        Exception?
    > invalidParameterMissingDefaultValue = LoggerMessage.Define<int, string>(
        LogLevel.Warning,
        new EventId(3, "User Commands Startup"),
        "Parameter {ParameterIndex} of '{CommandName}' does not have a default value. Since this command is being registered as a user context menu command, any additional parameters must have a default value."
    );
}
