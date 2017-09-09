using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

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
        public DiscordChannel Channel => this.Message.Channel;

        /// <summary>
        /// Gets the guild in which the execution was triggered. This property is null for commands sent over direct messages.
        /// </summary>
        public DiscordGuild Guild => this.Channel.Guild;

        /// <summary>
        /// Gets the user who triggered the execution.
        /// </summary>
        public DiscordUser User => this.Message.Author;

        /// <summary>
        /// Gets the member who triggered the execution. This property is null for commands sent over direct messages.
        /// </summary>
        public DiscordMember Member => this._lazy_ass_member.Value;
        private Lazy<DiscordMember> _lazy_ass_member;

        /// <summary>
        /// Gets the CommandsNext service instance that handled this command.
        /// </summary>
        public CommandsNextModule CommandsNext { get; internal set; }

        /// <summary>
        /// Gets the collection of dependencies for this CNext instance.
        /// </summary>
        public DependencyCollection Dependencies { get; internal set; }

        /// <summary>
        /// Gets the command that is being executed.
        /// </summary>
        public Command Command { get; internal set; }

        /*/// <summary>
        /// Gets the list of raw arguments passed to the command.
        /// </summary>
        public IReadOnlyList<string> RawArguments { get; internal set; }*/

        /// <summary>
        /// Gets the raw argument string passed to the command.
        /// </summary>
        public string RawArgumentString { get; internal set; }

        internal CommandsNextConfiguration Config { get; set; }

        internal CommandContext()
        {
            this._lazy_ass_member = new Lazy<DiscordMember>(() => this.Guild?.Members.FirstOrDefault(xm => xm.Id == this.User.Id) ?? this.Guild?.GetMemberAsync(this.User.Id).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Quickly respond to the message that triggered the command.
        /// </summary>
        /// <param name="content">Message to respond with.</param>
        /// <param name="is_tts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach.</param>
        /// <returns></returns>
        public Task<DiscordMessage> RespondAsync(string content = null, bool is_tts = false, DiscordEmbed embed = null) =>
            this.Message.RespondAsync(content, is_tts, embed);

        /// <summary>
        /// Quickly respond with a file to the message that triggered the command.
        /// </summary>
        /// <param name="file_data">Stream containing the data to attach as a file.</param>
        /// <param name="file_name">Name of the file to send.</param>
        /// <param name="content">Message to respond with.</param>
        /// <param name="is_tts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(Stream file_data, string file_name, string content = null, bool is_tts = false, DiscordEmbed embed = null) =>
            this.Message.RespondWithFileAsync(file_data, file_name, content, is_tts, embed);

#if !NETSTANDARD1_1
        /// <summary>
        /// Quickly respond with a file to the message that triggered the command.
        /// </summary>
        /// <param name="file_data">Stream containing the data to attach as a file.</param>
        /// <param name="content">Message to respond with.</param>
        /// <param name="is_tts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(FileStream file_data, string content = null, bool is_tts = false, DiscordEmbed embed = null) =>
            this.Message.RespondWithFileAsync(file_data, content, is_tts, embed);

        /// <summary>
        /// Quickly respond with a file to the message that triggered the command.
        /// </summary>
        /// <param name="file_path">Path to the file to be attached to the message.</param>
        /// <param name="content">Message to respond with.</param>
        /// <param name="is_tts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(string file_path, string content = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            return this.Message.RespondWithFileAsync(file_path, content, is_tts, embed);
        }
#endif

        /// <summary>
        /// Quickly respond with multiple files to the message that triggered the command.
        /// </summary>
        /// <param name="content">Message to respond with.</param>
        /// <param name="files">Files to send.</param>
        /// <param name="is_tts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFilesAsync(Dictionary<string, Stream> files, string content = null, bool is_tts = false, DiscordEmbed embed = null) =>
            this.Message.RespondWithFilesAsync(files, content, is_tts, embed);

        /// <summary>
        /// Triggers typing in the channel containing the message that triggered the command.
        /// </summary>
        /// <returns></returns>
        public Task TriggerTypingAsync() =>
            this.Channel.TriggerTypingAsync();
    }
}
