using System;
using System.Diagnostics;

using Microsoft.Win32.SafeHandles;

namespace DSharpPlus.Voice.Interop.Opus;

/// <summary>
/// Represents a convenience wrapper around a <see cref="NativeOpusRepacketizer"/>. This wrapper is not thread-safe
/// and may only be used synchronized.
/// </summary>
public sealed unsafe class OpusRepacketizerHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private int msInRp;

    private NativeOpusRepacketizer* Repacketizer
    {
        get => (NativeOpusRepacketizer*)this.handle;
        set => this.handle = (nint)value;
    }

    public OpusRepacketizerHandle()
        : base(true)
    {
        this.msInRp = 0;
        this.Repacketizer = OpusInterop.CreateRepacketizer();
    }

    /// <summary>
    /// Check whether a frame of the specified length in milliseconds can be emplaced safely into the repacketizer.
    /// </summary>
    /// <param name="frameLength">The length of this frame in milliseconds.</param>
    public bool Fits(int frameLength)
        => this.msInRp + frameLength <= 120;

    /// <summary>
    /// Places an encoded opus frame into the repacketizer.
    /// </summary>
    /// <param name="frame">The frame data.</param>
    /// <param name="frameLength">The frame length in milliseconds.</param>
    /// <returns>A value indicating whether the repacketizer has reached 120ms in total and is ready to be extracted from.</returns>
    public bool Emplace(ReadOnlySpan<byte> frame, int frameLength = 20)
    {
        Debug.Assert(this.msInRp + frameLength <= 120);

        OpusError error = OpusInterop.SubmitFrameToRepacketizer(this.Repacketizer, frame);
        this.msInRp += frameLength;

        Debug.Assert(error == OpusError.OpusOK);

        return this.msInRp == 120;
    }

    /// <summary>
    /// Extracts a frame from the repacketizer and places it into the provided buffer.
    /// </summary>
    /// <returns>The amount of bytes written to the buffer.</returns>
    public int Extract(Span<byte> buffer)
    {
        int length = OpusInterop.ExtractPacket(this.Repacketizer, buffer);

        this.Repacketizer = OpusInterop.ReinitializeRepacketizer(this.Repacketizer);
        this.msInRp = 0;

        return length;
    }

    /// <inheritdoc/>
    protected override bool ReleaseHandle()
    {
        OpusInterop.DestroyRepacketizer(this.Repacketizer);
        this.handle = 0;
        return true;
    }
}
