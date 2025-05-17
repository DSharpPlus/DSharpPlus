using System;

namespace DSharpPlus.Commands.Exceptions;

public abstract class CommandsException : Exception
{
    protected CommandsException() { }
    protected CommandsException(string? message) : base(message) { }
    protected CommandsException(string? message, Exception? innerException) : base(message, innerException) { }
}
