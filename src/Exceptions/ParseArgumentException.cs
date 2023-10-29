using System;
using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.Exceptions
{
    public class ParseArgumentException(CommandArgument Argument, Exception? innerException) : Exception($"Failed to parse {Argument.Name}.", innerException);
}
