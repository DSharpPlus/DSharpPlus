using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Additional flags for handling file upload streams.
/// </summary>
[Flags]
public enum AddFileOptions
{
    /// <summary>
    /// No additional behavior. The stream will read to completion and is left at that position after sending.
    /// </summary>
    None = 0,

    /// <summary>
    /// Resets the stream to its original position after sending.
    /// </summary>
    ResetStream = 0x1,

    /// <summary>
    /// Closes the stream upon sending.
    /// </summary>
    CloseStream = 0x2,

    /// <summary>
    /// Immediately reads the stream to completion and copies its contents to an in-memory stream.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this incurs an additional memory overhead and may perform synchronous I/O and should only be used if the source stream cannot be kept open any longer.
    /// </para>
    /// <para>
    /// If specified together with <see cref="CloseStream"/>, the stream will closed immediately after the copy.
    /// </para>
    /// </remarks>
    CopyStream = 0x4,
}
