using System;
using System.IO;

namespace DSharpPlus.Net;

// this class is a clusterfuck to prevent the RestClient from disposing streams we dont want to dispose
// only god, aaron and i know what a psychosis it was to fix this issue (#1677)
public class RequestStreamWrapper : Stream, IDisposable
{
    public Stream UnderlyingStream { get; init; }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(this.UnderlyingStream is null, this);

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
        this.UnderlyingStream = stream;
    }

    /// <inheritdoc cref="Stream.CanRead"/>
    public override bool CanRead => this.UnderlyingStream.CanRead;

    /// <inheritdoc cref="Stream.CanSeek"/>
    public override bool CanSeek => this.UnderlyingStream.CanSeek;

    /// <inheritdoc cref="Stream.CanWrite"/>
    public override bool CanWrite => this.UnderlyingStream.CanWrite;

    /// <inheritdoc cref="Stream.Flush"/>
    public override void Flush() => this.UnderlyingStream.Flush();

    /// <inheritdoc cref="Stream.Length"/>
    public override long Length
    {
        get
        {
            CheckDisposed();
            return this.UnderlyingStream.Length;
        }
    }

    /// <inheritdoc cref="Stream.Position"/>
    public override long Position
    {
        get => this.UnderlyingStream.Position;
        set => this.UnderlyingStream.Position = value;
    }

    /// <inheritdoc cref="Stream.Read(byte[], int, int)"/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        CheckDisposed();
        return this.UnderlyingStream.Read(buffer, offset, count);
    }

    /// <inheritdoc cref="Stream.Seek(long, SeekOrigin)"/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        CheckDisposed();
        return this.UnderlyingStream.Seek(offset, origin);
    }

    /// <inheritdoc cref="Stream.SetLength(long)"/>
    public override void SetLength(long value) => this.UnderlyingStream.SetLength(value);

    /// <inheritdoc cref="Stream.Write(byte[], int, int)"/>
    public override void Write(byte[] buffer, int offset, int count) => this.UnderlyingStream.Write(buffer, offset, count);
}
