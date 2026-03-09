namespace DSharpPlus.Voice.Protocol.RTCP.Payloads;

/// <summary>
/// A particular item within a source description.
/// </summary>
internal sealed record SourceDescriptionItem
{
    /// <summary>
    /// The type of this item.
    /// </summary>
    public required SourceDescriptionItemType Type { get; init; }

    /// <summary>
    /// The name of this item. For a type of <see cref="SourceDescriptionItemType.PrivateExtension"/>, this is specified
    /// in the item, otherwise it is derived from the type.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The value of this description item. "EXPUNGED" for item types DSharpPlus.Voice chooses not to support.
    /// </summary>
    public required string Value { get; init; }
}
