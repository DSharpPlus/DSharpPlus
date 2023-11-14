namespace DSharpPlus.Commands.Processors.TextCommands;

using System;
using Microsoft.Extensions.Logging;

internal static class TextLogging
{
    // Startup logs
    public static readonly Action<ILogger, Exception?> MissingMessageContentIntent = LoggerMessage.Define(LogLevel.Warning, new EventId(1, "Text Commands Startup"), "To make the bot work properly with command prefixes, the MessageContents intent needs to be enabled in your DiscordClientConfiguration. Without this intent, commands invoked through prefixes will not function, and the bot will only respond to mentions and DMs. Please ensure that the MessageContents intent is enabled in your configuration. To suppress this warning, set 'CommandsConfiguration.SuppressMissingMessageContentIntentWarning' to true.");
}
