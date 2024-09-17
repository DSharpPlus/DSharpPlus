using System.Runtime.InteropServices;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// Represents a structure holding input and output values for the zlib functions.
/// </summary>
// this struct should not be marked as a record struct.
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ZlibStream
{
    /// <summary>
    /// The address to the next input byte to process.
    /// </summary>
    internal byte* nextInputByte; // uint8_t* nextIn

    /// <summary>
    /// The address to the next decompressed output byte.
    /// </summary>
    internal byte* nextOutputByte; // uint8_t* nextOut

    /// <summary>
    /// The last error message, or null if there was no error.
    /// </summary>
    internal byte* message; // char* msg

    /// <summary>
    /// Internal state of the inflater.
    /// </summary>
    internal void* internalState; // void* internalState

    /// <summary>
    /// The amount of bytes available at nextInputByte.
    /// </summary>
    internal uint availableInputBytes; // uint32_t availIn

    /// <summary>
    /// The remaining free space at nextOutputByte for the next read.
    /// </summary>
    internal uint availableOutputBytes; // uint32_t availOut
}
