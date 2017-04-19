namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Represents a configuration for <see cref="CommandsNextModule"/>.
    /// </summary>
    public sealed class CommandsNextConfig
    {
        /// <summary>
        /// Gets or sets the prefix used for commands. By default has no value.
        /// </summary>
        public string Prefix { get; set; } = null;

        /// <summary>
        /// Gets or sets whether to allow bot's mention as command prefix. Defaults to true.
        /// </summary>
        public bool MentionPrefix { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the bot should only respond to messages from its own account. This is used for selfbots. Defaults to false.
        /// </summary>
        public bool SelfBot { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the commands should be case-sensitive. Defaults to true.
        /// </summary>
        public bool CaseSensitive { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to enable default help command. Disable this if you want to make your own help command. Defaults to true.
        /// </summary>
        public bool EnableDefaultHelp { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to enable commands via direct messages. Defaults to true.
        /// </summary>
        public bool EnableDms { get; set; } = true;
    }
}
