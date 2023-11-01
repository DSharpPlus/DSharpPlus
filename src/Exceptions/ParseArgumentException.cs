using System;
using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.Exceptions
{
    public sealed class ParseArgumentException(CommandArgument Argument, Exception? innerException = null, string? message = null) : Exception(message ?? $"Failed to parse {Argument.Name}.", innerException);
}
