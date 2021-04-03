using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.Entities.Builders
{
    /// <summary>
    /// Represents a 
    /// </summary>
    public sealed class WelcomeScreenBuilder
    {
        /// <summary>
        /// Sets whether the welcome screen should be enabled.
        /// </summary>
        public Optional<bool> Enabled { internal get; set; }

        /// <summary>
        /// Sets the welcome channels.
        /// </summary>
        public Optional<IEnumerable<DiscordGuildWelcomeScreenChannel>> WelcomeChannels { internal get; set; }

        /// <summary>
        /// Sets the serer description shown.
        /// </summary>
        public Optional<string> Description { internal get; set; }

        public WelcomeScreenBuilder WithEnabled(bool enabled)
        {
            this.Enabled = Optional.FromValue(enabled);
            return this;
        }

        public WelcomeScreenBuilder WithWelcomeChannel(bool enabled)
        {
            this.Enabled = Optional.FromValue(enabled);
            return this;
        }
    }
}
