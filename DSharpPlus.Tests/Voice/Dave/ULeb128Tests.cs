using System;
using System.Runtime.CompilerServices;

using DSharpPlus.Voice.Protocol.Dave;

using NUnit.Framework;

namespace DSharpPlus.Tests.Voice.Dave;

public sealed class ULeb128Tests
{
    [Test]
    public void TestRoundtrippingZeroAsInt32()
    {
        Span<byte> buffer = stackalloc byte[4];

        Assert.That(ULeb128.TryWriteInt32(buffer, 0, out int written));
        Assert.That(written, Is.EqualTo(1));
        Assert.That(Unsafe.ReadUnaligned<int>(in buffer[0]), Is.Zero);

        Assert.That(ULeb128.TryReadInt32(buffer, out int value, out int read));
        Assert.That(value, Is.Zero);
        Assert.That(read, Is.EqualTo(1));
    }

    [Test]
    public void TestRoundtrippingSingleByteAsInt32()
    {
        Span<byte> buffer = stackalloc byte[4];

        Assert.That(ULeb128.TryWriteInt32(buffer, 17, out int written));
        Assert.That(written, Is.EqualTo(1));
        Assert.That(Unsafe.ReadUnaligned<int>(in buffer[0]), Is.EqualTo(17));

        Assert.That(ULeb128.TryReadInt32(buffer, out int value, out int read));
        Assert.That(value, Is.EqualTo(17));
        Assert.That(read, Is.EqualTo(1));
    }

    [Test]
    public void TestRoundtrippingTwoBytesAsInt32()
    {
        Span<byte> buffer = stackalloc byte[4];

        Assert.That(ULeb128.TryWriteInt32(buffer, 129, out int written));
        Assert.That(written, Is.EqualTo(2));
        Assert.That(buffer[0], Is.EqualTo(0x81));
        Assert.That(buffer[1], Is.EqualTo(0x01));

        Assert.That(ULeb128.TryReadInt32(buffer, out int value, out int read));
        Assert.That(value, Is.EqualTo(129));
        Assert.That(read, Is.EqualTo(2));
    }

    [Test]
    public void TestRoundtrippingZeroAsInt64()
    {
        Span<byte> buffer = stackalloc byte[4];

        Assert.That(ULeb128.TryWriteInt64(buffer, 0, out int written));
        Assert.That(written, Is.EqualTo(1));
        Assert.That(Unsafe.ReadUnaligned<int>(in buffer[0]), Is.Zero);

        Assert.That(ULeb128.TryReadInt64(buffer, out long value, out int read));
        Assert.That(value, Is.Zero);
        Assert.That(read, Is.EqualTo(1));
    }

    [Test]
    public void TestRoundtrippingSingleByteAsInt64()
    {
        Span<byte> buffer = stackalloc byte[4];

        Assert.That(ULeb128.TryWriteInt64(buffer, 17, out int written));
        Assert.That(written, Is.EqualTo(1));
        Assert.That(Unsafe.ReadUnaligned<int>(in buffer[0]), Is.EqualTo(17));

        Assert.That(ULeb128.TryReadInt64(buffer, out long value, out int read));
        Assert.That(value, Is.EqualTo(17));
        Assert.That(read, Is.EqualTo(1));
    }

    [Test]
    public void TestRoundtrippingTwoBytesAsInt64()
    {
        Span<byte> buffer = stackalloc byte[4];

        Assert.That(ULeb128.TryWriteInt64(buffer, 129, out int written));
        Assert.That(written, Is.EqualTo(2));
        Assert.That(buffer[0], Is.EqualTo(0x81));
        Assert.That(buffer[1], Is.EqualTo(0x01));

        Assert.That(ULeb128.TryReadInt64(buffer, out long value, out int read));
        Assert.That(value, Is.EqualTo(129));
        Assert.That(read, Is.EqualTo(2));
    }

    [Test]
    public void TestRejectingNeverendingULeb128()
    {
        Span<byte> buffer = [0x80, 0xF7, 0xE4, 0x90, 0xAA, 0xA7, 0xB3];

        Assert.That(!ULeb128.TryReadUInt64(buffer, out _, out _));
    }
}
