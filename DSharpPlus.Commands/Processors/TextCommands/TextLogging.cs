using System;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors.TextCommands;

internal static class TextLogging
{
    // Startup logs
    internal static readonly Action<ILogger, DiscordIntents, Exception?> missingRequiredIntents = LoggerMessage.Define<DiscordIntents>(LogLevel.Error, new EventId(0, "Text Commands Startup"), "To make the bot work properly with text commands, the following intents need to be enabled in your DiscordClientConfiguration: {Intents}. Without these intents, text commands will not function AT ALL. Please ensure that the intents are enabled in your Discord configuration.");
    internal static readonly Action<ILogger, Exception?> missingMessageContentIntent = LoggerMessage.Define(LogLevel.Warning, new EventId(1, "Text Commands Startup"), "To make the bot work properly with command prefixes, the MessageContents intent needs to be enabled in your DiscordClientConfiguration. Without this intent, commands invoked through prefixes will not function, and the bot will only respond to mentions and DMs. Please ensure that the MessageContents intent is enabled in your configuration. To suppress this warning, set 'CommandsConfiguration.SuppressMissingMessageContentIntentWarning' to true.");
}
