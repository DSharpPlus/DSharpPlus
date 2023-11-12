namespace DSharpPlus.CommandAll.Processors.TextCommands;

using DSharpPlus.CommandAll.Processors.TextCommands.Parsing;

public record TextCommandConfiguration
{
    public ResolvePrefixDelegateAsync PrefixResolver { get; init; } = new DefaultPrefixResolver("!").ResolvePrefixAsync;
    public TextArgumentSplicer TextArgumentSplicer { get; init; } = DefaultTextArgumentSplicer.Splice;
    public char[] QuoteCharacters { get; init; } = ['"', '\'', '«', '»', '‘', '“', '„', '‟'];
    public bool IgnoreBots { get; init; } = true;
}
