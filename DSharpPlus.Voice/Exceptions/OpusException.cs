using System;

using DSharpPlus.Voice.Interop.Opus;

namespace DSharpPlus.Voice.Exceptions;

/// <summary>
/// Thrown if an error occurred in opus encoding/decoding.
/// </summary>
public sealed class OpusException : Exception
{
    internal OpusException(OpusError error, string site) : base($"Encountered opus codec error {error} in {site}.")
    {

    }
}
