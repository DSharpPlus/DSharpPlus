using System;

namespace DSharpPlus.Commands.Exceptions;

/// <summary>
/// Thrown if the extension failed to register application commands and an attempt was made to execute an application command.
/// </summary>
public class CommandRegistrationFailedException : Exception;
