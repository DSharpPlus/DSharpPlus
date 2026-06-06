using System;
using System.Threading;

namespace DSharpPlus.Voice;

/// <summary>
/// Represents the known data of a user in a voice connection.
/// </summary>
internal sealed class VoiceUser
{
    private const ushort MSBSet16Bit = 1 << 15;
    private const uint MSBSet32Bit = (uint)1 << 31;

    private uint timestampNormalizationFactor;
    private ushort sequenceNormalizationFactor;
    private DateTimeOffset firstSpeakingTimestamp = DateTimeOffset.MinValue;
    private ulong timestampResynchronizationFactor = 0;
    private bool receivedFirstPacketSinceSpeaking = false;
    private int lastTransitTime = 0;
    private uint packetsLost = 0;
    private int packetsReceived = 0;
    private uint cumulativePacketsLost = 0;

    /// <summary>
    /// The snowflake ID of this user.
    /// </summary>
    public ulong UserId { get; init; }

    /// <summary>
    /// The SSRC of this user.
    /// </summary>
    public uint SSRC { get; init; }

    /// <summary>
    /// Indicates whether this user is currently speaking.
    /// </summary>
    public bool IsSpeaking { get; internal set; }

    public ulong NormalizedTimestamp { get; private set; }

    public uint NormalizedSequence { get; private set; }

    public uint HighestSequenceReceived { get; private set; }

    public float InterarrivalJitterEstimate { get; private set; } = 0.0f;

    public float PacketLoss
    {
        get
        {
            float loss = this.packetsReceived / this.packetsLost;
            this.packetsReceived = 0;
            this.packetsLost = 0;
            return loss;
        }
    }

    public uint CumulativePacketsLost => this.cumulativePacketsLost;

    // defaults to the NTP timestamp zero.
    public DateTimeOffset LastSenderReport { get; internal set; } = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public (uint sequence, ulong timestamp) UpdateTimestampAndSequence(ushort rtpSequence, uint rtpTimestamp)
    {
        // uninitialized, the values we received are the random offset from zero
        if (!this.receivedFirstPacketSinceSpeaking)
        {
            if (this.firstSpeakingTimestamp == DateTimeOffset.MinValue)
            {
                this.firstSpeakingTimestamp = DateTimeOffset.UtcNow;
                this.timestampNormalizationFactor = rtpTimestamp;
                this.sequenceNormalizationFactor = rtpSequence;

                this.NormalizedTimestamp = 0;
                this.NormalizedSequence = 0;

                this.receivedFirstPacketSinceSpeaking = true;
                return (0, 0);
            }
            else
            {
                this.timestampResynchronizationFactor = (ulong)(DateTimeOffset.UtcNow - this.firstSpeakingTimestamp).TotalMilliseconds * 48;
                this.timestampNormalizationFactor = rtpTimestamp;
            }
        }

        this.receivedFirstPacketSinceSpeaking = true;

        ushort normalizedLowerSequence = (ushort)(rtpSequence - this.sequenceNormalizationFactor);
        uint normalizedLowerTimestamp = rtpTimestamp - this.timestampNormalizationFactor;

        uint normalizedSequence = normalizedLowerSequence | (this.NormalizedSequence & 0xFFFF_0000);
        ulong normalizedTimestamp = normalizedLowerTimestamp | (this.NormalizedTimestamp & 0xFFFF_FFFF_0000_0000);

        // handle rollovers
        if ((normalizedSequence & MSBSet16Bit) == 0 && (this.NormalizedSequence & MSBSet16Bit) == 1)
        {
            normalizedSequence += ushort.MaxValue;
        }

        if ((normalizedTimestamp & MSBSet32Bit) == 0 && (this.NormalizedTimestamp & MSBSet32Bit) == 1)
        {
            normalizedTimestamp += uint.MaxValue;
        }

        normalizedTimestamp += this.timestampResynchronizationFactor;

        // register packet loss
        Interlocked.Increment(ref this.packetsReceived);

        if (this.NormalizedSequence > normalizedSequence)
        {
            Interlocked.Decrement(ref this.packetsLost);
            Interlocked.Decrement(ref this.cumulativePacketsLost);
        }
        else
        {
            // this will add zero if the sequences are directly incremental
            Interlocked.Add(ref this.packetsLost, normalizedSequence - this.NormalizedSequence - 1);
            Interlocked.Add(ref this.cumulativePacketsLost, normalizedSequence - this.NormalizedSequence - 1);
        }

        // don't update if we received an out of order packet, since that'll fuck just about everything up
        this.NormalizedSequence = uint.Max(this.NormalizedSequence, normalizedSequence);
        this.NormalizedTimestamp = ulong.Max(this.NormalizedTimestamp, normalizedTimestamp);
        this.HighestSequenceReceived = uint.Max(this.HighestSequenceReceived, normalizedSequence + this.sequenceNormalizationFactor);

        // calculate transit time and jitter estimate
        int transitTime = (int)((ulong)((DateTimeOffset.UtcNow - this.firstSpeakingTimestamp).TotalMilliseconds * 48) - normalizedTimestamp);
        int deviation = this.lastTransitTime == 0 ? 0 : transitTime - this.lastTransitTime;
        this.lastTransitTime = transitTime;

        deviation = int.Abs(deviation);
        this.InterarrivalJitterEstimate = (15 / 16 * this.InterarrivalJitterEstimate) + (1 / 16 * deviation);

        return (normalizedSequence, normalizedTimestamp);
    }

    public void IndicateStoppedSpeaking() 
        => this.receivedFirstPacketSinceSpeaking = false;
}
