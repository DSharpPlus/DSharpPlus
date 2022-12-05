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
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs ready-to-send webhook requests.
    /// </summary>
    public sealed class DiscordWebhookBuilder : BaseDiscordMessageBuilder<DiscordWebhookBuilder>
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
        /// Id of the thread to send the webhook request to.
        /// </summary>
        public ulong? ThreadId { get; set; }

        /// <summary>
        /// Constructs a new empty webhook request builder.
        /// </summary>
        public DiscordWebhookBuilder() { } // I still see no point in initializing collections with empty collections. //

        /// <summary>
        /// Constructs a new webhook request builder based on a previous message builder
        /// </summary>
        /// <param name="builder"></param>
        public DiscordWebhookBuilder(DiscordWebhookBuilder builder) : base(builder)
        {
            this.Username = builder.Username;
            this.AvatarUrl = builder.AvatarUrl;
            this.ThreadId= builder.ThreadId;
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
        /// Sets the id of the thread to execute the webhook on.
        /// </summary>
        /// <param name="threadId">The id of the thread</param>
        public DiscordWebhookBuilder WithThreadId(ulong? threadId)
        {
            this.ThreadId = threadId;
            return this;
        }

        public override void Clear()
        {
            this.Username = default;
            this.AvatarUrl = default;
            this.ThreadId = default;
            base.Clear();
        }

        /// <summary>
        /// Executes a webhook.
        /// </summary>
        /// <param name="webhook">The webhook that should be executed.</param>
        /// <returns>The message sent</returns>
        public async Task<DiscordMessage> SendAsync(DiscordWebhook webhook) => await webhook.ExecuteAsync(this).ConfigureAwait(false);

        /// <summary>
        /// Sends the modified webhook message.
        /// </summary>
        /// <param name="webhook">The webhook that should be executed.</param>
        /// <param name="message">The message to modify.</param>
        /// <returns>The modified message</returns>
        public async Task<DiscordMessage> ModifyAsync(DiscordWebhook webhook, DiscordMessage message) => await this.ModifyAsync(webhook, message.Id).ConfigureAwait(false);
        /// <summary>
        /// Sends the modified webhook message.
        /// </summary>
        /// <param name="webhook">The webhook that should be executed.</param>
        /// <param name="messageId">The id of the message to modify.</param>
        /// <returns>The modified message</returns>
        public async Task<DiscordMessage> ModifyAsync(DiscordWebhook webhook, ulong messageId) => await webhook.EditMessageAsync(messageId, this).ConfigureAwait(false);

        /// <summary>
        /// Does the validation before we send a the Create/Modify request.
        /// </summary>
        /// <param name="isModify">Tells the method to perform the Modify Validation or Create Validation.</param>
        /// <param name="isFollowup">Tells the method to perform the follow up message validation.</param>
        /// <param name="isInteractionResponse">Tells the method to perform the interaction response validation.</param>
        internal void Validate(bool isModify = false, bool isFollowup = false, bool isInteractionResponse = false)
        {
            if (isModify)
            {
                if (this.Username.HasValue)
                    throw new ArgumentException("You cannot change the username of a message.");

                if (this.AvatarUrl.HasValue)
                    throw new ArgumentException("You cannot change the avatar of a message.");
            }
            else if (isFollowup)
            {
                if (this.Username.HasValue)
                    throw new ArgumentException("You cannot change the username of a follow up message.");

                if (this.AvatarUrl.HasValue)
                    throw new ArgumentException("You cannot change the avatar of a follow up message.");
            }
            else if (isInteractionResponse)
            {
                if (this.Username.HasValue)
                    throw new ArgumentException("You cannot change the username of an interaction response.");

                if (this.AvatarUrl.HasValue)
                    throw new ArgumentException("You cannot change the avatar of an interaction response.");
            }
            else
            {
                if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && !this.Embeds.Any())
                    throw new ArgumentException("You must specify content, an embed, or at least one file.");
            }
        }
    }
}
