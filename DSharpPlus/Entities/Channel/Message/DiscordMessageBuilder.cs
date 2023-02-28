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
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs a Message to be sent.
    /// </summary>
    public sealed class DiscordMessageBuilder : BaseDiscordMessageBuilder<DiscordMessageBuilder>
    {
        /// <summary>
        /// Gets or sets the embed for the builder. This will always set the builder to have one embed.
        /// </summary>
        public DiscordEmbed Embed
        {
            get => this._embeds.Count > 0 ? this._embeds[0] : null;
            set
            {
                this._embeds.Clear();
                if (value != null)
                    this._embeds.Add(value);
            }
        }

        /// <summary>
        /// Gets or sets the sticker for the builder. This will always set the builder to have one sticker.
        /// </summary>
        public DiscordMessageSticker Sticker
        {
            get => this._stickers.Count > 0 ? this._stickers[0] : null;
            set
            {
                this._stickers.Clear();
                if (value != null)
                    this._stickers.Add(value);
            }
        }

        /// <summary>
        /// The stickers to attach to the message.
        /// </summary>
        public IReadOnlyList<DiscordMessageSticker> Stickers => this._stickers;
        internal List<DiscordMessageSticker> _stickers = new();

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
        /// Constructs a new discord message builder
        /// </summary>
        public DiscordMessageBuilder() { }

        /// <summary>
        /// Constructs a new discord message builder based on a previous builder.
        /// </summary>
        /// <param name="builder">The builder to copy.</param>
        public DiscordMessageBuilder(DiscordMessageBuilder builder) : base(builder)
        {
            this.Sticker = builder.Sticker;
            this.ReplyId = builder.ReplyId;
            this.MentionOnReply = builder.MentionOnReply;
            this.FailOnInvalidReply = builder.FailOnInvalidReply;
        }

        /// <summary>
        /// Copies the common properties from the passed builder.
        /// </summary>
        /// <param name="builder">The builder to copy.</param>
        public DiscordMessageBuilder(IDiscordMessageBuilder builder) : base(builder) { }

        /// <summary>
        /// Constructs a new discord message builder based on the passed message.
        /// </summary>
        /// <param name="baseMessage">The message to copy.</param>
        public DiscordMessageBuilder(DiscordMessage baseMessage)
        {
            this.IsTTS = baseMessage.IsTTS;
            this.ReplyId = baseMessage.ReferencedMessage?.Id;
            this._components = baseMessage.Components.ToList(); // Calling ToList copies the list instead of referencing it
            this._content = baseMessage.Content;
            this._embeds = baseMessage.Embeds.ToList();
            this._stickers = baseMessage.Stickers.ToList();
            this._mentions = new();

            if (baseMessage._mentionedRoles != null)
            {
                foreach (var user in baseMessage._mentionedUsers)
                {
                    this._mentions.Add(new UserMention(user.Id));
                }
            }

            // Unsure about mentionedRoleIds
            if (baseMessage._mentionedRoles != null)
            {
                foreach (var role in baseMessage._mentionedRoles)
                {
                    this._mentions.Add(new RoleMention(role.Id));
                }
            }
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
        /// Adds a sticker to the message. Sticker must be from current guild.
        /// </summary>
        /// <param name="stickers">The sticker to add.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithStickers(IEnumerable<DiscordMessageSticker> stickers)
        {
            this._stickers = stickers.ToList();
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
        /// Sets if the message has allowed mentions.
        /// </summary>
        /// <param name="allowedMention">The allowed Mention that should be sent.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithAllowedMention(IMention allowedMention)
            => this.AddMention(allowedMention);

        /// <summary>
        /// Sets if the message has allowed mentions.
        /// </summary>
        /// <param name="allowedMentions">The allowed Mentions that should be sent.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithAllowedMentions(IEnumerable<IMention> allowedMentions)
            => this.AddMentions(allowedMentions);

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

        public override void ClearComponents()
        {
            this.ReplyId = null;
            this.MentionOnReply = false;
            this.Sticker = default;
            this.FailOnInvalidReply = default;

            base.ClearComponents();
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

            if (this.Stickers?.Count > 3)
                throw new InvalidOperationException("You can only have 3 stickers per message.");
        }
    }
}
