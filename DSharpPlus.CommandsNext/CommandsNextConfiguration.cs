using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

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
        /// Sets the string prefix used for commands. By default has no value.
        /// </summary>
        public string StringPrefix { internal get; set; } = null;

        /// <summary>
        /// Sets the custom prefix predicate used for commands. By default is not specified.
        /// </summary>
        public CustomPrefixPredicate CustomPrefixPredicate { internal get; set; } = null;

        /// <summary>
        /// Sets whether to allow bot's mention as command prefix. Defaults to true.
        /// </summary>
        public bool EnableMentionPrefix { internal get; set; } = true;

        /// <summary>
        /// Sets whether the bot should only respond to messages from its own account. This is used for selfbots. Defaults to false.
        /// </summary>
        public bool SelfBot { internal get; set; } = false;

        /// <summary>
        /// Sets whether the commands should be case-sensitive. Defaults to true.
        /// </summary>
        public bool CaseSensitive { internal get; set; } = true;

        /// <summary>
        /// Sets whether to enable default help command. Disable this if you want to make your own help command. Defaults to true.
        /// </summary>
        public bool EnableDefaultHelp { internal get; set; } = true;

        /// <summary>
        /// Sets the default pre-execution checks for the built-in help command. Only applicable if default help is enabled.
        /// </summary>
        public IEnumerable<CheckBaseAttribute> DefaultHelpChecks { internal get; set; } = null;

        /// <summary>
        /// Sets whether to enable commands via direct messages. Defaults to true.
        /// </summary>
        public bool EnableDms { internal get; set; } = true;

        /// <summary>
        /// Sets the dependency collection for this CommandsNext instance.
        /// </summary>
        public DependencyCollection Dependencies { internal get; set; } = null;

        /// <summary>
        /// Gets whether any extra arguments passed to commands should be ignored or not. If this is set to false, extra arguments will throw. Defaults to true.
        /// </summary>
        public bool IgnoreExtraArguments { internal get; set; } = true;
    }
}
