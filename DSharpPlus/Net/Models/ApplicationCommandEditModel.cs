using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models
{
    public class ApplicationCommandEditModel
    {
        /// <summary>
        /// Sets the command's new name.
        /// </summary>
        public Optional<string> Name { internal get; set; }

        /// <summary>
        /// Sets the command's new description
        /// </summary>
        public Optional<string> Description { internal get; set; }

        /// <summary>
        /// Sets the command's new options.
        /// </summary>
        public Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> Options { internal get; set; }
    }
}
