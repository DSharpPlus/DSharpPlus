// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using System.Text;

namespace DSharpPlus;

/// <summary>
/// Tool to detect image formats and convert from binary data to base64 strings.
/// </summary>
public sealed class ImageTool : IDisposable
{
    private const ulong PNG_MAGIC = 0x0A1A_0A0D_474E_5089;
    private const ushort JPEG_MAGIC_1 = 0xD8FF;
    private const ushort JPEG_MAGIC_2 = 0xD9FF;
    private const ulong GIF_MAGIC_1 = 0x0000_6139_3846_4947;
    private const ulong GIF_MAGIC_2 = 0x0000_6137_3846_4947;
    private const uint WEBP_MAGIC_1 = 0x4646_4952;
    private const uint WEBP_MAGIC_2 = 0x5042_4557;

    private const ulong GIF_MASK = 0x0000_FFFF_FFFF_FFFF;
    private const ulong MASK32 = 0x0000_0000_FFFF_FFFF;
    private const uint MASK16 = 0x0000_FFFF;

    /// <summary>
    /// Gets the stream this tool is operating on.
    /// </summary>
    public Stream SourceStream { get; }

    private ImageFormat _ifcache;
    private string _b64cache;

    /// <summary>
    /// Creates a new image tool from given stream.
    /// </summary>
    /// <param name="stream">Stream to work with.</param>
    public ImageTool(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (!stream.CanRead || !stream.CanSeek)
            throw new ArgumentException("The stream needs to be both readable and seekable.", nameof(stream));

        this.SourceStream = stream;
        this.SourceStream.Seek(0, SeekOrigin.Begin);

        this._ifcache = 0;
        this._b64cache = null;
    }

    /// <summary>
    /// Detects the format of this image.
    /// </summary>
    /// <returns>Detected format.</returns>
    public ImageFormat GetFormat()
    {
        if (this._ifcache != ImageFormat.Unknown)
            return this._ifcache;

        using (var br = new BinaryReader(this.SourceStream, Utilities.UTF8, true))
        {
            var bgn64 = br.ReadUInt64();
            if (bgn64 == PNG_MAGIC)
                return this._ifcache = ImageFormat.Png;

            bgn64 &= GIF_MASK;
            if (bgn64 == GIF_MAGIC_1 || bgn64 == GIF_MAGIC_2)
                return this._ifcache = ImageFormat.Gif;

            var bgn32 = (uint)(bgn64 & MASK32);
            if (bgn32 == WEBP_MAGIC_1 && br.ReadUInt32() == WEBP_MAGIC_2)
                return this._ifcache = ImageFormat.WebP;

            var bgn16 = (ushort)(bgn32 & MASK16);
            if (bgn16 == JPEG_MAGIC_1)
            {
                this.SourceStream.Seek(-2, SeekOrigin.End);
                if (br.ReadUInt16() == JPEG_MAGIC_2)
                    return this._ifcache = ImageFormat.Jpeg;
            }
        }

        throw new InvalidDataException("The data within the stream was not valid image data.");
    }

    /// <summary>
    /// Converts this image into base64 data format string.
    /// </summary>
    /// <returns>Data-scheme base64 string.</returns>
    public string GetBase64()
    {
        if (this._b64cache != null)
            return this._b64cache;

        var fmt = this.GetFormat();
        var sb = new StringBuilder();

        sb.Append("data:image/")
            .Append(fmt.ToString().ToLowerInvariant())
            .Append(";base64,");

        this.SourceStream.Seek(0, SeekOrigin.Begin);
        var buff = new byte[this.SourceStream.Length];
        var br = 0;
        while (br < buff.Length)
            br += this.SourceStream.Read(buff, br, (int)this.SourceStream.Length - br);

        sb.Append(Convert.ToBase64String(buff));

        return this._b64cache = sb.ToString();
    }

    /// <summary>
    /// Disposes this image tool.
    /// </summary>
    public void Dispose()
    {
        if (this.SourceStream != null)
            this.SourceStream.Dispose();
    }
}

/// <summary>
/// Represents format of an image.
/// </summary>
public enum ImageFormat : int
{
    Unknown = 0,
    Jpeg = 1,
    Png = 2,
    Gif = 3,
    WebP = 4,
    Auto = 5
}
