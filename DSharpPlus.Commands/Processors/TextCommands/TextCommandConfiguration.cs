using System;
using System.Collections.Generic;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;

namespace DSharpPlus.Commands.Processors.TextCommands;

public record TextCommandConfiguration
{
    /// <summary>
    /// The function to use to resolve prefixes for commands.
    /// </summary>
    /// <remarks>For dynamic prefix resolving, <see cref="IPrefixResolver"/> registered to the <see cref="DiscordClient"/>'s <see cref="IServiceProvider"/> should be preferred.</remarks>
    public ResolvePrefixDelegateAsync PrefixResolver { get; init; } = new DefaultPrefixResolver(true, "!").ResolvePrefixAsync;
    public TextArgumentSplicer TextArgumentSplicer { get; init; } = DefaultTextArgumentSplicer.Splice;
    public char[] QuoteCharacters { get; init; } = ['"', '\'', '«', '»', '‘', '“', '„', '‟'];
    public bool IgnoreBots { get; init; } = true;

    /// <summary>
    /// Disables the exception thrown when a command is not found.
    /// </summary>
    public bool EnableCommandNotFoundException { get; init; }

    /// <summary>
    /// Whether to suppress the missing message content intent warning.
    /// </summary>
    public bool SuppressMissingMessageContentIntentWarning { get; set; }

    /// <summary>
    /// Whether commands should rerun whenever a message is edited/updated.
    /// </summary>
    public bool EnableEditCommands { get; set; }

    public IEqualityComparer<string> CommandNameComparer { get; init; } = StringComparer.OrdinalIgnoreCase;
}
