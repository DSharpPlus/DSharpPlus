// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using System.Threading.Tasks;
using DSharpPlus.Net;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents information about a Discord webhook.
    /// </summary>
    public class DiscordWebhook : SnowflakeObject, IEquatable<DiscordWebhook>
    {
        internal DiscordApiClient ApiClient { get; set; }

        /// <summary>
        /// Gets the ID of the guild this webhook belongs to.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets the ID of the channel this webhook belongs to.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the user this webhook was created by.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the default name of this webhook.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets hash of the default avatar for this webhook.
        /// </summary>
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        internal string AvatarHash { get; set; }

        /// <summary>
        /// Gets the default avatar url for this webhook.
        /// </summary>
        public string AvatarUrl
            => !string.IsNullOrWhiteSpace(this.AvatarHash) ? $"https://cdn.discordapp.com/avatars/{this.Id}/{this.AvatarHash}.png?size=1024" : null;

        /// <summary>
        /// Gets the secure token of this webhook.
        /// </summary>
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; internal set; }

        /// <summary>
        /// A partial guild object for the guild of the channel this channel follower webhook is following.
        /// </summary>
        [JsonProperty("source_guild", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuild SourceGuild { get; internal set; }

        /// <summary>
        /// A partial channel object for the channel this channel follower webhook is following.
        /// </summary>
        [JsonProperty("source_channel", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordChannel SourceChannel { get; internal set; }

        /// <summary>
        /// Gets the webhook's url. Only returned when using the webhook.incoming OAuth2 scope.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; internal set; }

        internal DiscordWebhook() { }

        /// <summary>
        /// Modifies this webhook.
        /// </summary>
        /// <param name="name">New default name for this webhook.</param>
        /// <param name="avatar">New avatar for this webhook.</param>
        /// <param name="channelId">The new channel id to move the webhook to.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns>The modified webhook.</returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordWebhook> ModifyAsync(string name = null, Optional<Stream> avatar = default, ulong? channelId = null, string reason = null)
        {
            var avatarb64 = Optional.FromNoValue<string>();
            if (avatar.HasValue && avatar.Value != null)
                using (var imgtool = new ImageTool(avatar.Value))
                    avatarb64 = imgtool.GetBase64();
            else if (avatar.HasValue)
                avatarb64 = null;

            var newChannelId = channelId ?? this.ChannelId;

            return this.Discord.ApiClient.ModifyWebhookAsync(this.Id, newChannelId, name, avatarb64, reason);
        }

        /// <summary>
        /// Permanently deletes this webhook.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task DeleteAsync()
            => this.Discord.ApiClient.DeleteWebhookAsync(this.Id, this.Token);

        /// <summary>
        /// Executes this webhook with the given <see cref="DiscordWebhookBuilder"/>.
        /// </summary>
        /// <param name="builder">Webhook builder filled with data to send.</param>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordMessage> ExecuteAsync(DiscordWebhookBuilder builder)
            => (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookAsync(this.Id, this.Token, builder);

        /// <summary>
        /// Executes this webhook in Slack compatibility mode.
        /// </summary>
        /// <param name="json">JSON containing Slack-compatible payload for this webhook.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task ExecuteSlackAsync(string json)
            => (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookSlackAsync(this.Id, this.Token, json);

        /// <summary>
        /// Executes this webhook in GitHub compatibility mode.
        /// </summary>
        /// <param name="json">JSON containing GitHub-compatible payload for this webhook.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task ExecuteGithubAsync(string json)
            => (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookGithubAsync(this.Id, this.Token, json);

        /// <summary>
        /// Gets a previously-sent webhook message.
        /// </summary>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook or message does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordMessage> GetMessageAsync(ulong messageId)
            => await (this.Discord?.ApiClient ?? this.ApiClient).GetWebhookMessageAsync(this.Id, this.Token, messageId);

        /// <summary>
        /// Edits a previously-sent webhook message.
        /// </summary>
        /// <param name="messageId">The id of the message to edit.</param>
        /// <param name="builder">The builder of the message to edit.</param>
        /// <param name="attachments">Attached files to keep.</param>
        /// <returns>The modified <see cref="DiscordMessage"/></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordMessage> EditMessageAsync(ulong messageId, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments = default)
        {
            builder.Validate(true);

            return await (this.Discord?.ApiClient ?? this.ApiClient).EditWebhookMessageAsync(this.Id, this.Token, messageId, builder, attachments);
        }

        /// <summary>
        /// Deletes a message that was created by the webhook.
        /// </summary>
        /// <param name="messageId">The id of the message to delete</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task DeleteMessageAsync(ulong messageId)
            => (this.Discord?.ApiClient ?? this.ApiClient).DeleteWebhookMessageAsync(this.Id, this.Token, messageId);

        /// <summary>
        /// Checks whether this <see cref="DiscordWebhook"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordWebhook"/>.</returns>
        public override bool Equals(object obj) => this.Equals(obj as DiscordWebhook);

        /// <summary>
        /// Checks whether this <see cref="DiscordWebhook"/> is equal to another <see cref="DiscordWebhook"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordWebhook"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordWebhook"/> is equal to this <see cref="DiscordWebhook"/>.</returns>
        public bool Equals(DiscordWebhook e)
        {
            if (e is null)
                return false;

            return ReferenceEquals(this, e) ? true : this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordWebhook"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordWebhook"/>.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="DiscordWebhook"/> objects are equal.
        /// </summary>
        /// <param name="e1">First webhook to compare.</param>
        /// <param name="e2">Second webhook to compare.</param>
        /// <returns>Whether the two webhooks are equal.</returns>
        public static bool operator ==(DiscordWebhook e1, DiscordWebhook e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            return o1 == null && o2 == null ? true : e1.Id == e2.Id;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordWebhook"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First webhook to compare.</param>
        /// <param name="e2">Second webhook to compare.</param>
        /// <returns>Whether the two webhooks are not equal.</returns>
        public static bool operator !=(DiscordWebhook e1, DiscordWebhook e2)
            => !(e1 == e2);
    }
}
