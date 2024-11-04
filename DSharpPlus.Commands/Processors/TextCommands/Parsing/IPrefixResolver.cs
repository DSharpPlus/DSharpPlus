using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.TextCommands.Parsing;

/// <summary>
/// Represents a resolver for command prefixes.
/// </summary>
public interface IPrefixResolver
{
    /// <summary>
    /// Resolves the prefix for the command.
    /// </summary>
    /// <param name="extension">The commands extension.</param>
    /// <param name="message">The message to resolve the prefix for.</param>
    /// <returns>An integer representing the length of the prefix.</returns>
    public ValueTask<int> ResolvePrefixAsync(CommandsExtension extension, DiscordMessage message);
}
