using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Commands.ContextChecks;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// Represents an overload of a command. Overloads may be distinguished by predicates or parameter list. No two overloads of the same
/// executable node may match in both predicates and parameter list.
/// </summary>
public sealed class CommandOverload : ICommandNode
{
    /// <summary>
    /// This will be a mangled name used for diagnostics rather than an actual user-visible name. Refer to <c>Parent.Name</c> for this node's name.
    /// </summary>
    public required string Name { get; init; }

    /// <inheritdoc/>
    IReadOnlyList<string> ICommandNode.Aliases => [];

    /// <inheritdoc/>
    string ICommandNode.LowercasedName => "";

    /// <inheritdoc/>
    string ICommandNode.Description => "Overload";

    /// <inheritdoc/>
    IReadOnlyList<ICommandNode> ICommandNode.Children => [];

    /// <summary>
    /// The parameters of this command, excluding the command context.
    /// </summary>
    public required IReadOnlyList<ParameterNode> Parameters { get; init; }

    /// <summary>
    /// The context type of this command. This is a valid overloading distinction.
    /// </summary>
    public required Type ContextType { get; init; }

    /// <summary>
    /// A list of types of handlers allowed to handle this command. If this list is empty, any handler may decide on its own.
    /// </summary>
    public required IReadOnlyList<Type> AllowedHandlers { get; init; }

    /// <summary>
    /// Specifies this overload as the canonical overload in cases where there is no true overloading support.
    /// </summary>
    public required bool IsCanonicalOverload { get; init; }

    /// <summary>
    /// The executing function for this command.
    /// </summary>
    public required Func<CommandContext, object?[], IServiceProvider, ValueTask> Execute { get; init; }

    /// <summary>
    /// The check attributes applicable to this command. This does not necessarily correlate to the list of executed checks,
    /// which may vary based on context and registered check implementations.
    /// </summary>
    public required IReadOnlyList<ContextCheckAttribute> CheckAttributes { get; init; }

    /// <inheritdoc/>
    public required NodeMetadataCollection Metadata { get; init; }
}
