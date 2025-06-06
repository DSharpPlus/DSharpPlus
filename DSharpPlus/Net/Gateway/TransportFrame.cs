using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Represents an union between a string message read from the gateway, an exception or a close code.
/// </summary>
public readonly record struct TransportFrame
{
    private readonly object value;

    /// <summary>
    /// Indicates whether reading this gateway frame was successful.
    /// </summary>
    public readonly bool IsSuccess => this.value is string or MemoryStream;

    /// <summary>
    /// Attempts to retrieve the string message received.
    /// </summary>
    public readonly bool TryGetMessage([NotNullWhen(true)] out string? message)
    {
        message = null;

        if (this.value is string str)
        {
            message = str;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve a message wrapped in a MemoryStream. This is only permissible if the log level
    /// is higher than Trace or if inbound gateway logging is disabled.
    /// </summary>
    public readonly bool TryGetStreamMessage([NotNullWhen(true)] out MemoryStream? message)
    {
        message = null;

        if (this.value is MemoryStream str)
        {
            message = str;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve the exception thrown attempting to read this frame.
    /// </summary>
    public readonly bool TryGetException([NotNullWhen(true)] out Exception? exception)
    {
        exception = null;

        if (this.value is Exception ex)
        {
            exception = ex;
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
    public TransportFrame(object value) 
        => this.value = value;
}
