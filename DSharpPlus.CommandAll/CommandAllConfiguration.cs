namespace DSharpPlus.CommandAll;

using System;

/// <summary>
/// The configuration copied to an instance of <see cref="CommandAllExtension"/>.
/// </summary>
public sealed record CommandAllConfiguration
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
