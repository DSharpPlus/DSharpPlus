namespace DSharpPlus.CommandAll.Processors.SlashCommands;
using System;
using Microsoft.Extensions.Logging;

internal static class SlashLogging
{
    // Startup logs
    public static readonly Action<ILogger, int, Exception?> RegisteredCommands = LoggerMessage.Define<int>(LogLevel.Information, new EventId(1, "Slash Commands Startup"), "Registered {CommandCount:N0} slash commands.");

    // Runtime logs
    public static readonly Action<ILogger, string, Exception?> UnknownCommandName = LoggerMessage.Define<string>(LogLevel.Trace, new EventId(2, "Slash Commands Runtime"), "Received interaction for command '{CommandName}' but no matching command id was found. Was this command for a different shard or process?");
}
