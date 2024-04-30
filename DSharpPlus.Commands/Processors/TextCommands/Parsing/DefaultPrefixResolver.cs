namespace DSharpPlus.Commands.Processors.TextCommands.Parsing;

using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

public delegate ValueTask<int> ResolvePrefixDelegateAsync(CommandsExtension extension, DiscordMessage message);

public sealed class DefaultPrefixResolver
{
    public string Prefix { get; init; }

    public DefaultPrefixResolver(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentException("Prefix cannot be null or whitespace.", nameof(prefix));
        }

        Prefix = prefix;
    }

    public ValueTask<int> ResolvePrefixAsync(CommandsExtension extension, DiscordMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Content))
        {
            return ValueTask.FromResult(-1);
        }
        else if (message.Content.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult(Prefix.Length);
        }
        // Mention check
        else if (message.Content.StartsWith(extension.Client.CurrentUser.Mention, StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult(extension.Client.CurrentUser.Mention.Length);
        }

        return ValueTask.FromResult(-1);
    }
}
