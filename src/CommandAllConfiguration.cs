using System;

namespace DSharpPlus.CommandAll
{
    /// <summary>
    /// The configuration copied to an instance of <see cref="CommandAllExtension"/>.
    /// </summary>
    public sealed record CommandAllConfiguration
    {
        /// <summary>
        /// The guild id to use for debugging.
        /// </summary>
        public ulong? DebugGuildId { get; init; }

        /// <summary>
        /// The service provider to use for dependency injection.
        /// </summary>
        public IServiceProvider ServiceProvider { get; init; }
    }
}
