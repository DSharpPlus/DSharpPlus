using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace DSharpPlus.Voice.AudioWriters.Ogg;

/// <summary>
/// Contains information about one ogg page.
/// </summary>
internal record struct OggPageInfo
{
    /// <summary>
    /// Gets the lengths of the opus packets contained on this page, in bytes.
    /// </summary>
    public IReadOnlyList<int> PacketLengths { get; private set; }

    /// <summary>
    /// A range specifying the header of a page.
    /// </summary>
    public Range Header { get; private set; }

    /// <summary>
    /// A range specifying the body of a page.
    /// </summary>
    public Range Body { get; private set;}

    /// <summary>
    /// A range specifying the location of the CRC32 checksum of the page.
    /// </summary>
    public Range CRC32 { get; private set; }

    /// <summary>
    /// The granule position of the first sample of audio in this page.
    /// </summary>
    public ulong GranulePosition { get; private set; }

    /// <summary>
    /// The serial number of this logical bitstream.
    /// </summary>
    public uint BitstreamSerialNumber { get; private set; }

    /// <summary>
    /// The sequence number of this page.
    /// </summary>
    public uint SequenceNumber { get; private set; }

    /// <summary>
    /// The checksum this page claims to have.
    /// </summary>
    public uint ClaimedChecksum { get; private set; }

    /// <summary>
    /// The total length of this page, in bytes.
    /// </summary>
    public int TotalPageLength { get; private set; }

    /// <summary>
    /// The amount of OGG segments in this page.
    /// </summary>
    public byte Segments { get; private set; }

    /// <summary>
    /// The format version of this page.
    /// </summary>
    public byte Version { get; private set; }

    /// <summary>
    /// Flags providing further information about this page.
    /// </summary>
    public OggHeaderTypeFlags Flags { get; private set; }

    /// <summary>
    /// Indicates whether the last packet of this page continues on the next page.
    /// </summary>
    public bool LastPacketContinuesOnNextPage { get; private set; }

    /// <summary>
    /// Attempts to parse info from this page.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<byte> page, ref OggPageInfo info)
    {
        if (!page.StartsWith("OggS"u8))
        {
            return false;
        }

        // first check whether we have a segments byte to begin with, then check whether we have enough bytes
        // the logical operator || is short-circuiting, so we write it this way to avoid an exception
        // it is after all a TryParse method
        if (page.Length < 27 || page.Length < 27 + page[26])
        {
            return false;
        }

        byte version = page[4];

        if (version != 0)
        {
            return false;
        }

        OggHeaderTypeFlags flags = (OggHeaderTypeFlags)page[5];
        ulong granulePosition = BinaryPrimitives.ReadUInt64LittleEndian(page[6..]);
        uint serial = BinaryPrimitives.ReadUInt32LittleEndian(page[14..]);
        uint sequence = BinaryPrimitives.ReadUInt32LittleEndian(page[18..]);
        uint checksum = BinaryPrimitives.ReadUInt32LittleEndian(page[22..]);
        byte segments = page[26];

        int currentPacketLength = 0;
        int totalPacketLengths = 0;
        List<int> packetLengths = [];

        for (int index = 27; index < 27 + segments; index++)
        {
            currentPacketLength += page[index];
            totalPacketLengths += page[index];

            if (page[index] == 255)
            {
                continue;
            }

            packetLengths.Add(currentPacketLength);
            currentPacketLength = 0;
        }

        info.PacketLengths = packetLengths;                                  
        info.Header = new(0, 27 + segments);
        info.Body = new(27 + segments, 27 + segments + totalPacketLengths);
        info.CRC32 = new(22, 26);
        info.GranulePosition = granulePosition;
        info.SequenceNumber = sequence;
        info.BitstreamSerialNumber = serial;
        info.ClaimedChecksum = checksum;
        info.TotalPageLength = 27 + segments + totalPacketLengths;
        info.Segments = segments;
        info.Version = version;
        info.Flags = flags;
        info.LastPacketContinuesOnNextPage = page[26 + segments] == 255;

        return true;
    }

    public static bool IsCompletePageHeader(ReadOnlySpan<byte> page)
        => page.StartsWith("OggS"u8) && page.Length >= 27 && page[4] == 0 && page.Length >= 27 + page[26];
}
