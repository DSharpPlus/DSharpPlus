using System;

namespace DSharpPlus.Voice.Protocol.RTCP.Serialization;

/// <summary>
/// A helper type to convert NTP timestamps to DateTimeOffset and vice versa
/// </summary>
internal static class NTPTimestampHelper
{
    // NTP timestamps, which RTCP uses all over the place, are defined in RFC 1305, section 3.1, as follows:
    // 
    // 0                   1                   2                   3
    // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    // |                   Integer part of seconds                     |
    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    // |                 Fractional part of seconds                    |
    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    // 
    // where the integer part is the amount of seconds since January 01, 1900 and the fractional part is the
    // amount of 4294967269th (2^32) parts of a second passed within that second.
    //
    // this gives us a range of 1900-01-01 to 2036-02-07, at which point NTP will roll over. since we are only
    // concerned with implementing RTCP, which operates mostly in real-time, we'll infer that we have rolled over
    // if the current time is 2036 or after that and the timestamp would amount to being older than 2000. for
    // now, we're not going to concern ourselves with the next rollover in 2172, as presumably this godforsaken
    // protocol will have been superseded by then.
    //
    // insanely enough, RFC 3550 actually somehow makes this worse. in section 4 it defines "a more compact
    // representation", where the low 16 bits of the integer part and the high 16 bits of the fractional part
    // are used. since RTP is a real-time protocol, we process these relative to current time; that is, we
    // assume that the timestamp is probably recent enough to have the same or the immediately preceding
    // high 16 bits of the integer part, or framed differently, that the timestamp is no older than about 18:12
    // hours, or 65535 seconds.

    // 2208988800 seconds between 1900-01-01T00:00:00 and 1970-01-01T00:00:00
    private const uint NTPToUnixOffset = 2208988800;

    extension (DateTimeOffset timestamp)
    {
        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> to an NTP timestamp. 
        /// </summary>
        public ulong ToNTPTimestamp()
        {
            long seconds = timestamp.ToUnixTimeSeconds() + NTPToUnixOffset;
            seconds %= uint.MaxValue;

            long fraction = (long)((double)timestamp.SubsecondNanoseconds * uint.MaxValue / 1000000000);

            return (ulong)((seconds << 32) | (fraction & 0xFFFFFFFF));
        }

        /// <summary>
        /// Converts an NTP timestamp to a <see cref="DateTimeOffset"/>. 
        /// </summary>
        public static DateTimeOffset FromNTPTimestamp(ulong ntp)
        {
            uint ntpSeconds = (uint)(ntp >> 32);
            uint ntpFraction = (uint)(ntp & 0xFFFFFFFF);

            long seconds = ntpSeconds - NTPToUnixOffset;

            // the timestamp 2000-01-01T00:00:00 in unix time, as discussed above: roll over once to get above 2036
            if (seconds <= 946684800)
            {
                seconds += uint.MaxValue;
            }

            long nanoseconds = (long)((double)ntpFraction * 1000000000 / uint.MaxValue);

            return DateTimeOffset.FromUnixTimeSeconds(seconds) + TimeSpan.FromTicks(nanoseconds / 100);
        }

        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> to a compact NTP timestamp as defined in RFC 3550, Section 4. 
        /// </summary>
        public uint ToRFC3550CompactNTPTimestamp()
        {
            ulong ntp = timestamp.ToNTPTimestamp();
            return (uint)((ntp >> 16) & 0xFFFFFFFF);
        }

        /// <summary>
        /// Converts a compact NTP timestamp as defined in RFC 3550, Section 4, to a <see cref="DateTimeOffset"/>. 
        /// </summary>
        public static DateTimeOffset FromRFC3550CompactNTPTimestamp(uint compactNTP)
        {
            ulong ntp = (ulong)compactNTP << 16;
            ulong currentNtpTimestamp = DateTimeOffset.UtcNow.ToNTPTimestamp();

            uint compactNTPSeconds = compactNTP >> 16;
            uint currentLessSignificantCompactNTPSeconds = (uint)((currentNtpTimestamp >> 32) & 0xFFFF);

            // assume we didn't timetravel but rather that the more significant half of the NTP second rolled over since
            // this timestamp was created. this does require the system clock to be somewhat accurate.
            if (compactNTPSeconds > currentLessSignificantCompactNTPSeconds)
            {
                ntp |= (currentNtpTimestamp & 0xFFFF0000_00000000) - 1;
            }
            else
            {
                ntp |= currentNtpTimestamp & 0xFFFF0000_00000000;
            }

            return DateTimeOffset.FromNTPTimestamp(ntp);
        }

        /// <summary>
        /// Gets the total amount of nanoseconds that have progressed since the last full second of this timestamp.
        /// </summary>
        private long SubsecondNanoseconds => (timestamp.Millisecond * 1000000) + (timestamp.Microsecond * 1000) + timestamp.Nanosecond;
    }
}
