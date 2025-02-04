using System.Collections.Generic;

using DSharpPlus.Commands.Trees.Predicates;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// Represents the root node of a command tree. This specifies the tree's namespace and scoping.
/// </summary>
public class CommandTreeRoot
{
    /// <summary>
    /// The name of this command tree. This will be <c>dsharpplus-commands{-id}</c> for all default trees.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// The guild ID this tree is specific to, null if it's global.
    /// </summary>
    public ulong? GuildId { get; internal set; }

    /// <summary>
    /// The top level command nodes managed by this tree.
    /// </summary>
    public IReadOnlyList<ICommandNode> TopLevelCommands { get; internal set; }

    /// <summary>
    /// A collection of predicates for this tree being considerable at all.
    /// </summary>
    public IReadOnlyList<ICommandExecutionPredicate> Predicates { get; internal set; }
}
