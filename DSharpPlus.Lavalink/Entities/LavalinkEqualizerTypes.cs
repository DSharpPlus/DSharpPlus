namespace DSharpPlus.Lavalink;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Represents Lavalink equalizer band adjustment. This is used to alter the sound output by using Lavalink's equalizer.
/// </summary>
public readonly struct LavalinkBandAdjustment
{
    /// <summary>
    /// Gets the ID of the band to adjust.
    /// </summary>
    [JsonProperty("band")]
    public int BandId { get; }

    /// <summary>
    /// Gets the gain of the specified band.
    /// </summary>
    [JsonProperty("gain")]
    public float Gain { get; }

    /// <summary>
    /// Creates a new band adjustment with specified parameters.
    /// </summary>
    /// <param name="bandId">Which band to adjust. Must be in 0-14 range.</param>
    /// <param name="gain">By how much to adjust the band. Must be greater than or equal to -0.25 (muted), and less than or equal to +1.0. +0.25 means the band is doubled.</param>
    public LavalinkBandAdjustment(int bandId, float gain)
    {
        if (bandId < 0 || bandId > 14)
        {
            throw new ArgumentOutOfRangeException(nameof(bandId), "Band ID cannot be lower than 0 or greater than 14.");
        }

        if (gain < -0.25 || gain > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(gain), "Gain cannot be lower than -0.25 or greater than 1.0.");
        }

        BandId = bandId;
        Gain = gain;
    }
}

internal class LavalinkBandAdjustmentComparer : IEqualityComparer<LavalinkBandAdjustment>
{
    public bool Equals(LavalinkBandAdjustment x, LavalinkBandAdjustment y)
        => x.BandId == y.BandId;

    public int GetHashCode(LavalinkBandAdjustment obj)
        => obj.BandId;
}
