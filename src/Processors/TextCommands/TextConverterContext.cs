using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.Processors.TextCommands.Parsing;

namespace DSharpPlus.CommandAll.Processors.TextCommands
{
    public record TextConverterContext : ConverterContext
    {
        public required TextArgumentSplicer Splicer { get; init; }
        public required string RawArguments { get; init; }
        public string CurrentTextArgument { get; private set; } = string.Empty;
        public int NextTextIndex { get; private set; }

        public bool NextTextArgument()
        {
            if (NextTextIndex >= RawArguments.Length || NextTextIndex == -1)
            {
                return false;
            }

            int nextTextIndex = NextTextIndex;
            string? nextText = Splicer(Extension, RawArguments, ref nextTextIndex);
            if (string.IsNullOrEmpty(nextText))
            {
                CurrentTextArgument = string.Empty;
                return false;
            }

            NextTextIndex = nextTextIndex;
            CurrentTextArgument = nextText;
            return true;
        }
    }
}
