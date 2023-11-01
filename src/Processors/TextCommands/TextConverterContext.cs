using System;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.Processors.TextCommands.Parsing;

namespace DSharpPlus.CommandAll.Processors.TextCommands
{
    public record TextConverterContext : ConverterContext
    {
        public required TextArgumentSplicer Splicer { get; init; }
        public required string TextArguments { get; init; }
        public string CurrentTextArgument { get; private set; } = string.Empty;
        public int TextIndex { get; private set; }

        public bool NextTextArgument()
        {
            if (TextIndex >= TextArguments.Length || TextIndex == -1)
            {
                return false;
            }

            TextIndex = Splicer(Extension, TextArguments, TextIndex, out ReadOnlySpan<char> argument);
            CurrentTextArgument = argument.ToString();
            return TextIndex != -1;
        }
    }
}
