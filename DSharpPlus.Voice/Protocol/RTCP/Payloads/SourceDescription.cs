using System.Collections.Generic;

namespace DSharpPlus.Voice.Protocol.RTCP.Payloads;

/// <summary>
/// Contains description about a call participant.
/// </summary>
internal sealed record SourceDescription
{
    /// <summary>
    /// The SSRC of this participant.
    /// </summary>
    public required uint SSRC { get; init; }

    /// <summary>
    /// The description items contained.
    /// </summary>
    public required IReadOnlyList<SourceDescriptionItem> DescriptionItems { get; init; }
}
