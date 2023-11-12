using System;
using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.Exceptions;

public sealed class ArgumentParseException : Exception
{
    public CommandParameter Parameter { get; init; }

    public ArgumentParseException(CommandParameter parameter, Exception? innerException = null, string? message = null) : base(message ?? $"Failed to parse {parameter.Name}.", innerException)
    {
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));
        Parameter = parameter;
    }
}
