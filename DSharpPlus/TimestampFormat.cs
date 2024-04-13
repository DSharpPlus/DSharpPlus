namespace DSharpPlus;

/// <summary>
/// Denotes the type of formatting to use for timestamps.
/// </summary>
public enum TimestampFormat : byte
{
    /// <summary>
    /// A short date. e.g. 18/06/2021.
    /// </summary>
    ShortDate = (byte)'d',

    /// <summary>
    /// A long date. e.g. 18 June 2021.
    /// </summary>
    LongDate = (byte)'D',

    /// <summary>
    /// A short date and time. e.g. 18 June 2021 03:50.
    /// </summary>
    ShortDateTime = (byte)'f',

    /// <summary>
    /// A long date and time. e.g. Friday 18 June 2021 03:50.
    /// </summary>
    LongDateTime = (byte)'F',

    /// <summary>
    /// A short time. e.g. 03:50.
    /// </summary>
    ShortTime = (byte)'t',

    /// <summary>
    /// A long time. e.g. 03:50:15.
    /// </summary>
    LongTime = (byte)'T',

    /// <summary>
    /// The time relative to the client. e.g. An hour ago.
    /// </summary>
    RelativeTime = (byte)'R'
}
