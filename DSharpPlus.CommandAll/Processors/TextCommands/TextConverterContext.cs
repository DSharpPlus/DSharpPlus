namespace DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.Processors.TextCommands.Parsing;

public record TextConverterContext : ConverterContext
{
    public required TextArgumentSplicer Splicer { get; init; }
    public required string RawArguments { get; init; }
    public new string Argument => (string)(base.Argument ?? string.Empty);
    public int CurrentArgumentIndex { get; private set; }
    public int NextArgumentIndex { get; private set; }

    public override bool NextArgument()
    {
        if (this.NextArgumentIndex >= this.RawArguments.Length || this.NextArgumentIndex == -1)
        {
            return false;
        }

        this.CurrentArgumentIndex = this.NextArgumentIndex;
        int nextTextIndex = this.NextArgumentIndex;
        string? nextText = this.Splicer(this.Extension, this.RawArguments, ref nextTextIndex);
        if (string.IsNullOrEmpty(nextText))
        {
            base.Argument = string.Empty;
            return false;
        }

        this.NextArgumentIndex = nextTextIndex;
        base.Argument = nextText;
        return true;
    }
}
