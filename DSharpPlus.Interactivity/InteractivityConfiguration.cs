using DSharpPlus.Interactivity.Enums;
using System;

namespace DSharpPlus.Interactivity
{
    /// <summary>
    /// Configuration class for your Interactivity extension
    /// </summary>
    public sealed class InteractivityConfiguration
    {
        /// <summary>
        /// <para>Sets the default interactivity action timeout.</para>
        /// <para>Defaults to 1 minute.</para>
        /// </summary>
        public TimeSpan Timeout { internal get; set; } = TimeSpan.FromMinutes(1);

        public PollBehaviour PolBehaviour { internal get; set; } = PollBehaviour.DeleteEmojis;
        /// <summary>
        /// Creates a new instance of <see cref="InteractivityConfiguration"/>.
        /// </summary>
        public InteractivityConfiguration() { }

        /// <summary>
        /// Creates a new instance of <see cref="InteractivityConfiguration"/>, copying the properties of another configuration.
        /// </summary>
        /// <param name="other">Configuration the properties of which are to be copied.</param>
        public InteractivityConfiguration(InteractivityConfiguration other)
        {
            this.Timeout = other.Timeout;
        }
    }
}
