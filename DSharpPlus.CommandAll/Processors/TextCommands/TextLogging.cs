namespace DSharpPlus.CommandAll.Processors.TextCommands;

using System;
using Microsoft.Extensions.Logging;

internal static class TextLogging
{
    // Startup logs
    public static readonly Action<ILogger, Exception?> MissingMessageContentIntent = LoggerMessage.Define(LogLevel.Critical, new EventId(1, "Text Commands Startup"), "TextCommandProcessor requires the MessageContents intent to be enabled. Please enable this intent in your DiscordClientConfiguration.");
}
