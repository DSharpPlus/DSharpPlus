using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.Net;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

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
    public DiscordPartialChannel SourceChannel { get; internal set; }

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
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageWebhooks"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordWebhook> ModifyAsync(string name = null, Optional<Stream> avatar = default, ulong? channelId = null, string reason = null)
    {
        Optional<string> avatarb64 = Optional.FromNoValue<string>();
        if (avatar.HasValue && avatar.Value != null)
        {
            using ImageTool imgtool = new(avatar.Value);
            avatarb64 = imgtool.GetBase64();
        }
        else if (avatar.HasValue)
        {
            avatarb64 = null;
        }

        ulong newChannelId = channelId ?? this.ChannelId;

        return await this.Discord.ApiClient.ModifyWebhookAsync(this.Id, newChannelId, name, avatarb64, reason);
    }

    /// <summary>
    /// Permanently deletes this webhook.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageWebhooks"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteAsync()
        => await this.Discord.ApiClient.DeleteWebhookAsync(this.Id, this.Token);

    /// <summary>
    /// Executes this webhook with the given <see cref="DiscordWebhookBuilder"/>.
    /// </summary>
    /// <param name="builder">Webhook builder filled with data to send.</param>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> ExecuteAsync(DiscordWebhookBuilder builder)
        => await (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookAsync(this.Id, this.Token, builder);

    /// <summary>
    /// Executes this webhook in Slack compatibility mode.
    /// </summary>
    /// <param name="json">JSON containing Slack-compatible payload for this webhook.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ExecuteSlackAsync(string json)
        => await (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookSlackAsync(this.Id, this.Token, json);

    /// <summary>
    /// Executes this webhook in GitHub compatibility mode.
    /// </summary>
    /// <param name="json">JSON containing GitHub-compatible payload for this webhook.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ExecuteGithubAsync(string json)
        => await (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookGithubAsync(this.Id, this.Token, json);

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
    public async Task DeleteMessageAsync(ulong messageId)
        => await (this.Discord?.ApiClient ?? this.ApiClient).DeleteWebhookMessageAsync(this.Id, this.Token, messageId);

    /// <summary>
    /// Checks whether this <see cref="DiscordWebhook"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordWebhook"/>.</returns>
    public override bool Equals(object obj) => Equals(obj as DiscordWebhook);

    /// <summary>
    /// Checks whether this <see cref="DiscordWebhook"/> is equal to another <see cref="DiscordWebhook"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordWebhook"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordWebhook"/> is equal to this <see cref="DiscordWebhook"/>.</returns>
    public bool Equals(DiscordWebhook e) => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

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
        object? o1 = e1;
        object? o2 = e2;

        return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
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
