using System;
using System.Collections.Generic;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;

namespace DSharpPlus.Commands.Processors.TextCommands;

public record TextCommandConfiguration
{
    public ResolvePrefixDelegateAsync PrefixResolver { get; init; } = new DefaultPrefixResolver(true,"!").ResolvePrefixAsync;
    public TextArgumentSplicer TextArgumentSplicer { get; init; } = DefaultTextArgumentSplicer.Splice;
    public char[] QuoteCharacters { get; init; } = ['"', '\'', '«', '»', '‘', '“', '„', '‟'];
    public bool IgnoreBots { get; init; } = true;

    /// <summary>
    /// Whether to suppress the missing message content intent warning.
    /// </summary>
    public bool SuppressMissingMessageContentIntentWarning { get; set; }
    
    public IEqualityComparer<string> CommandNameComparer { get; init; } = StringComparer.OrdinalIgnoreCase;
}
