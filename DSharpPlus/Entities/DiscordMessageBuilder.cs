﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs a Message to be sent.
    /// </summary>
    public sealed class DiscordMessageBuilder
    {
        /// <summary>
        /// Gets or Sets the Message to be sent.
        /// </summary>
        public string Content
        {
            get => this._content;
            set
            {
                if (value != null && value.Length > 2000)
                    throw new ArgumentException("Content cannot exceed 2000 characters.", nameof(value));
                this._content = value;
            }
        }
        private string _content;

        /// <summary>
        /// Gets or Sets the Embed to be sent.
        /// </summary>
        public DiscordEmbed Embed { get; set; }

        /// <summary>
        /// Gets or Sets if the message should be TTS.
        /// </summary>
        public bool IsTTS { get; set; } = false;

        /// <summary>
        /// Gets the Allowed Mentions for the message to be sent. 
        /// </summary>
        public List<IMention> Mentions { get; private set; } = null;

        /// <summary>
        /// Gets the Files to be sent in the Message.
        /// </summary>
        public IReadOnlyDictionary<string, Stream> Files => this._files;

        internal Dictionary<string, Stream> _files = new Dictionary<string, Stream>();

        /// <summary>
        /// Gets the Reply Message ID.
        /// </summary>
        public ulong? ReplyId { get; private set; } = null;

        /// <summary>
        /// Gets if the Reply should mention the user.
        /// </summary>
        public bool MentionOnReply { get; private set; } = false;

        /// <summary>
        /// Sets the Content of the Message.
        /// </summary>
        /// <param name="content">The content to be set.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithContent(string content)
        {
            this.Content = content;
            return this;
        } 

        /// <summary>
        /// Sets if the message should be TTS.
        /// </summary>
        /// <param name="isTTS">If TTS should be set.</param>
        /// <returns></returns>
        public DiscordMessageBuilder HasTTS(bool isTTS)
        {
            this.IsTTS = isTTS;
            return this;
        }

        /// <summary>
        /// Sets if the message will have an Embed.
        /// </summary>
        /// <param name="embed">The embed that should be sent.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithEmbed(DiscordEmbed embed)
        {
            this.Embed = embed;
            return this;
        }

        /// <summary>
        /// Sets if the message has allowed mentions.
        /// </summary>
        /// <param name="allowedMention">The allowed Mention that should be sent.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithAllowedMention(IMention allowedMention)
        {
            if (this.Mentions != null)
                this.Mentions.Add(allowedMention);
            else
                this.Mentions = new List<IMention> { allowedMention };

            return this;
        }

        /// <summary>
        /// Sets if the message has allowed mentions.
        /// </summary>
        /// <param name="allowedMentions">The allowed Mentions that should be sent.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithAllowedMentions(IEnumerable<IMention> allowedMentions)
        {
            if (this.Mentions != null)
                this.Mentions.AddRange(allowedMentions);
            else
                this.Mentions = allowedMentions.ToList();

            return this;
        }               

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="fileName">The fileName that the file should be sent as.</param>
        /// <param name="stream">The Stream to the file.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithFile(string fileName, Stream stream)
        {
            if(this.Files.Count() >= 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            this._files.Add(fileName, stream);

            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="stream">The Stream to the file.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithFile(FileStream stream)
        {
            if (this.Files.Count() >= 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            this._files.Add(stream.Name, stream);

            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithFile(string filePath)
        {
            if (this.Files.Count() >= 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            var fs = File.OpenRead(filePath);
            this._files.Add(fs.Name, fs);

            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="files">The Files that should be sent.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithFiles(Dictionary<string, Stream> files)
        {
            if (this.Files.Count() + files.Count() >= 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            foreach (var file in files)
                this._files.Add(file.Key, file.Value);

            return this;
        }

        /// <summary>
        /// Sets if the message is a reply
        /// </summary>
        /// <param name="messageId">The ID of the message to reply to.</param>
        /// <param name="mention">If we should mention the user in the reply.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithReply(ulong messageId, bool mention = false)
        {
            this.ReplyId = messageId;
            this.MentionOnReply = mention;

            return this;
        }

        /// <summary>
        /// Sends the Message to a specific channel
        /// </summary>
        /// <param name="channel">The channel the message should be sent to.</param>
        /// <returns></returns>
        public async Task<DiscordMessage> SendAsync(DiscordChannel channel)
        {
            return await channel.SendMessageAsync(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the modified message.
        /// </summary>
        /// <param name="msg">The original Message to modify.</param>
        /// <returns></returns>
        public async Task<DiscordMessage> ModifyAsync(DiscordMessage msg)
        {
            return await msg.ModifyAsync(this).ConfigureAwait(false);
        }
    }
}
