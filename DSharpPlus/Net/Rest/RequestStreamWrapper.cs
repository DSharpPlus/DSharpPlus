using System;
using System.IO;

namespace DSharpPlus.Net;

// this class is a clusterfuck to prevent the RestClient from disposing streams we dont want to dispose
// only god, aaron and i know what a psychosis it was to fix this issue (#1677)
public class RequestStreamWrapper : Stream, IDisposable
{
    public Stream UnderlyingStream { get; init; }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(UnderlyingStream is null, this);

    //basically these two methods are the whole purpose of this class
    protected override void Dispose(bool disposing) { /* NOT TODAY MY FRIEND */ }
    protected new void Dispose() => Dispose(true);
    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public RequestStreamWrapper(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        UnderlyingStream = stream;
    }

    /// <inheritdoc cref="Stream.CanRead"/>
    public override bool CanRead => UnderlyingStream.CanRead;

    /// <inheritdoc cref="Stream.CanSeek"/>
    public override bool CanSeek => UnderlyingStream.CanSeek;

    /// <inheritdoc cref="Stream.CanWrite"/>
    public override bool CanWrite => UnderlyingStream.CanWrite;

    /// <inheritdoc cref="Stream.Flush"/>
    public override void Flush() => UnderlyingStream.Flush();

    /// <inheritdoc cref="Stream.Length"/>
    public override long Length
    {
        get
        {
            CheckDisposed();
            return UnderlyingStream.Length;
        }
    }

    /// <inheritdoc cref="Stream.Position"/>
    public override long Position
    {
        get => UnderlyingStream.Position;
        set => UnderlyingStream.Position = value;
    }

    /// <inheritdoc cref="Stream.Read(byte[], int, int)"/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        CheckDisposed();
        return UnderlyingStream.Read(buffer, offset, count);
    }

    /// <inheritdoc cref="Stream.Seek(long, SeekOrigin)"/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        CheckDisposed();
        return UnderlyingStream.Seek(offset, origin);
    }

    /// <inheritdoc cref="Stream.SetLength(long)"/>
    public override void SetLength(long value) => UnderlyingStream.SetLength(value);

    /// <inheritdoc cref="Stream.Write(byte[], int, int)"/>
    public override void Write(byte[] buffer, int offset, int count) => UnderlyingStream.Write(buffer, offset, count);
}
