namespace DSharpPlus
{
    /// <summary>
    /// Represents base for all DSharpPlus modules. To implement your own module, extend this class.
    /// </summary>
    public abstract class BaseModule
    {
        /// <summary>
        /// Gets the instance of <see cref="DiscordClient"/> this module is attached to.
        /// </summary>
        public DiscordClient Client { get; protected set; }

        /// <summary>
        /// Initializes this module for given <see cref="DiscordClient"/> instance.
        /// </summary>
        /// <param name="client">Discord client to initialize for.</param>
        protected internal abstract void Setup(DiscordClient client);
    }
}
