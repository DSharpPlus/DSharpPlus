using System;

namespace DSharpPlus.Voice.Exceptions;

/// <summary>
/// Thrown if a provided ogg/opus stream was in an unsupported format.
/// </summary>
public sealed class UnsupportedOggOpusStreamException : Exception
{
    internal UnsupportedOggOpusStreamException(string message) : base(message)
    {
        
    }
}
