using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

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
        public DiscordChannel Channel 
            => this.Message.Channel;

        /// <summary>
        /// Gets the guild in which the execution was triggered. This property is null for commands sent over direct messages.
        /// </summary>
        public DiscordGuild Guild 
            => this.Channel.Guild;

        /// <summary>
        /// Gets the user who triggered the execution.
        /// </summary>
        public DiscordUser User 
            => this.Message.Author;

        /// <summary>
        /// Gets the member who triggered the execution. This property is null for commands sent over direct messages.
        /// </summary>
        public DiscordMember Member 
            => this._lazyAssMember.Value;

        private Lazy<DiscordMember> _lazyAssMember;

        /// <summary>
        /// Gets the CommandsNext service instance that handled this command.
        /// </summary>
        public CommandsNextExtension CommandsNext { get; internal set; }

        /// <summary>
        /// Gets the service provider for this CNext instance.
        /// </summary>
        public IServiceProvider Services { get; internal set; }

        /// <summary>
        /// Gets the command that is being executed.
        /// </summary>
        public Command Command { get; internal set; }

        /// <summary>
        /// Gets the overload of the command that is being executed.
        /// </summary>
        public CommandOverload Overload { get; internal set; }

        /// <summary>
        /// Gets the list of raw arguments passed to the command.
        /// </summary>
        public IReadOnlyList<string> RawArguments { get; internal set; }

        /// <summary>
        /// Gets the raw string from which the arguments were extracted.
        /// </summary>
        public string RawArgumentString { get; internal set; }

        /// <summary>
        /// Gets the prefix used to invoke the command.
        /// </summary>
        public string Prefix { get; internal set; }

        internal CommandsNextConfiguration Config { get; set; }

        internal ServiceContext ServiceScopeContext { get; set; }

        internal CommandContext()
        {
            this._lazyAssMember = new Lazy<DiscordMember>(() => this.Guild?.Members.FirstOrDefault(xm => xm.Id == this.User.Id) ?? this.Guild?.GetMemberAsync(this.User.Id).ConfigureAwait(false).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Quickly respond to the message that triggered the command.
        /// </summary>
        /// <param name="content">Message to respond with.</param>
        /// <param name="isTTS">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach.</param>
        /// <returns></returns>
        public Task<DiscordMessage> RespondAsync(string content = null, bool isTTS = false, DiscordEmbed embed = null) 
            => this.Message.RespondAsync(content, isTTS, embed);

        /// <summary>
        /// Quickly respond with a file to the message that triggered the command.
        /// </summary>
        /// <param name="fileName">Name of the file to send.</param>
        /// <param name="fileData">Stream containing the data to attach as a file.</param>
        /// <param name="content">Message to respond with.</param>
        /// <param name="isTTS">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(string fileName, Stream fileData, string content = null, bool isTTS = false, DiscordEmbed embed = null) 
            => this.Message.RespondWithFileAsync(fileName, fileData, content, isTTS, embed);

#if !NETSTANDARD1_1
        /// <summary>
        /// Quickly respond with a file to the message that triggered the command.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach as a file.</param>
        /// <param name="content">Message to respond with.</param>
        /// <param name="isTTS">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(FileStream fileData, string content = null, bool isTTS = false, DiscordEmbed embed = null) 
            => this.Message.RespondWithFileAsync(fileData, content, isTTS, embed);

        /// <summary>
        /// Quickly respond with a file to the message that triggered the command.
        /// </summary>
        /// <param name="filePath">Path to the file to be attached to the message.</param>
        /// <param name="content">Message to respond with.</param>
        /// <param name="isTTS">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(string filePath, string content = null, bool isTTS = false, DiscordEmbed embed = null)
        {
            return this.Message.RespondWithFileAsync(filePath, content, isTTS, embed);
        }
#endif

        /// <summary>
        /// Quickly respond with multiple files to the message that triggered the command.
        /// </summary>
        /// <param name="content">Message to respond with.</param>
        /// <param name="files">Files to send.</param>
        /// <param name="isTTS">Whether the message is to be spoken aloud.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>Message that was sent.</returns>
        public Task<DiscordMessage> RespondWithFilesAsync(Dictionary<string, Stream> files, string content = null, bool isTTS = false, DiscordEmbed embed = null) 
            => this.Message.RespondWithFilesAsync(files, content, isTTS, embed);

        /// <summary>
        /// Triggers typing in the channel containing the message that triggered the command.
        /// </summary>
        /// <returns></returns>
        public Task TriggerTypingAsync() 
            => this.Channel.TriggerTypingAsync();

        internal struct ServiceContext : IDisposable
        {
            public IServiceProvider Provider { get; }
            public IServiceScope Scope { get; }
            public bool IsInitialized { get; }

            public ServiceContext(IServiceProvider services, IServiceScope scope)
            {
                this.Provider = services;
                this.Scope = scope;
                this.IsInitialized = true;
            }

            public void Dispose()
            {
                this.Scope?.Dispose();
            }
        }
    }
}
