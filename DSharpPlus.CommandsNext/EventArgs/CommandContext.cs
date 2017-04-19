using System.Collections.Generic;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Represents a context in which a command is executed.
    /// </summary>
    public sealed class CommandContext
    {
        /// <summary>
        /// Gets the client which received the message.
        /// </summary>
        public DiscordClient Client { get; internal set; }
        
        /// <summary>
        /// Gets the message that triggered the execution.
        /// </summary>
        public DiscordMessage Message { get; internal set; }
        
        /// <summary>
        /// Gets the channel in which the execution was triggered,
        /// </summary>
        public DiscordChannel Channel => this.Message.Parent;

        /// <summary>
        /// Gets the guild in which the execution was triggered. This property is null for commands sent over direct messages.
        /// </summary>
        public DiscordGuild Guild => this.Channel.Parent;

        /// <summary>
        /// Gets the user who triggered the execution.
        /// </summary>
        public DiscordUser User => this.Message.Author;

        /// <summary>
        /// Gets the member who triggered the execution. This property is null for commands sent over direct messages.
        /// </summary>
        public DiscordMember Member => this.Guild?.GetMember(this.User.ID).GetAwaiter().GetResult();

        /// <summary>
        /// Gets the command that is being executed.
        /// </summary>
        public Command Command { get; internal set; }

        /// <summary>
        /// Gets the list of raw arguments passed to the command.
        /// </summary>
        public IReadOnlyList<string> RawArguments { get; internal set; }
    }
}
