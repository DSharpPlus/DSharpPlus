
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;

namespace DSharpPlus.Commands.Processors.TextCommands;
public record TextConverterContext : ConverterContext
{
    public required TextArgumentSplicer Splicer { get; init; }
    public required string RawArguments { get; init; }
    public new string Argument => (string)(base.Argument ?? string.Empty);
    public int CurrentArgumentIndex { get; private set; }
    public int NextArgumentIndex { get; private set; }

    public override bool NextArgument()
    {
        if (NextArgumentIndex >= RawArguments.Length || NextArgumentIndex == -1)
        {
            return false;
        }

        CurrentArgumentIndex = NextArgumentIndex;
        int nextTextIndex = NextArgumentIndex;
        string? nextText = Splicer(Extension, RawArguments, ref nextTextIndex);
        if (string.IsNullOrEmpty(nextText))
        {
            base.Argument = string.Empty;
            return false;
        }

        NextArgumentIndex = nextTextIndex;
        base.Argument = nextText;
        return true;
    }
}
