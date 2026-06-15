using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Represents an union between a string message read from the gateway, an exception or a close code.
/// </summary>
public readonly record struct TransportFrame
{
    private readonly object value;

    public readonly WebSocketMessageType MessageType { get; }

    /// <summary>
    /// Indicates whether reading this gateway frame was successful.
    /// </summary>
    public readonly bool IsSuccess => this.value is byte[];

    /// <summary>
    /// Attempts to retrieve the string message received.
    /// </summary>
    public readonly bool TryGetMessage([NotNullWhen(true)] out byte[]? message)
    {
        message = null;

        if (this.value is byte[] value)
        {
            message = value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve the exception thrown attempting to read this frame.
    /// </summary>
    public readonly bool TryGetException<T>([NotNullWhen(true)] out T? exception)
        where T : Exception
    {
        exception = null;

        if (this.value is T ex)
        {
            exception = ex;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve the error code returned attempting to read this frame.
    /// </summary>
    public readonly bool TryGetErrorCode(out int errorCode)
    {
        errorCode = default;

        if (this.value is int code)
        {
            errorCode = code;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a new transport frame from the specified data.
    /// </summary>
    public TransportFrame(object value, WebSocketMessageType type)
    {
        this.value = value;
        this.MessageType = type;
    }
}
