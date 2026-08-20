using System;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Indicates that the gateway has entered an invalid state and must reconnect.
/// </summary>
public sealed class InvalidGatewayStateException(string message) : Exception(message);
