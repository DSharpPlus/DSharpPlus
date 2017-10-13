using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

// ReSharper disable once CheckNamespace
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
        public DiscordChannel Channel => Message.Channel;

        /// <summary>
        /// Gets the guild in which the execution was triggered. This property is null for commands sent over direct messages.
        /// </summary>
        public DiscordGuild Guild => Channel.Guild;

        /// <summary>
        /// Gets the user who triggered the execution.
        /// </summary>
        public DiscordUser User => Message.Author;

        /// <summary>
        /// Gets the member who triggered the execution. This property is null for commands sent over direct messages.
        /// </summary>
        public DiscordMember Member => _lazyAssMember.Value;
        private readonly Lazy<DiscordMember> _lazyAssMember;

        /// <summary>
        /// Gets the CommandsNext service instance that handled this command.
        /// </summary>
        public CommandsNextExtension CommandsNext { get; internal set; }

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
            _lazyAssMember = new Lazy<DiscordMember>(() => Guild?.Members.FirstOrDefault(xm => xm.Id == User.Id) ?? Guild?.GetMemberAsync(User.Id).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Quickly respond to the message that triggered the command.
        /// </summary>
        /// <param name="content">Message to respond with.</param>
        /// <param name="isTts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach.</param>
        /// <returns></returns>
        public Task<DiscordMessage> RespondAsync(string content = null, bool isTts = false, DiscordEmbed embed = null) =>
            Message.RespondAsync(content, isTts, embed);

        /// <summary>
        /// Quickly respond with a file to the message that triggered the command.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach as a file.</param>
        /// <param name="fileName">Name of the file to send.</param>
        /// <param name="content">Message to respond with.</param>
        /// <param name="isTts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(Stream fileData, string fileName, string content = null, bool isTts = false, DiscordEmbed embed = null) =>
            Message.RespondWithFileAsync(fileData, fileName, content, isTts, embed);

#if !NETSTANDARD1_1
        /// <summary>
        /// Quickly respond with a file to the message that triggered the command.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach as a file.</param>
        /// <param name="content">Message to respond with.</param>
        /// <param name="isTts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(FileStream fileData, string content = null, bool isTts = false, DiscordEmbed embed = null) =>
            Message.RespondWithFileAsync(fileData, content, isTts, embed);

        /// <summary>
        /// Quickly respond with a file to the message that triggered the command.
        /// </summary>
        /// <param name="filePath">Path to the file to be attached to the message.</param>
        /// <param name="content">Message to respond with.</param>
        /// <param name="isTts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(string filePath, string content = null, bool isTts = false, DiscordEmbed embed = null)
        {
            return Message.RespondWithFileAsync(filePath, content, isTts, embed);
        }
#endif

        /// <summary>
        /// Quickly respond with multiple files to the message that triggered the command.
        /// </summary>
        /// <param name="content">Message to respond with.</param>
        /// <param name="files">Files to send.</param>
        /// <param name="isTts">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFilesAsync(Dictionary<string, Stream> files, string content = null, bool isTts = false, DiscordEmbed embed = null) =>
            Message.RespondWithFilesAsync(files, content, isTts, embed);

        /// <summary>
        /// Triggers typing in the channel containing the message that triggered the command.
        /// </summary>
        /// <returns></returns>
        public Task TriggerTypingAsync() =>
            Channel.TriggerTypingAsync();
    }
}
