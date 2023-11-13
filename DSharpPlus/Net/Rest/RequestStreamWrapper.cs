namespace DSharpPlus.Net;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

// this class is a clusterfuck to prevent the RestClient from disposing streams we dont want to dispose
// only god, aaron and i know what a psychosis it was to fix this issue (#1677)
public class RequestStreamWrapper : Stream, IDisposable
{
    private readonly Stream _refStream;

    private void CheckDisposed()
    {
        if (this._refStream is null)
        {
            throw new ObjectDisposedException(nameof(RequestStreamWrapper));
        }
    }

    //basically these two methods are the whole purpose of this class
    protected override void Dispose(bool disposing){ /* NOT TODAY MY FRIEND */ }
    protected void Dispose() => this.Dispose(true);

    public RequestStreamWrapper(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        this._refStream = stream;
    }

    public override bool CanRead => this._refStream.CanRead;

    public override bool CanSeek => this._refStream.CanSeek;

    public override bool CanWrite => this._refStream.CanWrite;
    
    public override void Flush() => this._refStream.Flush();

    public override long Length
    {
        get
        {
            this.CheckDisposed();
            return this._refStream.Length;
        }
    }

    public override long Position
    {
        get => this._refStream.Position;
        set => this._refStream.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        CheckDisposed();
        return this._refStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        CheckDisposed();
        return this._refStream.Seek(offset, origin);
    }

    public override void SetLength(long value) => this._refStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => this._refStream.Write(buffer, offset, count);

    void IDisposable.Dispose() => Dispose(false);
}
