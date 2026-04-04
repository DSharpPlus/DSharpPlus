using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DSharpPlus.Voice;

/// <summary>
/// Represents a timestamp within the lifetime of an audio connection, relative to when the user first started sending audio.
/// </summary>
public readonly record struct AudioTimestamp 
    : IComparable<AudioTimestamp>, 
    IParsable<AudioTimestamp>, 
    ISpanParsable<AudioTimestamp>,
    IUtf8SpanParsable<AudioTimestamp>,
    IFormattable,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    private readonly ulong timestamp;

    internal AudioTimestamp(ulong normalizedRtpTimestamp)
        => this.timestamp = normalizedRtpTimestamp;

    /// <summary>
    /// Represents the minimum possible value of zero time elapsed since establishing the connection.
    /// </summary>
    public static AudioTimestamp Zero => new(0);

    /// <summary>
    /// Represents the maximum possible value.
    /// </summary>
    public static AudioTimestamp MaxValue => new(ulong.MaxValue);

    /// <summary>
    /// Gets the milliseconds component of this timestamp.
    /// </summary>
    public int Milliseconds => (int)(this.timestamp % 1000);

    /// <summary>
    /// Gets the seconds component of this timestamp.
    /// </summary>
    public int Seconds => (int)(this.timestamp / 1000 % 60);

    /// <summary>
    /// Gets the minutes component of this timestamp.
    /// </summary>
    public int Minutes => (int)(this.timestamp / 60000 % 60);

    /// <summary>
    /// Gets the hours component of this timestamp.
    /// </summary>
    public int Hours => (int)(this.timestamp / 3600000 % 24);

    /// <summary>
    /// Gets the days component of this timestamp.
    /// </summary>
    public int Days => (int)(this.timestamp / 86400000);

    /// <summary>
    /// Gets this timestamp expressed in total milliseconds.
    /// </summary>
    public ulong TotalMilliseconds => this.timestamp;

    /// <summary>
    /// Gets this timestamp expressed in total seconds.
    /// </summary>
    public double TotalSeconds => (double)this.timestamp / 1000;

    /// <summary>
    /// Gets this timestamp expressed in total minutes.
    /// </summary>
    public double TotalMinutes => (double)this.timestamp / 60000;

    /// <summary>
    /// Gets this timestamp expressed in total hours.
    /// </summary>
    public double TotalHours => (double)this.timestamp / 3600000;

    /// <summary>
    /// Gets this timestamp expressed in total days.
    /// </summary>
    public double TotalDays => (double)this.timestamp / 86400000;

    /// <summary>
    /// Subtracts a duration from the given timestamp.
    /// </summary>
    public static AudioTimestamp operator - (AudioTimestamp timestamp, TimeSpan timeSpan)
        => new(timestamp.timestamp - (ulong)timeSpan.TotalMilliseconds);

    /// <summary>
    /// Adds a duration to the given timestamp.
    /// </summary>
    public static AudioTimestamp operator + (AudioTimestamp timestamp, TimeSpan timeSpan)
        => new(timestamp.timestamp + (ulong)timeSpan.TotalMilliseconds);

    /// <summary>
    /// Compares two timestamps for which one is greater.
    /// </summary>
    public static bool operator > (AudioTimestamp left, AudioTimestamp right)
        => left.timestamp > right.timestamp;

    /// <summary>
    /// Compares two timestamps for which one is lesser.
    /// </summary>
    public static bool operator < (AudioTimestamp left, AudioTimestamp right)
        => left.timestamp < right.timestamp;

    // we just implement parsing and formatting on top of TimeSpan, which in principle looks similar, even if it's a bit inefficient to do so
    // for the utf-8 bits

    /// <inheritdoc/>
    public static AudioTimestamp Parse(string s, IFormatProvider? provider) 
        => new((ulong)TimeSpan.Parse(s, provider).TotalMilliseconds);

    /// <inheritdoc/>
    public static AudioTimestamp Parse(ReadOnlySpan<char> s, IFormatProvider? provider) 
        => new((ulong)TimeSpan.Parse(s, provider).TotalMilliseconds);

    /// <inheritdoc/>
    public static AudioTimestamp Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider) 
        => new((ulong)TimeSpan.Parse(Encoding.UTF8.GetString(utf8Text), provider).TotalMilliseconds);
    
    /// <inheritdoc/>
    public static bool TryParse
    (
        [NotNullWhen(true)] 
        string? s, 
        
        IFormatProvider? provider, 
        
        [MaybeNullWhen(false)] 
        out AudioTimestamp result
    ) 
    {
        if (TimeSpan.TryParse(s, provider, out TimeSpan parsed))
        {
            result = new((ulong)parsed.TotalMilliseconds);
            return true;
        }

        result = default;
        return false;
    }
    
    /// <inheritdoc/>
    public static bool TryParse
    (
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        
        [MaybeNullWhen(false)] 
        out AudioTimestamp result
    )
    {
        if (TimeSpan.TryParse(s, provider, out TimeSpan parsed))
        {
            result = new((ulong)parsed.TotalMilliseconds);
            return true;
        }

        result = default;
        return false;
    }
    
    /// <inheritdoc/>
    public static bool TryParse
    (
        ReadOnlySpan<byte> utf8Text, 
        IFormatProvider? provider, 
        
        [MaybeNullWhen(false)] 
        out AudioTimestamp result
    )
    {
        if (TimeSpan.TryParse(Encoding.UTF8.GetString(utf8Text), provider, out TimeSpan parsed))
        {
            result = new((ulong)parsed.TotalMilliseconds);
            return true;
        }

        result = default;
        return false;
    }
    
    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => TimeSpan.FromMilliseconds(this.timestamp).ToString(format, formatProvider);
    
    /// <inheritdoc/>
    public bool TryFormat
    (
        Span<char> destination, 
        out int charsWritten, 
        ReadOnlySpan<char> format, 
        IFormatProvider? provider
    )
    {
        return TimeSpan.FromMilliseconds(this.timestamp).TryFormat
        (
            destination,
            out charsWritten,
            format,
            provider
        );
    }
    
    /// <inheritdoc/>
    public bool TryFormat
    (
        Span<byte> utf8Destination, 
        out int bytesWritten, 
        ReadOnlySpan<char> format, 
        IFormatProvider? provider
    )
    {
        return TimeSpan.FromMilliseconds(this.timestamp).TryFormat
        (
            utf8Destination,
            out bytesWritten,
            format,
            provider
        );
    }

    /// <inheritdoc/>
    public int CompareTo(AudioTimestamp other) 
        => this.timestamp.CompareTo(other.timestamp);
}
