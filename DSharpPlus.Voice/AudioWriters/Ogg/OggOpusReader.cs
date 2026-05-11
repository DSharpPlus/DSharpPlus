using System;
using System.IO;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Exceptions;
using DSharpPlus.Voice.MemoryServices;
using DSharpPlus.Voice.MemoryServices.Channels;
using DSharpPlus.Voice.MemoryServices.Collections;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.AudioWriters.Ogg;

/// <summary>
/// Provides a mechanism to read ogg/opus streams from a Pipe.
/// </summary>
internal sealed class OggOpusReader
{
    private readonly AudioChannelWriter packetWriter;
    private readonly ILogger<OggOpusReader> logger;
    private ManualResetAccumulatingBuffer pageBuffer = new(65307);

    private uint currentStreamSerial = 0;
    private bool lastPacketContinuesOnNextPage;
    private ManualResetAccumulatingBuffer packetBuffer = new(1275);
    private TimeSpan remainingPreSkipTime;

    public OggOpusReader(AudioChannelWriter packetWriter, ILogger<OggOpusReader> logger)
    {
        this.packetWriter = packetWriter;
        this.logger = logger;
    }

    public void Ingest(ReadOnlySpan<byte> data)
    {
        int consumed = 0;
        int inputLength = data.Length;
        OggPageInfo pageInfo = default;

        while (consumed < data.Length)
        {
            this.pageBuffer.Write(data[consumed..], out int consumedOnThisTurn);
            consumed += consumedOnThisTurn;

        ProcessPage:

            bool parsedPageInfo = ValidateAndExtractPageInfo(ref pageInfo);

            if (!parsedPageInfo || this.pageBuffer.WrittenCount < pageInfo.TotalPageLength)
            {
                // we need more data
                continue;
            }

            // we have a valid page. try to see whether it's an ID page first
            if (OggOpusIdentificationHeader.TryParse(this.pageBuffer.WrittenSpan[pageInfo.Body], out OggOpusIdentificationHeader? idHeader))
            {
                if (idHeader.MappingFamily != OggOpusChannelMappingFamily.Basic)
                {
                    throw new UnsupportedOggOpusStreamException($"Unsupported channel mapping {idHeader.MappingFamily}, only 0 is supported.");
                }

                this.logger.LogTrace("Encountered new ogg/opus stream with serial {serial}, switching.", pageInfo.BitstreamSerialNumber);

                this.currentStreamSerial = pageInfo.BitstreamSerialNumber;
                this.remainingPreSkipTime = TimeSpan.FromSeconds(idHeader.PreSkipSamples / 48000);
                this.lastPacketContinuesOnNextPage = false;

                if (idHeader.OutputGain != 0)
                {
                    this.logger.LogDebug("Encountered an ogg/opus stream with a non-zero output gain field. This will be ignored.");
                }

                SkipPage(pageInfo.TotalPageLength);
                goto ProcessPage;
            }
            // or maybe a comment page?
            else if (this.pageBuffer.WrittenSpan[pageInfo.Body].StartsWith("OpusTags"u8))
            {
                SkipPage(pageInfo.TotalPageLength);
                goto ProcessPage;
            }
            // or its a complete audio page?
            else if (this.pageBuffer.WrittenCount >= pageInfo.TotalPageLength)
            {
                if (!ValidatePage(pageInfo))
                {
                    this.logger.LogDebug("Received an ogg page with an invalid checksum. This page will be ignored.");
                    SkipPage(pageInfo.TotalPageLength);
                    goto ProcessPage;
                }

                ReadOnlySpan<byte> audio = this.pageBuffer.WrittenSpan[pageInfo.Body];
                int packetIndex = 0, byteOffset = 0;
                AudioBufferLease lease;

                if (this.lastPacketContinuesOnNextPage)
                {
                    packetIndex = 1;
                    byteOffset = pageInfo.PacketLengths[0];
                    this.packetBuffer.Write(audio[..byteOffset], out _);

                    TimeSpan packetLength = OpusCodec.CalculateOpusPacketLength(this.packetBuffer.WrittenSpan);

                    if (this.remainingPreSkipTime > packetLength)
                    {
                        this.remainingPreSkipTime -= packetLength;
                    }
                    else
                    {
                        this.remainingPreSkipTime = TimeSpan.Zero;

                        lease = AudioBufferManager.Shared.Rent(this.packetBuffer.WrittenCount);

                        this.packetBuffer.WrittenSpan.CopyTo(lease.Buffer);
                        lease.FrameCount = (int)packetLength.TotalMilliseconds / 20;

                        this.packetWriter.TryWrite(lease);
                    }
                }

                for (; packetIndex < pageInfo.PacketLengths.Count; packetIndex++)
                {
                    if (packetIndex == pageInfo.PacketLengths.Count - 1 && pageInfo.LastPacketContinuesOnNextPage)
                    {
                        this.packetBuffer.Reset();
                        this.packetBuffer.Write(audio[byteOffset..], out _);
                        break;
                    }

                    int packetLength = pageInfo.PacketLengths[packetIndex];
                    ReadOnlySpan<byte> audioPacket = audio[byteOffset..(byteOffset + packetLength)];

                    TimeSpan packetDuration = OpusCodec.CalculateOpusPacketLength(audioPacket);

                    if (this.remainingPreSkipTime > packetDuration)
                    {
                        this.remainingPreSkipTime -= packetDuration;
                    }
                    else
                    {
                        this.remainingPreSkipTime = TimeSpan.Zero;

                        lease = AudioBufferManager.Shared.Rent(pageInfo.PacketLengths[packetIndex]);

                        audioPacket.CopyTo(lease.Buffer);
                        lease.FrameCount = (int)packetDuration.TotalMilliseconds / 20;

                        this.packetWriter.TryWrite(lease);
                    }

                    byteOffset += packetLength;
                }

                this.lastPacketContinuesOnNextPage = pageInfo.LastPacketContinuesOnNextPage;

                SkipPage(pageInfo.TotalPageLength);
                goto ProcessPage;
            }
            // else: let the loop finish and get more data
        }

        bool ValidatePage(OggPageInfo pageInfo)
        {
            if (pageInfo.BitstreamSerialNumber != this.currentStreamSerial)
            {
                // this should be impossible?
                throw new InvalidDataException("Received an ogg page with an unexpected serial number.");
            }

            byte[] header = new byte[pageInfo.Header.GetOffsetAndLength(pageInfo.TotalPageLength).Length];
            this.pageBuffer.WrittenSpan[pageInfo.Header].CopyTo(header);
            
            // the crc32 field is zero while calculating the crc32 hash
            header.AsSpan()[pageInfo.CRC32].Clear();

            // RFC 3533, section 6
            OggCRC32 crc = new();
            crc.Append(header);
            crc.Append(this.pageBuffer.WrittenSpan[pageInfo.Body]);

            uint checksum = crc.GetHash();
            return checksum == pageInfo.ClaimedChecksum;
        }

        void SkipPage(int length)
        {
            if (length >= this.pageBuffer.WrittenCount || length == -1)
            {
                this.pageBuffer.Reset();
                return;
            }

            this.pageBuffer.DiscardStart(length);
        }

        bool ValidateAndExtractPageInfo(ref OggPageInfo pageInfo)
        {
            if (!OggPageInfo.IsCompletePageHeader(this.pageBuffer.WrittenSpan))
            {
                SkipPage(this.pageBuffer.WrittenSpan.IndexOf("OggS"u8));
                return false;
            }

            if (!OggPageInfo.TryParse(this.pageBuffer.WrittenSpan, ref pageInfo))
            {
                SkipPage(this.pageBuffer.WrittenSpan.IndexOf("OggS"u8));
                return false;
            }

            if (consumed != inputLength && pageInfo.TotalPageLength > this.pageBuffer.WrittenCount)
            {
                throw new InvalidDataException("Encountered an invalid ogg page.");
            }

            return true;
        }
    }
}
