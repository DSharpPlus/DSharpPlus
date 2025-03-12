using System.Collections.Generic;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// Represents a single node on a command tree, of any type.
/// </summary>
public interface ICommandNode
{
    /// <summary>
    /// The name of this node. For overload nodes, this is a mangled name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Aliases acceptable for this node. This is not respected for application commands.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>
    /// The lowercased primary name according to the current naming policy.
    /// </summary>
    public string LowercasedName { get; }

    /// <summary>
    /// A description of this node. For overload nodes, this is a human-readable representation of its parameters.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The parent node of this node. Null if this is a top-level node.
    /// </summary>
    public ICommandNode? Parent { get; }

    /// <summary>
    /// The child nodes of this node. For overload nodes, this is empty.
    /// </summary>
    public IReadOnlyList<ICommandNode> Children { get; }

    /// <summary>
    /// A collection of custom metadata for this command.
    /// </summary>
    public NodeMetadataCollection Metadata { get; }
}
