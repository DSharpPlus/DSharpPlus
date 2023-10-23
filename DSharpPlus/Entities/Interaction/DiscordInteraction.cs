using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

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
        => (this.Discord as DiscordClient).InternalGetCachedChannel(this.ChannelId) ?? (DiscordChannel)(this.Discord as DiscordClient).InternalGetCachedThread(this.ChannelId) ?? (this.Guild == null ? new DiscordDmChannel { Id = this.ChannelId, Type = ChannelType.Private, Discord = this.Discord, Recipients = new DiscordUser[] { this.User } } : null);

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
    /// Gets the locale of the user that invoked this interaction.
    /// </summary>
    [JsonProperty("locale")]
    public string? Locale { get; internal set; }

    /// <summary>
    /// Gets the guild's preferred locale, if invoked in a guild.
    /// </summary>
    [JsonProperty("guild_locale")]
    public string? GuildLocale { get; internal set; }

    /// <summary>
    /// Creates a response to this interaction.
    /// </summary>
    /// <param name="type">The type of the response.</param>
    /// <param name="builder">The data, if any, to send.</param>
    public async Task CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder builder = null) =>
        await this.Discord.ApiClient.CreateInteractionResponseAsync(this.Id, this.Token, type, builder);

    /// <summary>
    ///     Creates a deferred response to this interaction.
    /// </summary>
    /// <param name="ephemeral">Whether the response should be ephemeral.</param>
    public Task DeferAsync(bool ephemeral = false) => this.CreateResponseAsync(
        InteractionResponseType.DeferredChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().AsEphemeral(ephemeral));

    /// <summary>
    /// Gets the original interaction response.
    /// </summary>
    /// <returns>The original message that was sent.</returns>
    public async Task<DiscordMessage> GetOriginalResponseAsync() =>
        await this.Discord.ApiClient.GetOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token);

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
    public async Task DeleteOriginalResponseAsync() =>
        await this.Discord.ApiClient.DeleteOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token);

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
    /// Gets a follow up message.
    /// </summary>
    /// <param name="messageId">The id of the follow up message.</param>
    public async Task<DiscordMessage> GetFollowupMessageAsync(ulong messageId) =>
        await this.Discord.ApiClient.GetFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId);

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
    public async Task DeleteFollowupMessageAsync(ulong messageId) =>
        await this.Discord.ApiClient.DeleteFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId);
}
