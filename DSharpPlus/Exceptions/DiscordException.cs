using System;
using System.Net.Http;

namespace DSharpPlus.Exceptions;

public abstract class DiscordException : Exception
{
    /// <summary>
    /// Gets the request that caused the exception.
    /// </summary>
    public virtual HttpRequestMessage? Request { get; internal set; }

    /// <summary>
    /// Gets the response to the request.
    /// </summary>
    public virtual HttpResponseMessage? Response { get; internal set; }

    /// <summary>
    /// Gets the JSON message received.
    /// </summary>
    public virtual string? JsonMessage { get; internal set; }

    public DiscordException() : base() { }
    public DiscordException(string message) : base(message) { }
    public DiscordException(string message, Exception innerException) : base(message, innerException) { }
}
