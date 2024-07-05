using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an interaction that was invoked.
/// </summary>
public class DiscordInteraction : SnowflakeObject
{
    /// <summary>
    /// Gets the response state of the interaction.
    /// </summary>
    [JsonIgnore]
    public DiscordInteractionResponseState ResponseState { get; private set; }

    /// <summary>
    /// Gets the type of interaction invoked.
    /// </summary>
    [JsonProperty("type")]
    public DiscordInteractionType Type { get; internal set; }

    /// <summary>
    /// Gets the command data for this interaction.
    /// </summary>
    [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordInteractionData Data { get; internal set; }

    /// <summary>
    /// Gets the Id of the guild that invoked this interaction. Returns null if interaction was triggered in a private channel.
    /// </summary>
    [JsonIgnore]
    public ulong? GuildId { get; internal set; }

    /// <summary>
    /// Gets the guild that invoked this interaction.  Returns null if interaction was triggered in a private channel.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild? Guild
        => this.GuildId.HasValue ? (this.Discord as DiscordClient).InternalGetCachedGuild(this.GuildId) : null;

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
        => (this.Discord as DiscordClient).InternalGetCachedChannel(this.ChannelId) ?? (DiscordChannel)(this.Discord as DiscordClient).InternalGetCachedThread(this.ChannelId) ?? (this.Guild == null ? new DiscordDmChannel { Id = this.ChannelId, Type = DiscordChannelType.Private, Discord = this.Discord, Recipients = new DiscordUser[] { this.User } } : null);

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
    public DiscordMessage? Message { get; set; }

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
    /// The permissions allowed to the application for the given context.
    /// </summary>
    /// <remarks>
    /// For guilds, this will be the bot's permissions. For group DMs, this is `ATTACH_FILES`, `EMBED_LINKS`, and `MENTION_EVERYONE`.
    /// In the context of the bot's DM, it also includes `USE_EXTERNAL_EMOJI`.
    /// </remarks>
    [JsonProperty("app_permissions", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordPermissions AppPermissions { get; internal set; }

    /// <summary>
    /// Gets the interactions that authorized the interaction.
    ///     <para>
    ///         This dictionary contains the following:
    ///         <list type="bullet">
    ///             <item>
    ///                 If the interaction is installed to a user, a key of <see cref="DiscordApplicationIntegrationType.UserInstall"/> and a value of the user's ID.
    ///             </item>
    ///             <item>
    ///                 If the interaction is installed to a guild, a key of <see cref="DiscordApplicationIntegrationType.GuildInstall"/> and a value of the guild's ID.
    ///                 <list type="bullet">
    ///                     <item>
    ///                         IF the interaction was sent from a guild context, the above holds true, otherwise the ID is 0.
    ///                     </item>
    ///                 </list>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<DiscordApplicationIntegrationType, ulong> AuthorizingIntegrationOwners => this.authorizingIntegrationOwners;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
    // Justification: Used by JSON.NET
    [JsonProperty("authorizing_integration_owners", NullValueHandling = NullValueHandling.Ignore)]
    private readonly Dictionary<DiscordApplicationIntegrationType, ulong> authorizingIntegrationOwners;
#pragma warning restore CS0649

    /// <summary>
    /// Represents the context in which the interaction was executed in
    /// </summary>
    [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordInteractionContextType? Context { get; internal set; }

    /// <summary>
    /// Creates a response to this interaction.
    /// </summary>
    /// <param name="type">The type of the response.</param>
    /// <param name="builder">The data, if any, to send.</param>
    public virtual async Task CreateResponseAsync(DiscordInteractionResponseType type, DiscordInteractionResponseBuilder builder = null)
    {
        if (this.ResponseState is not DiscordInteractionResponseState.Unacknowledged)
        {
            throw new InvalidOperationException("A response has already been made to this interaction.");
        }

        this.ResponseState = type == DiscordInteractionResponseType.DeferredChannelMessageWithSource
            ? DiscordInteractionResponseState.Deferred
            : DiscordInteractionResponseState.Replied;

        await this.Discord.ApiClient.CreateInteractionResponseAsync(this.Id, this.Token, type, builder);
    }

    /// <summary>
    ///     Creates a deferred response to this interaction.
    /// </summary>
    /// <param name="ephemeral">Whether the response should be ephemeral.</param>
    public Task DeferAsync(bool ephemeral = false) => CreateResponseAsync(
        DiscordInteractionResponseType.DeferredChannelMessageWithSource,
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

        return this.ResponseState is DiscordInteractionResponseState.Unacknowledged
            ? throw new InvalidOperationException("A response has not been made to this interaction.")
            : await this.Discord.ApiClient.EditOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token, builder, attachments);
    }

    /// <summary>
    /// Deletes the original interaction response.
    /// </summary>>
    public async Task DeleteOriginalResponseAsync()
    {
        if (this.ResponseState is DiscordInteractionResponseState.Unacknowledged)
        {
            throw new InvalidOperationException("A response has not been made to this interaction.");
        }

        await this.Discord.ApiClient.DeleteOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token);
    }

    /// <summary>
    /// Creates a follow up message to this interaction.
    /// </summary>
    /// <param name="builder">The webhook builder.</param>
    /// <returns>The <see cref="DiscordMessage"/> created.</returns>
    public async Task<DiscordMessage> CreateFollowupMessageAsync(DiscordFollowupMessageBuilder builder)
    {
        builder.Validate();

        this.ResponseState = DiscordInteractionResponseState.Replied;

        return await this.Discord.ApiClient.CreateFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, builder);
    }

    /// <summary>
    /// Gets a follow up message.
    /// </summary>
    /// <param name="messageId">The id of the follow up message.</param>
    public async Task<DiscordMessage> GetFollowupMessageAsync(ulong messageId) => this.ResponseState is not DiscordInteractionResponseState.Replied
            ? throw new InvalidOperationException("A response has not been made to this interaction.")
            : await this.Discord.ApiClient.GetFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId);

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
    public async Task DeleteFollowupMessageAsync(ulong messageId)
    {
        if (this.ResponseState is not DiscordInteractionResponseState.Replied)
        {
            throw new InvalidOperationException("A response has not been made to this interaction.");
        }

        await this.Discord.ApiClient.DeleteFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId);
    }
}
