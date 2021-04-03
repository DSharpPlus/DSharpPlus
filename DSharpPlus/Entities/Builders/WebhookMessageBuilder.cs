using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs ready-to-send webhook requests.
    /// </summary>
    public abstract class WebhookMessageBuilder<T>
    {        
        /// <summary>
        /// Whether this webhook request is text-to-speech.
        /// </summary>
        public bool IsTTS { get; set; }

        /// <summary>
        /// Message to send on this webhook request.
        /// </summary>
        public string Content
        {
            get => this._content;
            set
            {
                if (value != null && value.Length > 2000)
                    throw new ArgumentException("Content length cannot exceed 2000 characters.", nameof(value));
                this._content = value;
            }
        }
        internal string _content;
        
        /// <summary>
        /// Embeds to send on this webhook request.
        /// </summary>
        public IReadOnlyList<DiscordEmbed> Embeds { get; }
        internal readonly List<DiscordEmbed> _embeds = new List<DiscordEmbed>();

        /// <summary>
        /// Mentions to send on this webhook request.
        /// </summary>
        public IEnumerable<IMention> Mentions { get; }
        internal readonly List<IMention> _mentions = new List<IMention>();

        /// <summary>
        /// Constructs a new empty webhook request builder.
        /// </summary>
        public WebhookMessageBuilder()
        {
            this.Embeds = new ReadOnlyCollection<DiscordEmbed>(this._embeds);
            this.Mentions = new ReadOnlyCollection<IMention>(this._mentions);
        }

        /// <summary>
        /// Indicates if the webhook must use text-to-speech.
        /// </summary>
        /// <param name="tts">Text-to-speech</param>
        public T WithTTS(bool tts)
        {
            this.IsTTS = tts;
            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the message to send at the execution of the webhook.
        /// </summary>
        /// <param name="content">Message to send.</param>
        public T WithContent(string content)
        {
            this.Content = content;
            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Adds an embed to send at the execution of the webhook.
        /// </summary>
        /// <param name="embed">Embed to add.</param>
        public T AddEmbed(DiscordEmbed embed)
        {
            this._embeds.Add(embed);
            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Adds the given embeds to send at the execution of the webhook.
        /// </summary>
        /// <param name="embeds">Embeds to add.</param>
        public T AddEmbeds(IEnumerable<DiscordEmbed> embeds)
        {
            this._embeds.AddRange(embeds);
            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Adds the mention to the mentions to parse, etc. at the execution of the webhook.
        /// </summary>
        /// <param name="mention">Mention to add.</param>
        public T AddMention(IMention mention)
        {
            this._mentions.Add(mention);
            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Adds the mentions to the mentions to parse, etc. at the execution of the webhook.
        /// </summary>
        /// <param name="mentions">Mentions to add.</param>
        public T AddMentions(IEnumerable<IMention> mentions)
        {
            this._mentions.AddRange(mentions);
            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Allows for clearing the Webhook Builder so that it can be used again to send a new message.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Does the validation before we send a the Create/Modify request.
        /// </summary>
        internal abstract void Validate();
        
    }

    /// <summary>
    /// Represents the Message to be sent to discord via webhook.
    /// </summary>
    public sealed class WebhookMessageCreateBuilder : WebhookMessageBuilder<WebhookMessageCreateBuilder>
    {
        /// <summary>
        /// Username to use for this webhook request.
        /// </summary>
        public Optional<string> Username { get; set; }

        /// <summary>
        /// Avatar url to use for this webhook request.
        /// </summary>
        public Optional<string> AvatarUrl { get; set; }

        /// <summary>
        /// Files to send on this webhook request.
        /// </summary>
        public IReadOnlyCollection<MessageFile> Files => this._files;

        internal readonly List<MessageFile> _files = new List<MessageFile>();

        /// <summary>
        /// Sets the username for this webhook builder.
        /// </summary>
        /// <param name="username">Username of the webhook</param>
        public WebhookMessageCreateBuilder WithUsername(string username)
        {
            this.Username = username;
            return this;
        }

        /// <summary>
        /// Sets the avatar of this webhook builder from its url.
        /// </summary>
        /// <param name="avatarUrl">Avatar url of the webhook</param>
        public WebhookMessageCreateBuilder WithAvatarUrl(string avatarUrl)
        {
            this.AvatarUrl = avatarUrl;
            return this;
        }

        /// <summary>
        /// Adds a file to send at the execution of the webhook.
        /// </summary>
        /// <param name="filename">Name of the file.</param>
        /// <param name="data">File data.</param>
        /// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        public WebhookMessageCreateBuilder AddFile(string filename, Stream data, bool resetStreamPosition = false)
        {
            if (this.Files.Count() >= 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            if (this._files.Any(x => x.FileName == filename))
                throw new ArgumentException("A File with that filename already exists");

            if (resetStreamPosition)
                this._files.Add(new MessageFile(filename, data, data.Position));
            else
                this._files.Add(new MessageFile(filename, data, null));

            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="stream">The Stream to the file.</param>
        /// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <returns></returns>
        public WebhookMessageCreateBuilder AddFile(FileStream stream, bool resetStreamPosition = false)
        {
            if (this.Files.Count() >= 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            if (this._files.Any(x => x.FileName == stream.Name))
                throw new ArgumentException("A File with that filename already exists");

            if (resetStreamPosition)
                this._files.Add(new MessageFile(stream.Name, stream, stream.Position));
            else
                this._files.Add(new MessageFile(stream.Name, stream, null));

            return this;
        }

        /// <summary>
        /// Adds the given files to send at the execution of the webhook.
        /// </summary>
        /// <param name="files">Dictionary of file name and file data.</param>
        /// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        public WebhookMessageCreateBuilder AddFiles(Dictionary<string, Stream> files, bool resetStreamPosition = false)
        {
            if (this.Files.Count() + files.Count() >= 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            foreach (var file in files)
            {
                if (this._files.Any(x => x.FileName == file.Key))
                    throw new ArgumentException("A File with that filename already exists");

                if (resetStreamPosition)
                    this._files.Add(new MessageFile(file.Key, file.Value, file.Value.Position));
                else
                    this._files.Add(new MessageFile(file.Key, file.Value, null));
            }


            return this;
        }

        /// <summary>
        /// Executes a webhook.
        /// </summary>
        /// <param name="webhook">The webhook that should be executed.</param>
        /// <returns>The message sent</returns>
        public async Task<DiscordMessage> SendAsync(DiscordWebhook webhook)
        {
            return await webhook.ExecuteAsync(this).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            this.Content = "";
            this._embeds.Clear();
            this.IsTTS = false;
            this._mentions.Clear();
            this._files.Clear();
        }

        /// <inheritdoc/>
        internal override void Validate()
        {
            if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && !this.Embeds.Any())
                throw new ArgumentException("You must specify content, an embed, or at least one file.");
        }
    }

    /// <summary>
    /// Represents the changes that will be made to a message via webhook.
    /// </summary>
    public sealed class WebhookMessageModifyBuilder : WebhookMessageBuilder<WebhookMessageModifyBuilder>
    {
        /// <summary>
        /// Sends the modified webhook message.
        /// </summary>
        /// <param name="webhook">The webhook that should be executed.</param>
        /// <param name="message">The message to modify.</param>
        /// <returns>The modified message</returns>
        public async Task<DiscordMessage> ModifyAsync(DiscordWebhook webhook, DiscordMessage message)
        {
            return await this.ModifyAsync(webhook, message.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the modified webhook message.
        /// </summary>
        /// <param name="webhook">The webhook that should be executed.</param>
        /// <param name="messageId">The id of the message to modify.</param>
        /// <returns>The modified message</returns>
        public async Task<DiscordMessage> ModifyAsync(DiscordWebhook webhook, ulong messageId)
        {
            return await webhook.EditMessageAsync(messageId, this).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            this.Content = "";
            this._embeds.Clear();
            this.IsTTS = false;
            this._mentions.Clear();
        }

        /// <summary>
        /// Does the validation before we send a the Create/Modify request.
        /// </summary>
        /// <param name="isModify">Tells the method to perform the Modify Validation or Create Validation.</param>
        internal void Validate(bool isModify = false)
        {
            if (isModify)
            {
                if (this.Files.Any())
                    throw new ArgumentException("You cannot add files when modifying a message.");

                if (this.Username.HasValue)
                    throw new ArgumentException("You cannot change the username of a message.");

                if (this.AvatarUrl.HasValue)
                    throw new ArgumentException("You cannot change the avatar of a message.");
            }
            else
            {
                if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && !this.Embeds.Any())
                    throw new ArgumentException("You must specify content, an embed, or at least one file.");
            }
        }
    }
}
