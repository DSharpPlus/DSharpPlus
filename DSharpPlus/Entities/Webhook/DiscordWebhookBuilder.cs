using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs ready-to-send webhook requests.
    /// </summary>
    public sealed class DiscordWebhookBuilder
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
        private string _content;
        
        /// <summary>
        /// Embeds to send on this webhook request.
        /// </summary>
        public IReadOnlyList<DiscordEmbed> Embeds { get; }
        private readonly List<DiscordEmbed> _embeds = new List<DiscordEmbed>();

        /// <summary>
        /// Files to send on this webhook request.
        /// </summary>
        public IReadOnlyDictionary<string, Stream> Files { get; }
        private readonly Dictionary<string, Stream> _files = new Dictionary<string, Stream>();

        /// <summary>
        /// Mentions to send on this webhook request.
        /// </summary>
        public IEnumerable<IMention> Mentions { get; }
        private readonly List<IMention> _mentions = new List<IMention>();

        /// <summary>
        /// Constructs a new empty webhook request builder.
        /// </summary>
        public DiscordWebhookBuilder()
        {
            this.Embeds = new ReadOnlyCollection<DiscordEmbed>(this._embeds);
            this.Files = new ReadOnlyDictionary<string, Stream>(this._files);
            this.Mentions = new ReadOnlyCollection<IMention>(this._mentions);
        }

        /// <summary>
        /// Sets the username for this webhook builder.
        /// </summary>
        /// <param name="username">Username of the webhook</param>
        public DiscordWebhookBuilder WithUsername(string username)
        {
            this.Username = username;
            return this;
        }

        /// <summary>
        /// Sets the avatar of this webhook builder from its url.
        /// </summary>
        /// <param name="avatarUrl">Avatar url of the webhook</param>
        public DiscordWebhookBuilder WithAvatarUrl(string avatarUrl)
        {
            this.AvatarUrl = avatarUrl;
            return this;
        }

        /// <summary>
        /// Indicates if the webhook must use text-to-speech.
        /// </summary>
        /// <param name="tts">Text-to-speech</param>
        public DiscordWebhookBuilder WithTTS(bool tts)
        {
            this.IsTTS = tts;
            return this;
        }

        /// <summary>
        /// Sets the message to send at the execution of the webhook.
        /// </summary>
        /// <param name="content">Message to send.</param>
        public DiscordWebhookBuilder WithContent(string content)
        {
            this.Content = content;
            return this;
        }

        /// <summary>
        /// Adds an embed to send at the execution of the webhook.
        /// </summary>
        /// <param name="embed">Embed to add.</param>
        public DiscordWebhookBuilder AddEmbed(DiscordEmbed embed)
        {
            this._embeds.Add(embed);
            return this;
        }

        /// <summary>
        /// Adds the given embeds to send at the execution of the webhook.
        /// </summary>
        /// <param name="embeds">Embeds to add.</param>
        public DiscordWebhookBuilder AddEmbeds(IEnumerable<DiscordEmbed> embeds)
        {
            this._embeds.AddRange(embeds);
            return this;
        }

        /// <summary>
        /// Adds a file to send at the execution of the webhook.
        /// </summary>
        /// <param name="filename">Name of the file.</param>
        /// <param name="data">File data.</param>
        public DiscordWebhookBuilder AddFile(string filename, Stream data)
        {
            this._files[filename] = data;
            return this;
        }

        /// <summary>
        /// Adds the given files to send at the execution of the webhook.
        /// </summary>
        /// <param name="files">Dictionary of file name and file data.</param>
        public DiscordWebhookBuilder AddFiles(Dictionary<string, Stream> files)
        {
            foreach (var file in files)
            {
                this._files[file.Key] = file.Value;
            }
            return this;
        }

        /// <summary>
        /// Adds the mention to the mentions to parse, etc. at the execution of the webhook.
        /// </summary>
        /// <param name="mention">Mention to add.</param>
        public DiscordWebhookBuilder AddMention(IMention mention)
        {
            this._mentions.Add(mention);
            return this;
        }

        /// <summary>
        /// Adds the mentions to the mentions to parse, etc. at the execution of the webhook.
        /// </summary>
        /// <param name="mentions">Mentions to add.</param>
        public DiscordWebhookBuilder AddMentions(IEnumerable<IMention> mentions)
        {
            this._mentions.AddRange(mentions);
            return this;
        }
    }
}
