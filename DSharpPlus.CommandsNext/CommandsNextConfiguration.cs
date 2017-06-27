using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Represents a delegate for a function that takes a message, and returns the position of the start of command invocation in the message. It has to return -1 if prefix is not present.
    /// </summary>
    /// <param name="msg">Message to check for prefix.</param>
    /// <returns>Position of the command invocation or -1 if not present.</returns>
    public delegate Task<int> CustomPrefixPredicate(DiscordMessage msg);

    /// <summary>
    /// Represents a configuration for <see cref="CommandsNextModule"/>.
    /// </summary>
    public sealed class CommandsNextConfiguration
    {
        /// <summary>
        /// Gets or sets the string prefix used for commands. By default has no value.
        /// </summary>
        public string StringPrefix { get; set; } = null;

        /// <summary>
        /// Gets or sets the custom prefix predicate used for commands. By default is not specified.
        /// </summary>
        public CustomPrefixPredicate CustomPrefixPredicate { get; set; } = null;

        /// <summary>
        /// Gets or sets whether to allow bot's mention as command prefix. Defaults to true.
        /// </summary>
        public bool EnableMentionPrefix { get; set; } = true;

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
