using System;

namespace DSharpPlus;

/// <summary>
/// Holds a delegate to obtain a token to initialize the application with.
/// </summary>
public sealed class TokenContainer
{
    /// <summary>
    /// Gets the token for this application.
    /// </summary>
    public required Func<string> GetToken { get; set; }
}
