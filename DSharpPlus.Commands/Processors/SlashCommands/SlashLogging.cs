using System;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors.SlashCommands;

internal static class SlashLogging
{
    // Startup logs
    internal static readonly Action<ILogger, int, int, Exception?> registeredCommands = LoggerMessage.Define<int, int>(LogLevel.Information, new EventId(1, "Slash Commands Startup"), "Registered {TopLevelCommandCount:N0} top-level slash commands, {TotalCommandCount:N0} total slash commands.");
    internal static readonly Action<ILogger, Exception?> interactionReceivedBeforeConfigured = LoggerMessage.Define(LogLevel.Warning, new EventId(2, "Slash Commands Startup"), "Received an interaction before the slash commands processor was configured. This interaction will be ignored.");
    internal static readonly Action<ILogger, string, Exception?> unknownCommandName = LoggerMessage.Define<string>(LogLevel.Trace, new EventId(1, "Slash Commands Runtime"), "Received Command '{CommandName}' but no matching local command was found. Was this command for a different process?");
}
