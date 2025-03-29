using System.Collections.Generic;
using System.Linq;

using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees.Metadata;

namespace DSharpPlus.Commands.Trees;

public sealed class CommandNodeBuilder
{
    private readonly List<string> aliases;
    private readonly List<CommandNodeBuilder> children;
    private readonly List<CommandOverloadBuilder> overloads;
    private readonly List<INodeMetadataItem> metadataItems = [];
    private readonly List<ContextCheckAttribute> checks;

    /// <inheritdoc/>
    public string? Name { get; internal set; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Aliases => this.aliases;

    /// <inheritdoc/>
    public string? LowercasedName { get; internal set; }

    /// <inheritdoc/>
    public string? Description { get; internal set; }

    /// <inheritdoc/>
    /// <remarks>
    /// This is guaranteed to be an <see cref="ExecutableCommandNode"/>.
    /// </remarks>
    public CommandNodeBuilder? Parent { get; internal set; }

    /// <summary>
    /// The children of this command node.
    /// </summary>
    public IReadOnlyList<CommandNodeBuilder> Children => this.children;

    /// <summary>
    /// The check attributes applicable to this command. This does not necessarily correlate to the list of executed checks,
    /// which may vary based on context and registered check implementations.
    /// </summary>
    // this is not actually respected on command nodes, but we'll collect it here and apply to the actual overload nodes at build time
    public IReadOnlyList<ContextCheckAttribute> CheckAttributes => this.checks;

    /// <summary>
    /// Additional metadata associated with this command.
    /// </summary>
    public NodeMetadataCollection Metadata => new([.. this.metadataItems.DistinctBy(x => x.GetType())]);
}
