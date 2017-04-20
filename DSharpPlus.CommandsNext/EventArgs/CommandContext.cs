using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        /// <summary>
        /// Quickly respond to the message that triggered the command.
        /// </summary>
        /// <param name="content">Message to respond with.</param>
        /// <param name="is_tts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach.</param>
        /// <returns></returns>
        public Task<DiscordMessage> RespondAsync(string content, bool is_tts = false, DiscordEmbed embed = null) =>
            this.Message.Respond(content, is_tts, embed);

        /// <summary>
        /// Quickly respond with a file to the message that triggered the command.
        /// </summary>
        /// <param name="content">Message to respond with.</param>
        /// <param name="file_data">File to send.</param>
        /// <param name="file_name">Name of the file to send.</param>
        /// <param name="is_tts">Whether the message is to be spoken aloud.</param>
        /// <returns></returns>
        public Task<DiscordMessage> RespondAsync(string content, Stream file_data, string file_name, bool is_tts = false) =>
            this.Message.Respond(content, file_data, file_name, is_tts);
    }
}
