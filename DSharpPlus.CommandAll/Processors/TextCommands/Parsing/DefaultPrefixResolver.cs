namespace DSharpPlus.CommandAll.Processors.TextCommands.Parsing;
using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

public delegate Task<int> ResolvePrefixDelegateAsync(CommandAllExtension extension, DiscordMessage message);

public sealed class DefaultPrefixResolver
{
    public string Prefix { get; init; }

    public DefaultPrefixResolver(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentException("Prefix cannot be null or whitespace.", nameof(prefix));
        }

        this.Prefix = prefix;
    }

    public Task<int> ResolvePrefixAsync(CommandAllExtension extension, DiscordMessage message)
    {
        if (message.Content.StartsWith(this.Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(this.Prefix.Length);
        }
        // Mention check
        else if (message.Content.StartsWith(extension.Client.CurrentUser.Mention, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(extension.Client.CurrentUser.Mention.Length);
        }

        return Task.FromResult(-1);
    }
}
