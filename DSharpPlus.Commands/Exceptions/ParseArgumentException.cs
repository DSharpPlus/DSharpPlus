
using System;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Exceptions;
public sealed class ArgumentParseException : CommandsException
{
    public CommandParameter Parameter { get; init; }

    public ArgumentParseException(CommandParameter parameter, Exception? innerException = null, string? message = null) : base(message ?? $"Failed to parse {parameter.Name}.", innerException)
    {
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));
        Parameter = parameter;
    }
}
