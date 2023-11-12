namespace DSharpPlus.Commands.Processors;

using System;
using Microsoft.Extensions.Logging;

internal static class BaseCommandLogging
{
    // Startup logs
    public static readonly Action<ILogger, string, string, Exception?> InvalidArgumentConverterImplementation = LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(1, "Command Processor Startup"), "Argument Converter {FullName} does not implement {InterfaceFullName}");
    public static readonly Action<ILogger, string, string, string, Exception?> DuplicateArgumentConvertersRegistered = LoggerMessage.Define<string, string, string>(LogLevel.Warning, new EventId(1, "Command Processor Startup"), "Failed to add converter {ConverterFullName} because a converter for type {ParameterType} already exists: {ExistingConverter}");
}
