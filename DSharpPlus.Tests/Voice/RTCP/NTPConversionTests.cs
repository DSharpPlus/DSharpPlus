using System;

using DSharpPlus.Voice.Protocol.RTCP.Serialization;

using NUnit.Framework;

namespace DSharpPlus.Tests.Voice.RTCP;

public sealed class NTPConversionTests
{
    [Test]
    public void Converts2000_01_01T00_00_01()
    {
        DateTimeOffset time = new(2000, 01, 01, 00, 00, 01, TimeSpan.FromMinutes(0));
        ulong ntp = time.ToNTPTimestamp();

        Assert.That(ntp, Is.EqualTo((ulong)3155673601 << 32));

        DateTimeOffset roundtrip = DateTimeOffset.FromNTPTimestamp(ntp);

        Assert.That(roundtrip, Is.EqualTo(time));
    }

    [Test]
    public void RollsOver2000_01_01T00_00_00()
    {
        DateTimeOffset time = new(2000, 01, 01, 00, 00, 00, TimeSpan.FromMinutes(0));
        ulong ntp = time.ToNTPTimestamp();

        Assert.That(ntp, Is.EqualTo((ulong)3155673600 << 32));

        DateTimeOffset roundtrip = DateTimeOffset.FromNTPTimestamp(ntp);

        Assert.That(roundtrip, Is.Not.EqualTo(time));
        Assert.That(roundtrip, Is.EqualTo(new DateTimeOffset(2136, 02, 07, 06, 28, 15, TimeSpan.FromMinutes(0))));
    }

    // example chosen based on the time of writing this test
    [Test]
    public void Converts2026_03_09T13_58_17()
    {
        DateTimeOffset time = new(2026, 03, 09, 13, 58, 17, TimeSpan.FromMinutes(0));
        ulong ntp = time.ToNTPTimestamp();

        Assert.That(ntp, Is.EqualTo((ulong)3982053497 << 32));

        DateTimeOffset roundtrip = DateTimeOffset.FromNTPTimestamp(ntp);

        Assert.That(roundtrip, Is.EqualTo(time));
    }

    [Test]
    // allow a bit of error in this test for floating point inaccuracy across different systems
    // the mathematically exact value of the first assert should be 2147483648, but on my system it calculates to 2147483647
    public void ConvertsMilliseconds()
    {
        DateTimeOffset time = new(2000, 01, 01, 00, 00, 01, 500, TimeSpan.FromMinutes(0));
        ulong ntp = time.ToNTPTimestamp();

        Assert.That(ntp, Is.InRange(((ulong)3155673601 << 32) | 2147483640, ((ulong)3155673601 << 32) | 2147483660));

        DateTimeOffset roundtrip = DateTimeOffset.FromNTPTimestamp(ntp);

        Assert.That(roundtrip, Is.InRange(time - TimeSpan.FromMicroseconds(1), time + TimeSpan.FromMicroseconds(1)));
    }
}
