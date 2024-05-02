using System;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors.SlashCommands;

internal static class SlashLogging
{
    // Startup logs
    public static readonly Action<ILogger, int, Exception?> RegisteredCommands = LoggerMessage.Define<int>(LogLevel.Information, new EventId(1, "Slash Commands Startup"), "Registered {CommandCount:N0} slash commands.");
    public static readonly Action<ILogger, string, Exception?> UnknownCommandName = LoggerMessage.Define<string>(LogLevel.Trace, new EventId(1, "Slash Commands Runtime"), "Received Command '{CommandName}' but no matching local command was found. Was this command for a different process?");
}
