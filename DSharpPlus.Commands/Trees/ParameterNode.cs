using System;
using System.Collections.Generic;

using DSharpPlus.Commands.ContextChecks.ParameterChecks;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// Represents a single parameter to a command.
/// </summary>
public class ParameterNode
{
    /// <summary>
    /// The unaltered name of this parameter.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The lowercased name of this parameter.
    /// </summary>
    public required string LowercasedName { get; init; }

    /// <summary>
    /// The description of this parameter.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The type of this parameter.
    /// </summary>
    public required Type ParameterType { get; init; }

    /// <summary>
    /// Parameter check data attributes found on this parameter. This does not control which checks will be executed, which is handled by the extension.
    /// </summary>
    public required IReadOnlyList<ParameterCheckAttribute> CheckAttributes { get; init; }

    /// <summary>
    /// Indicates whether this parameter must be specified.
    /// </summary>
    public required bool IsRequired { get; init; }

    /// <summary>
    /// The default value of this parameter, if it was not required and not specified at invocation.
    /// </summary>
    /// <remarks>
    /// This value is undefined for required parameters and must not be relied upon.
    /// </remarks>
    public required object? DefaultValue { get; init; }

    /// <summary>
    /// Additional metadata associated with this command.
    /// </summary>
    public required NodeMetadataCollection Metadata { get; init; }
}
