using System;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// A configuration for a <see cref="SlashCommandsExtension"/>.
    /// </summary>
    public sealed class SlashCommandsConfiguration
    {
        /// <summary>
        /// <para>Sets the service provider.</para>
        /// <para>Objects in this provider are used when instantiating slash command modules. This allows passing data around without resorting to static members.</para>
        /// <para>Defaults to null.</para>
        /// </summary>
        public IServiceProvider Services { internal get; set; } = new ServiceCollection().BuildServiceProvider(true);
    }
}
