using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Defines various extension methods for slash commands
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Enables slash commands on this <see cref="DiscordClient"/>
        /// </summary>
        /// <param name="client">Client to enable slash commands for</param>
        /// <param name="config">Configuration to use</param>
        /// <returns>Created <see cref="SlashCommandsExtension"/></returns>
        public static SlashCommandsExtension UseSlashCommands(this DiscordClient client)
        {
            if (client.GetExtension<SlashCommandsExtension>() != null)
                throw new InvalidOperationException("Slash commands are already enabled for that client.");

            var scomm = new SlashCommandsExtension();
            client.AddExtension(scomm);
            return scomm;
        }

        /// <summary>
        /// Gets the slash commands module for this client
        /// </summary>
        /// <param name="client">Client to get slash commands for</param>
        /// <returns>The module, or null if not activated</returns>
        public static SlashCommandsExtension GetSlashCommands(this DiscordClient client)
            => client.GetExtension<SlashCommandsExtension>();
    }
}