using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs a membership screening form
    /// </summary>
    public sealed class DiscordMembershipScreeningBuilder
    {
        /// <summary>
        /// Gets or sets whether membership screening should be enabled for this guild.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the server description shown in the membership screening form.
        /// </summary>
        public string Description { get; set; } = null;

        /// <summary>
        /// Gets the terms in this membership screening form.
        /// </summary>
        public DiscordGuildMembershipScreeningField Terms { get; private set; } = null;

        internal DiscordGuildMembershipScreeningField[] Fields
        {
            get => new DiscordGuildMembershipScreeningField[] { Terms };
        }

        /// <summary>
        /// Sets whether membership screening should be enabled for this guild. 
        /// </summary>
        /// <param name="isEnabled">Whether membership screening should be enabled for this guild.</param>
        /// <returns></returns>
        public DiscordMembershipScreeningBuilder IsEnabled(bool isEnabled)
        {
            this.Enabled = isEnabled;
            return this;
        }

        /// <summary>
        /// Sets the server description shown in the membership screening form.
        /// </summary>
        /// <param name="description">The description of the server to show in the membership screening form.</param>
        /// <returns></returns>
        public DiscordMembershipScreeningBuilder WithDescription(string description)
        {
            this.Description = description;
            return this;
        }

        /// <summary>
        /// Sets the list of rules in this membership screening form.
        /// </summary>
        /// <param name="label">The title of the field.</param>
        /// <param name="values">The list of rules in this membership screening form.</param>
        /// <param name="required">Whether the user has to fill out this field.</param>
        /// <returns></returns>
        public DiscordMembershipScreeningBuilder WithTerms(string label, string[] values, bool required)
        {
            this.Terms = new DiscordGuildMembershipScreeningField()
            {
                Type = "TERMS",
                Label = label,
                Values = values,
                IsRequired = required
            };
            return this;
        }
    }
}
