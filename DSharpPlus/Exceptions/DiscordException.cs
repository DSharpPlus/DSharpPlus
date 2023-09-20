using System;
using DSharpPlus.Net;

namespace DSharpPlus.Exceptions;

public abstract class DiscordException : Exception
{
    /// <summary>
    /// Gets the request that caused the exception.
    /// </summary>
    public virtual BaseRestRequest WebRequest { get; internal set; } = null!;

    /// <summary>
    /// Gets the response to the request.
    /// </summary>
    public virtual RestResponse WebResponse { get; internal set; } = null!;

    /// <summary>
    /// Gets the JSON message received.
    /// </summary>
    public virtual string? JsonMessage { get; internal set; }

    /// <inheritdoc />
    public override string Message => $"{base.Message}. Json Message: {JsonMessage ?? "Discord did not provide an error message."}";

    public DiscordException() : base() { }
    public DiscordException(string message) : base(message) { }
    public DiscordException(string message, Exception innerException) : base(message, innerException) { }
}
