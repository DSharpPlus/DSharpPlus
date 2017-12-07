using System;

namespace DSharpPlus.Interactivity
{
    public sealed class InteractivityConfiguration
    {
        /// <summary>
        /// <para>Sets the default interactivity action timeout.</para>
        /// <para>Defaults to 1 minute.</para>
        /// </summary>
        public TimeSpan Timeout { internal get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// <para>Sets the default pagination timeout.</para>
        /// <para>Defaults to 2 minutes.</para>
        /// </summary>
        public TimeSpan PaginationTimeout { internal get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// <para>Sets the default pagination timeout behaviour.</para>
        /// <para>Defaults to <see cref="TimeoutBehaviour.Ignore"/>.</para>
        /// </summary>
        public TimeoutBehaviour PaginationBehavior { internal get; set; } = TimeoutBehaviour.Ignore;

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
            this.PaginationBehavior = other.PaginationBehavior;
            this.PaginationTimeout = other.PaginationTimeout;
            this.Timeout = other.Timeout;
        }
    }
}
