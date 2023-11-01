using System;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.CommandAll.Processors.TextCommands
{
    internal static class TextLogging
    {
        // Startup logs
        public static readonly Action<ILogger, string, Exception?> FailedConverterCreation = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, "Text Commands Startup"), "Failed to create instance of converter '{FullName}' due to a lack of empty public constructors, lack of a service provider, or lack of services within the service provider.");
        public static readonly Action<ILogger, int, Exception?> RegisteredCommands = LoggerMessage.Define<int>(LogLevel.Information, new EventId(1, "Text Commands Startup"), "Registered {CommandCount:N0} text commands.");

        // Runtime logs
        public static readonly Action<ILogger, InteractionType, Exception?> UnknownInteractionType = LoggerMessage.Define<InteractionType>(LogLevel.Trace, new EventId(2, "Text Commands Runtime"), "Received interaction of type '{InteractionType}.' Ignoring.");
        public static readonly Action<ILogger, string, Exception?> UnknownCommandName = LoggerMessage.Define<string>(LogLevel.Trace, new EventId(2, "Text Commands Runtime"), "Received interaction for command '{CommandName}' but no matching command id was found. Was this command for a different shard or process?");
    }
}
