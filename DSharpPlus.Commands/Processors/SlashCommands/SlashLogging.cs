using System;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors.SlashCommands;

internal static class SlashLogging
{
    // Startup logs
    internal static readonly Action<ILogger, int, Exception?> registeredCommands = LoggerMessage.Define<int>(LogLevel.Information, new EventId(1, "Slash Commands Startup"), "Registered {CommandCount:N0} slash commands.");
    internal static readonly Action<ILogger, string, Exception?> unknownCommandName = LoggerMessage.Define<string>(LogLevel.Trace, new EventId(1, "Slash Commands Runtime"), "Received Command '{CommandName}' but no matching local command was found. Was this command for a different process?");
}
