using System;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors;

internal static class BaseCommandLogging
{
    // Startup logs
    internal static readonly Action<ILogger, string, string, Exception?> invalidArgumentConverterImplementation = LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(1, "Command Processor Startup"), "Argument Converter {FullName} does not implement {InterfaceFullName}");
    internal static readonly Action<ILogger, string, string, string, string, Exception?> invalidEnumConverterImplementation = LoggerMessage.Define<string, string, string, string>(LogLevel.Error, new EventId(1, "Command Processor Startup"), "'{GenericEnumConverterFullName}' does not implement '{TConverterFullName}' and cannot be used. Please ensure the command processor '{CommandProcessor}' overrides '{NameOfAddEnumConverters}' and provides it's own generic enum converter. Currently, any commands with enum parameters will NOT be registered.");
    internal static readonly Action<ILogger, string, string, string, Exception?> duplicateArgumentConvertersRegistered = LoggerMessage.Define<string, string, string>(LogLevel.Warning, new EventId(1, "Command Processor Startup"), "Failed to add converter {ConverterFullName} because a converter for type {ParameterType} already exists: {ExistingConverter}");
    internal static readonly Action<ILogger, string, Exception?> failedConverterCreation = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1), "Failed to create instance of converter '{FullName}' due to a lack of empty public constructors, lack of a service provider, or lack of services within the service provider.");
}
