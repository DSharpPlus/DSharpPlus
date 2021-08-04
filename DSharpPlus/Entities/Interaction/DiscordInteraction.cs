// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an interaction that was invoked.
    /// </summary>
    public sealed class DiscordInteraction : SnowflakeObject
    {
        /// <summary>
        /// Gets the type of interaction invoked.
        /// </summary>
        [JsonProperty("type")]
        public InteractionType Type { get; internal set; }

        /// <summary>
        /// Gets the command data for this interaction.
        /// </summary>
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInteractionData Data { get; internal set; }

        /// <summary>
        /// Gets the Id of the guild that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public ulong? GuildId { get; internal set; }

        /// <summary>
        /// Gets the guild that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild
            => (this.Discord as DiscordClient).InternalGetCachedGuild(this.GuildId);

        /// <summary>
        /// Gets the Id of the channel that invoked this interaction.
        /// </summary>
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the channel that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel
            => (this.Discord as DiscordClient).InternalGetCachedChannel(this.ChannelId) ?? (this.Guild == null ? new DiscordDmChannel { Id = this.ChannelId, Type = ChannelType.Private, Discord = this.Discord } : null);

        /// <summary>
        /// Gets the user that invoked this interaction.
        /// <para>This can be cast to a <see cref="DiscordMember"/> if created in a guild.</para>
        /// </summary>
        [JsonIgnore]
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the continuation token for responding to this interaction.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; internal set; }

        /// <summary>
        /// Gets the version number for this interaction type.
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; internal set; }

        /// <summary>
        /// Gets the ID of the application that created this interaction.
        /// </summary>
        [JsonProperty("application_id")]
        public ulong ApplicationId { get; internal set; }


        /// <summary>
        /// The message this interaction was created with, if any.
        /// </summary>
        [JsonProperty("message")]
        internal DiscordMessage Message { get; set; }

        /// <summary>
        /// Creates a response to this interaction.
        /// </summary>
        /// <param name="type">The type of the response.</param>
        /// <param name="builder">The data, if any, to send.</param>
        public Task CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder builder = null) =>
            this.Discord.ApiClient.CreateInteractionResponseAsync(this.Id, this.Token, type, builder);

        /// <summary>
        /// Gets the original interaction response.
        /// </summary>
        /// <returns>The origingal message that was sent. This <b>does not work on ephemeral messages.</b></returns>
        public Task<DiscordMessage> GetOriginalResponseAsync() =>
            this.Discord.ApiClient.GetOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token);

        /// <summary>
        /// Edits the original interaction response.
        /// </summary>
        /// <param name="builder">The webhook builder.</param>
        /// <param name="attachments">Attached files to keep.</param>
        /// <returns>The <see cref="DiscordMessage"/> edited.</returns>
        public async Task<DiscordMessage> EditOriginalResponseAsync(DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments = default)
        {
            builder.Validate(isInteractionResponse: true);

            return await this.Discord.ApiClient.EditOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token, builder, attachments);
        }

        /// <summary>
        /// Deletes the original interaction response.
        /// </summary>>
        public Task DeleteOriginalResponseAsync() =>
            this.Discord.ApiClient.DeleteOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token);

        /// <summary>
        /// Creates a follow up message to this interaction.
        /// </summary>
        /// <param name="builder">The webhook builder.</param>
        /// <returns>The <see cref="DiscordMessage"/> created.</returns>
        public async Task<DiscordMessage> CreateFollowupMessageAsync(DiscordFollowupMessageBuilder builder)
        {
            builder.Validate();

            return await this.Discord.ApiClient.CreateFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, builder);
        }

        /// <summary>
        /// Edits a follow up message.
        /// </summary>
        /// <param name="messageId">The id of the follow up message.</param>
        /// <param name="builder">The webhook builder.</param>
        /// <param name="attachments">Attached files to keep.</param>
        /// <returns>The <see cref="DiscordMessage"/> edited.</returns>
        public async Task<DiscordMessage> EditFollowupMessageAsync(ulong messageId, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments = default)
        {
            builder.Validate(isFollowup: true);

            return await this.Discord.ApiClient.EditFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId, builder, attachments);
        }

        /// <summary>
        /// Deletes a follow up message.
        /// </summary>
        /// <param name="messageId">The id of the follow up message.</param>
        public Task DeleteFollowupMessageAsync(ulong messageId) =>
            this.Discord.ApiClient.DeleteFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId);
    }
}
