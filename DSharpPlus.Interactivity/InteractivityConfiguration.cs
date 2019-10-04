using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
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

        /// <summary>
        /// What to do after the poll ends
        /// </summary>
        public PollBehaviour PollBehaviour { internal get; set; } = PollBehaviour.DeleteEmojis;

        /// <summary>
        /// Emojis to use for pagination
        /// </summary>
        public PaginationEmojis PaginationEmojis { internal get; set; } = new PaginationEmojis();

        /// <summary>
        /// How to handle pagination. Defaults to WrapAround.
        /// </summary>
        public PaginationBehaviour PaginationBehaviour { internal get; set; } = PaginationBehaviour.WrapAround;

        /// <summary>
        /// How to handle pagination deletion. Defaults to DeleteEmojis.
        /// </summary>
        public PaginationDeletion PaginationDeletion { internal get; set; } = PaginationDeletion.DeleteEmojis;

        /// <summary>
        /// Creates a new instance of <see cref="InteractivityConfiguration"/>.
        /// </summary>
        public InteractivityConfiguration() {
        }

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
