using System;

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
                this.timestampResynchronizationFactor = (ulong)(DateTimeOffset.UtcNow - this.firstSpeakingTimestamp).TotalMilliseconds;
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

        this.NormalizedSequence = normalizedSequence;
        this.NormalizedTimestamp = normalizedTimestamp;

        return (normalizedSequence, normalizedTimestamp);
    }

    public void IndicateStoppedSpeaking() 
        => this.receivedFirstPacketSinceSpeaking = false;
}
