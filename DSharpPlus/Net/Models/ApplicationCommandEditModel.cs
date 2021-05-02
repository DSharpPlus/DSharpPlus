using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models
{
    public class ApplicationCommandEditModel
    {
        /// <summary>
        /// Sets the command's new name.
        /// </summary>
        public Optional<string> Name
        {
            internal get => this._name;
            set
            {
                if (value.Value.Length > 32)
                    throw new ArgumentException("Slash command name cannot exceed 32 characters.", nameof(value));
                this._name = value;
            }
        }
        private Optional<string> _name;

        /// <summary>
        /// Sets the command's new description
        /// </summary>
        public Optional<string> Description
        {
            internal get => this._description;
            set
            {
                if (value.Value.Length > 100)
                    throw new ArgumentException("Slash command description cannot exceed 100 characters.", nameof(value));
                this._description = value;
            }
        }
        private Optional<string> _description;

        /// <summary>
        /// Sets the command's new options.
        /// </summary>
        public Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> Options { internal get; set; }
    }
}
