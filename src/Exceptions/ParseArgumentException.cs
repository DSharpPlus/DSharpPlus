using System;
using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.Exceptions
{
    public sealed class ParseArgumentException : Exception
    {
        public CommandArgument Argument { get; init; }

        public ParseArgumentException(CommandArgument argument, Exception? innerException = null, string? message = null) : base(message ?? $"Failed to parse {argument.Name}.", innerException)
        {
            ArgumentNullException.ThrowIfNull(argument, nameof(argument));
            Argument = argument;
        }
    }
}
