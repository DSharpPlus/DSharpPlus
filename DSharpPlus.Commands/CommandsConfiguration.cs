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
    /// The guild id to use for debugging. Leave as 0 to disable.
    /// </summary>
    public ulong DebugGuildId { get; set; }

    /// <summary>
    /// Whether to enable the default command error handler.
    /// </summary>
    public bool UseDefaultCommandErrorHandler { get; set; } = true;

    /// <summary>
    /// Whether to register default command processors when they're not found in the processor list.
    /// </summary>
    /// <remarks>
    /// You may still provide your own custom processors via <see cref="CommandsExtension.AddProcessorsAsync(Processors.ICommandProcessor[])"/>,
    /// as this configuration option will only add the default processors if they're not found in the list.
    /// </remarks>
    public bool RegisterDefaultCommandProcessors { get; set; } = true;
}
