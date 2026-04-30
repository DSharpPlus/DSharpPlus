#pragma warning disable IDE0040

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.RTCP.Payloads;

namespace DSharpPlus.Voice.Protocol.RTCP;

partial class RTCPSerializer
{
    private static void SerializeSourceDescription(RTCPSourceDescriptionPacket packet, ArrayPoolBufferWriter<byte> writer)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(packet.SourceDescriptions.Count, 31, "RTCPSenderReportPacket.SourceDescriptions.Count");

        // the first three bits are always constant, version 2 (10) and no padding (0).
        byte firstHeaderByte = (byte)(0b100_00000 | (packet.SourceDescriptions.Count & 0b000_11111));
        byte secondHeaderByte = (byte)RTCPPacketType.SourceDescription;

        // calculating the length is actually kind of awful, but
        ushort length = CalculateSerializedPacketLength(packet);

        Span<byte> headerSpan = writer.GetSpan(4);

        headerSpan[0] = firstHeaderByte;
        headerSpan[1] = secondHeaderByte;

        BinaryPrimitives.WriteUInt16BigEndian(headerSpan[2..], length);

        writer.Advance(4);

        foreach (SourceDescription description in packet.SourceDescriptions)
        {
            Span<byte> ssrcSpan = writer.GetSpan(4);
            BinaryPrimitives.WriteUInt32BigEndian(ssrcSpan, description.SSRC);
            writer.Advance(4);

            foreach (SourceDescriptionItem item in description.DescriptionItems)
            {
                Debug.Assert(item.Type is SourceDescriptionItemType.CanonicalName or SourceDescriptionItemType.DisplayName or SourceDescriptionItemType.ApplicationName);

                int valueByteCount = Encoding.UTF8.GetByteCount(item.Value);
                int nameByteCount = Encoding.UTF8.GetByteCount(item.Name);

                ArgumentOutOfRangeException.ThrowIfGreaterThan(valueByteCount, 255, "SourceDescriptionItem.Value.Length");
                ArgumentOutOfRangeException.ThrowIfGreaterThan(nameByteCount, 255, "SourceDescriptionItem.Name.Length");
                
                writer.Write((byte)item.Type);
                writer.Write((byte)item.Value.Length);

                if (item.Type == SourceDescriptionItemType.PrivateExtension)
                {
                    writer.Write((byte)item.Name.Length);

                    Span<byte> nameSpan = writer.GetSpan(nameByteCount);
                    Encoding.UTF8.GetBytes(item.Name, nameSpan);
                    writer.Advance(nameByteCount);
                }

                Span<byte> valueSpan = writer.GetSpan(valueByteCount);
                Encoding.UTF8.GetBytes(item.Value, valueSpan);
                writer.Advance(valueByteCount);
            }

            // we wrote the whole block, now we align it to four bytes
            // it's fine if we're already aligned, it just won't do anything
            int written = writer.WrittenCount;
            int target = (written + 3) & ~0b11;
            int diff = target - written;

            Span<byte> padding = writer.GetSpan(diff);
            padding.Clear();
            writer.Advance(diff);
        }
    }

    private static RTCPSourceDescriptionPacket DeserializeSourceDescription(ReadOnlySpan<byte> packet, out int consumed)
    {
        Debug.Assert(packet[1] == (byte)RTCPPacketType.SourceDescription);
        ArgumentOutOfRangeException.ThrowIfNotEqual(packet[0] & 0b11000000, 0b10000000, "RTCP packet version");

        bool hasPadding = (packet[0] & 0b00100000) == 0b00100000;
        ushort length = BinaryPrimitives.ReadUInt16BigEndian(packet[2..]);
        int descriptionCount = packet[0] & 0b00011111;

        List<SourceDescription> descriptions = new(descriptionCount);

        consumed = 4;

        for (int i = 0; i < descriptionCount; i++)
        {
            uint ssrc = BinaryPrimitives.ReadUInt32BigEndian(packet[consumed..]);
            consumed += 4;

            List<SourceDescriptionItem> items = [];

            // i don't really know how we're supposed to know that the next item is no longer a source description item
            // but part of the next block, considering 1 through 8 are technically valid first SSRC bytes, but we'll
            // just....... assume that if the next byte is not a valid source description type identifier, it's probably
            // a SSRC (or padding) and move on to the next block of description counts. luckily for us, i don't think 
            // there's ever a situation where we actually have to deal with multiple source description blocks, which
            // RFC 3550 specifies as happening when dealing with mixers in the connection, which we never do.

            while (packet[consumed] is >= 1 and <= 8)
            {
                SourceDescriptionItemType type = (SourceDescriptionItemType)packet[consumed];
                byte valueLength = packet[++consumed];
                string name = "";

                if (type == SourceDescriptionItemType.PrivateExtension)
                {
                    byte nameLength = packet[++consumed];
                    ReadOnlySpan<byte> nameSpan = packet[consumed..(consumed + nameLength)];
                    name = Encoding.UTF8.GetString(nameSpan);
                    consumed += nameLength;
                }

                ReadOnlySpan<byte> valueSpan = packet[consumed..(consumed + valueLength)];
                string value = Encoding.UTF8.GetString(valueSpan);
                consumed += valueLength;

                name = type switch
                {
                    SourceDescriptionItemType.CanonicalName => "Canonical Name",
                    SourceDescriptionItemType.DisplayName => "Display Name",
                    SourceDescriptionItemType.Email => "Email",
                    SourceDescriptionItemType.PhoneNumber => "Phone Number",
                    SourceDescriptionItemType.GeographicLocation => "Geographic Location",
                    SourceDescriptionItemType.ApplicationName => "Application Name",
                    SourceDescriptionItemType.Notice => "Notice",
                    SourceDescriptionItemType.PrivateExtension => name,
                    _ => throw new UnreachableException()
                };

                value = type switch
                {
                    SourceDescriptionItemType.CanonicalName => value,
                    SourceDescriptionItemType.DisplayName => value,
                    SourceDescriptionItemType.Email => "EXPUNGED",
                    SourceDescriptionItemType.PhoneNumber => "EXPUNGED",
                    SourceDescriptionItemType.GeographicLocation => "EXPUNGED",
                    SourceDescriptionItemType.ApplicationName => value,
                    SourceDescriptionItemType.Notice => "EXPUNGED",
                    SourceDescriptionItemType.PrivateExtension => value,
                    _ => throw new UnreachableException()
                };

                items.Add(new()
                {
                    Type = type,
                    Name = name,
                    Value = value
                });
            }

            descriptions.Add(new()
            {
                SSRC = ssrc,
                DescriptionItems = items
            });

            // we reached the end of an item block, pad
            consumed = (consumed + 3) & ~0b11;
        }

        if (!hasPadding)
        {
            Debug.Assert(consumed == (length + 1) * 4);
        }

        consumed = (length + 1) * 4;

        return new()
        {
            SourceDescriptions = descriptions
        };
    }

    private static ushort CalculateSerializedPacketLength(RTCPSourceDescriptionPacket packet)
    {
        int length = 0;

        foreach (SourceDescription description in packet.SourceDescriptions)
        {
            // the SSRC word
            length += 4;

            foreach (SourceDescriptionItem item in description.DescriptionItems)
            {
                // type
                length += 1;

                if (item.Type == SourceDescriptionItemType.PrivateExtension)
                {
                    // name length
                    length += 1;
                    length += item.Name.Length;
                }

                // description length
                length += 1;
                length += item.Value.Length;
            }

            // each source description block must begin at a multiple of four bytes
            // account for that padding
            length = (length + 3) & ~0b11;
        }

        Debug.Assert(length % 4 == 0);
        return (ushort)((length / 4) - 1);
    }
}
