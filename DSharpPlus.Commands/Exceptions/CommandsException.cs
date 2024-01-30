namespace DSharpPlus.Commands.Exceptions;

using System;

/// <summary>
/// A template for command exceptions.
/// </summary>
public abstract class CommandsException : Exception
{
    protected CommandsException() { }
    protected CommandsException(string? message) : base(message) { }
    protected CommandsException(string? message, Exception? innerException) : base(message, innerException) { }
}
