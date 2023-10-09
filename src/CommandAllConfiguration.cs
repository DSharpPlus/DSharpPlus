using System;
using DSharpPlus.CommandAll.EventProcessors;
using DSharpPlus.CommandAll.EventProcessors.SlashCommands;

namespace DSharpPlus.CommandAll
{
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
        /// The configuration to use for text commands.
        /// </summary>
        public required TextCommandsConfiguration TextCommandsConfiguration { get; set; }

        /// <summary>
        /// The configuration to use for slash commands.
        /// </summary>
        public required SlashCommandConfiguration SlashCommandsConfiguration { get; set; }

        /// <summary>
        /// The guild id to use for debugging.
        /// </summary>
        public ulong? DebugGuildId { get; set; }
    }
}
