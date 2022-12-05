// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs a Message to be sent.
    /// </summary>
    public sealed class DiscordMessageBuilder : BaseDiscordMessageBuilder<DiscordMessageBuilder>
    {
        /// <summary>
        /// Gets or Sets the Message to be sent.
        /// </summary>
        public override string Content
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
        /// Gets or sets the embed for the builder. This will always set the builder to have one embed.
        /// </summary>
        public DiscordEmbed Embed
        {
            get => this._embeds.Count > 0 ? this._embeds[0] : null;
            set
            {
                this._embeds.Clear();
                this._embeds.Add(value);
            }
        }

        /// <summary>
        /// Gets or Sets a sticker to be attached.
        /// </summary>
        public DiscordMessageSticker Sticker { get; set; }

        /// <summary>
        /// Gets the Embeds to be sent.
        /// </summary>
        public override IReadOnlyList<DiscordEmbed> Embeds => this._embeds;
        internal readonly List<DiscordEmbed> _embeds = new();

        /// <summary>
        /// Gets or Sets if the message should be TTS.
        /// </summary>
        public override bool IsTTS { get; set; } = false;

        /// <summary>
        /// Gets the Allowed Mentions for the message to be sent.
        /// </summary>
        public override IReadOnlyList<IMention> Mentions => this._mentions;
        internal List<IMention> _mentions = new();

        /// <summary>
        /// Gets the Files to be sent in the Message.
        /// </summary>
        public override IReadOnlyList<DiscordMessageFile> Files => this._files;
        internal readonly List<DiscordMessageFile> _files = new();

        /// <summary>
        /// Gets the components that will be attached to the message.
        /// </summary>
        public override IReadOnlyList<DiscordActionRowComponent> Components => this._components;
        internal readonly List<DiscordActionRowComponent> _components = new(5);

        /// <summary>
        /// Gets the Reply Message ID.
        /// </summary>
        public ulong? ReplyId { get; private set; } = null;

        /// <summary>
        /// Gets if the Reply should mention the user.
        /// </summary>
        public bool MentionOnReply { get; private set; } = false;

        /// <summary>
        /// Gets if the Reply will error if the Reply Message Id does not reference a valid message.
        /// <para>If set to false, invalid replies are send as a regular message.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool FailOnInvalidReply { get; set; }

        /// <summary>
        /// Sets the Content of the Message.
        /// </summary>
        /// <param name="content">The content to be set.</param>
        /// <returns>The current builder to be chained.</returns>
        public override DiscordMessageBuilder WithContent(string content)
        {
            this.Content = content;
            return this;
        }

        /// <summary>
        /// Adds a sticker to the message. Sticker must be from current guild.
        /// </summary>
        /// <param name="sticker">The sticker to add.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithSticker(DiscordMessageSticker sticker)
        {
            this.Sticker = sticker;
            return this;
        }

        /// <summary>
        /// Adds a row of components to a message, up to 5 components per row, and up to 5 rows per message.
        /// </summary>
        /// <param name="components">The components to add to the message.</param>
        /// <returns>The current builder to be chained.</returns>
        /// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
        public override DiscordMessageBuilder AddComponents(params DiscordComponent[] components)
            => this.AddComponents((IEnumerable<DiscordComponent>)components);


        /// <summary>
        /// Appends several rows of components to the message
        /// </summary>
        /// <param name="components">The rows of components to add, holding up to five each.</param>
        /// <returns></returns>
        public override DiscordMessageBuilder AddComponents(IEnumerable<DiscordActionRowComponent> components)
        {
            var ara = components.ToArray();

            if (ara.Length + this._components.Count > 5)
                throw new ArgumentException("ActionRow count exceeds maximum of five.");

            foreach (var ar in ara)
                this._components.Add(ar);

            return this;
        }

        /// <summary>
        /// Adds a row of components to a message, up to 5 components per row, and up to 5 rows per message.
        /// </summary>
        /// <param name="components">The components to add to the message.</param>
        /// <returns>The current builder to be chained.</returns>
        /// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
        public override DiscordMessageBuilder AddComponents(IEnumerable<DiscordComponent> components)
        {
            var cmpArr = components.ToArray();
            var count = cmpArr.Length;

            if (!cmpArr.Any())
                throw new ArgumentOutOfRangeException(nameof(components), "You must provide at least one component");

            if (count > 5)
                throw new ArgumentException("Cannot add more than 5 components per action row!");

            var comp = new DiscordActionRowComponent(cmpArr);
            this._components.Add(comp);

            return this;
        }

        /// <summary>
        /// Sets if the message should be TTS.
        /// </summary>
        /// <param name="isTTS">If TTS should be set.</param>
        /// <returns>The current builder to be chained.</returns>
        public override DiscordMessageBuilder WithTTS(bool isTTS)
        {
            this.IsTTS = isTTS;
            return this;
        }

        /// <summary>
        /// Sets the embed for the current builder.
        /// </summary>
        /// <param name="embed">The embed that should be set.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithEmbed(DiscordEmbed embed)
        {
            if (embed == null)
                return this;

            this.Embed = embed;
            return this;
        }

        /// <summary>
        /// Appends an embed to the current builder.
        /// </summary>
        /// <param name="embed">The embed that should be appended.</param>
        /// <returns>The current builder to be chained.</returns>
        public override DiscordMessageBuilder AddEmbed(DiscordEmbed embed)
        {
            if (embed == null)
                return this; //Providing null embeds will produce a 400 response from Discord.//
            this._embeds.Add(embed);
            return this;
        }

        /// <summary>
        /// Appends several embeds to the current builder.
        /// </summary>
        /// <param name="embeds">The embeds that should be appended.</param>
        /// <returns>The current builder to be chained.</returns>
        public override DiscordMessageBuilder AddEmbeds(IEnumerable<DiscordEmbed> embeds)
        {
            this._embeds.AddRange(embeds);
            return this;
        }



        /// <summary>
        /// Sets if the message has allowed mentions.
        /// </summary>
        /// <param name="allowedMention">The allowed Mention that should be sent.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithAllowedMention(IMention allowedMention)
        {
            if (this._mentions != null)
                this._mentions.Add(allowedMention);
            else
                this._mentions = new List<IMention> { allowedMention };

            return this;
        }

        /// <summary>
        /// Sets if the message has allowed mentions.
        /// </summary>
        /// <param name="allowedMentions">The allowed Mentions that should be sent.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithAllowedMentions(IEnumerable<IMention> allowedMentions)
        {
            if (this._mentions != null)
                this._mentions.AddRange(allowedMentions);
            else
                this._mentions = allowedMentions.ToList();

            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="fileName">The fileName that the file should be sent as.</param>
        /// <param name="stream">The Stream to the file.</param>
        /// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <returns>The current builder to be chained.</returns>
        public override DiscordMessageBuilder AddFile(string fileName, Stream stream, bool resetStreamPosition = false)
        {
            if (this.Files.Count >= 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            if (this._files.Any(x => x.FileName == fileName))
                throw new ArgumentException("A File with that filename already exists");

            if (resetStreamPosition)
                this._files.Add(new DiscordMessageFile(fileName, stream, stream.Position));
            else
                this._files.Add(new DiscordMessageFile(fileName, stream, null));

            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="stream">The Stream to the file.</param>
        /// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <returns>The current builder to be chained.</returns>
        public override DiscordMessageBuilder AddFile(FileStream stream, bool resetStreamPosition = false)
        {
            if (this.Files.Count >= 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            if (this._files.Any(x => x.FileName == stream.Name))
                throw new ArgumentException("A File with that filename already exists");

            if (resetStreamPosition)
                this._files.Add(new DiscordMessageFile(stream.Name, stream, stream.Position));
            else
                this._files.Add(new DiscordMessageFile(stream.Name, stream, null));

            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="files">The Files that should be sent.</param>
        /// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <returns>The current builder to be chained.</returns>
        public override DiscordMessageBuilder AddFiles(IDictionary<string, Stream> files, bool resetStreamPosition = false)
        {
            if (this.Files.Count + files.Count > 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            foreach (var file in files)
            {
                if (this._files.Any(x => x.FileName == file.Key))
                    throw new ArgumentException("A File with that filename already exists");

                if (resetStreamPosition)
                    this._files.Add(new DiscordMessageFile(file.Key, file.Value, file.Value.Position));
                else
                    this._files.Add(new DiscordMessageFile(file.Key, file.Value, null));
            }

            return this;
        }

        /// <summary>
        /// Sets if the message is a reply
        /// </summary>
        /// <param name="messageId">The ID of the message to reply to.</param>
        /// <param name="mention">If we should mention the user in the reply.</param>
        /// <param name="failOnInvalidReply">Whether sending a reply that references an invalid message should be </param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithReply(ulong? messageId, bool mention = false, bool failOnInvalidReply = false)
        {
            this.ReplyId = messageId;
            this.MentionOnReply = mention;
            this.FailOnInvalidReply = failOnInvalidReply;

            if (mention)
            {
                this._mentions ??= new List<IMention>();
                this._mentions.Add(new RepliedUserMention());
            }

            return this;
        }

        /// <summary>
        /// Adds the mention to the mentions to parse, etc. with the interaction response.
        /// </summary>
        /// <param name="mention">Mention to add.</param>
        public override DiscordMessageBuilder AddMention(IMention mention)
        {
            this._mentions.Add(mention);
            return this;
        }

        /// <summary>
        /// Adds the mentions to the mentions to parse, etc. with the interaction response.
        /// </summary>
        /// <param name="mentions">Mentions to add.</param>
        public override DiscordMessageBuilder AddMentions(IEnumerable<IMention> mentions)
        {
            this._mentions.AddRange(mentions);
            return this;
        }

        /// <summary>
        /// Sends the Message to a specific channel
        /// </summary>
        /// <param name="channel">The channel the message should be sent to.</param>
        /// <returns>The current builder to be chained.</returns>
        public Task<DiscordMessage> SendAsync(DiscordChannel channel) => channel.SendMessageAsync(this);

        /// <summary>
        /// Sends the modified message.
        /// <para>Note: Message replies cannot be modified. To clear the reply, simply pass <see langword="null"/> to <see cref="WithReply"/>.</para>
        /// </summary>
        /// <param name="msg">The original Message to modify.</param>
        /// <returns>The current builder to be chained.</returns>
        public Task<DiscordMessage> ModifyAsync(DiscordMessage msg) => msg.ModifyAsync(this);

        /// <summary>
        /// Clears all message components on this builder.
        /// </summary>
        public override void ClearComponents()
            => this._components.Clear();

        /// <summary>
        /// Allows for clearing the Message Builder so that it can be used again to send a new message.
        /// </summary>
        public override void Clear()
        {
            this.Content = "";
            this._embeds.Clear();
            this.IsTTS = false;
            this._mentions.Clear();
            this._files.Clear();
            this.ReplyId = null;
            this.MentionOnReply = false;
            this._components.Clear();
        }

        /// <summary>
        /// Does the validation before we send a the Create/Modify request.
        /// </summary>
        /// <param name="isModify">Tells the method to perform the Modify Validation or Create Validation.</param>
        internal void Validate(bool isModify = false)
        {
            if (this._embeds.Count > 10)
                throw new ArgumentException("A message can only have up to 10 embeds.");

            if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && (!this.Embeds?.Any() ?? true) && this.Sticker is null)
                throw new ArgumentException("You must specify content, an embed, a sticker, or at least one file.");

            if (this.Components.Count > 5)
                throw new InvalidOperationException("You can only have 5 action rows per message.");

            if (this.Components.Any(c => c.Components.Count > 5))
                throw new InvalidOperationException("Action rows can only have 5 components");
        }
    }
}
