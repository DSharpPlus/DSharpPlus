using DSharpPlus.CommandAll.Processors.TextCommands.Parsing;

namespace DSharpPlus.CommandAll.Processors.TextCommands
{
    public record TextCommandConfiguration
    {
        public ResolvePrefixDelegateAsync PrefixResolver { get; init; } = new DefaultPrefixResolver("!").ResolvePrefixAsync;
        public TextArgumentSplicer TextArgumentSplicer { get; init; } = DefaultTextArgumentSplicer.Splice;
        public bool IgnoreBots { get; init; } = true;
    }
}
