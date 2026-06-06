using System;

namespace DSharpPlus.Voice.AudioWriters.Ogg;

/// <summary>
/// Specifies the type of page we have on our hands.
/// </summary>
[Flags]
internal enum OggHeaderTypeFlags : byte
{
    /// <summary>
    /// Indicates whether this page's first packet starts at the page boundary or is continued from the last page.
    /// </summary>
    ContinuedPacket = 1 << 0,

    /// <summary>
    /// Indicates whether this is the first page of this particular logical bitstream.
    /// </summary>
    LogicalFirstPage = 1 << 1,

    /// <summary>
    /// Indicates whether this is the last page of this particular logical bitstream.
    /// </summary>
    LogicalLastPage = 1 << 2
}
