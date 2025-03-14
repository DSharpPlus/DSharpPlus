namespace DSharpPlus.Commands.Trees.Metadata;

/// <summary>
/// Indicates whether the current parameter can or is required to be sourced from a reply.
/// </summary>
public sealed class ReplyMetadata : INodeMetadataItem
{
    /// <summary>
    /// Indicates whether the parameter is allowed to be sourced from a reply.
    /// </summary>
    public required bool IsAllowed { get; init; }

    /// <summary>
    /// Indicates whether the parameter is required to be sourced from a reply.
    /// </summary>
    public required bool IsRequired { get; init; }
}
