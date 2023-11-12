namespace DSharpPlus.Commands;

using System;

/// <summary>
/// The configuration copied to an instance of <see cref="CommandsExtension"/>.
/// </summary>
public sealed record CommandsConfiguration
{
    /// <summary>
    /// The service provider to use for dependency injection.
    /// </summary>
    public required IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// The guild id to use for debugging.
    /// </summary>
    public ulong? DebugGuildId { get; set; }
}
