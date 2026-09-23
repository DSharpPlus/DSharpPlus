using System;

namespace DSharpPlus.Voice.Exceptions;

/// <summary>
/// Thrown if DSharpPlus.Voice failed to establish a connection to the voice gateway or media session.
/// </summary>
public sealed class ConnectingFailedException : Exception
{
    internal ConnectingFailedException(string message) : base(message)
    {
        
    }
}