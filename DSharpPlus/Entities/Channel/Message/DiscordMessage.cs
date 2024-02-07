using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord text message.
/// </summary>
public class DiscordMessage : SnowflakeObject, IEquatable<DiscordMessage>
{
    internal DiscordMessage()
    {

    }

    internal DiscordMessage(DiscordMessage other)
        : this()
    {
        this.Discord = other.Discord;

        this._attachments = new List<DiscordAttachment>(other._attachments);
        this._embeds = new List<DiscordEmbed>(other._embeds);

        if (other._mentionedChannels is not null)
        {
            this._mentionedChannels = new List<DiscordChannel>(other._mentionedChannels);
        }

        if (other._mentionedRoles is not null)
        {
            this._mentionedRoles = new List<DiscordRole>(other._mentionedRoles);
        }

        if (other._mentionedRoleIds is not null)
        {
            this._mentionedRoleIds = new List<ulong>(other._mentionedRoleIds);
        }

        this._mentionedUsers = new List<DiscordUser>(other._mentionedUsers);
        this._reactions = new List<DiscordReaction>(other._reactions);
        this._stickers = new List<DiscordMessageSticker>(other._stickers);

        this.Author = other.Author;
        this.ChannelId = other.ChannelId;
        this.Content = other.Content;
        this.EditedTimestamp = other.EditedTimestamp;
        this.Id = other.Id;
        this.IsTTS = other.IsTTS;
        this.MessageType = other.MessageType;
        this.Pinned = other.Pinned;
        this._timestampRaw = other._timestampRaw;
        this.WebhookId = other.WebhookId;
        this.ApplicationId = other.ApplicationId;
    }

    /// <summary>
    /// Gets the channel in which the message was sent.
    /// </summary>
    [JsonIgnore]
    public DiscordChannel? Channel
    {
        get => (this.Discord as DiscordClient)?.InternalGetCachedChannel(this.ChannelId) ?? (this.Discord as DiscordClient)?.InternalGetCachedThread(this.ChannelId) ?? this._channel;
        internal set => this._channel = value;
    }

    private DiscordChannel? _channel;

    /// <summary>
    /// Gets the ID of the channel in which the message was sent.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the components this message was sent with.
    /// </summary>
    [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyCollection<DiscordActionRowComponent>? Components { get; internal set; }

    /// <summary>
    /// Gets the user or member that sent the message.
    /// </summary>
    [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUser? Author { get; internal set; }

    /// <summary>
    /// Gets the message's content.
    /// </summary>
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    public string? Content { get; internal set; }

    /// <summary>
    /// Gets the message's creation timestamp.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset Timestamp => this._timestampRaw ?? this.CreationTimestamp;

    [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
    internal DateTimeOffset? _timestampRaw { get; set; }

    /// <summary>
    /// Gets the message's edit timestamp. Will be null if the message was not edited.
    /// </summary>
    [JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? EditedTimestamp { get; internal set; }

    /// <summary>
    /// Gets whether this message was edited.
    /// </summary>
    [JsonIgnore]
    public bool IsEdited => this.EditedTimestamp != null;

    /// <summary>
    /// Gets whether the message is a text-to-speech message.
    /// </summary>
    [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsTTS { get; internal set; }

    /// <summary>
    /// Gets whether the message mentions everyone.
    /// </summary>
    [JsonProperty("mention_everyone", NullValueHandling = NullValueHandling.Ignore)]
    public bool MentionEveryone { get; internal set; }

    /// <summary>
    /// Gets users or members mentioned by this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordUser> MentionedUsers
        => this._mentionedUsers;

    [JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordUser> _mentionedUsers = [];

    // TODO this will probably throw an exception in DMs since it tries to wrap around a null List...
    // this is probably low priority but need to find out a clean way to solve it...
    /// <summary>
    /// Gets roles mentioned by this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordRole> MentionedRoles
        => this._mentionedRoles;

    [JsonIgnore]
    internal List<DiscordRole> _mentionedRoles = [];

    [JsonProperty("mention_roles")]
    internal List<ulong> _mentionedRoleIds = [];

    /// <summary>
    /// Gets channels mentioned by this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordChannel> MentionedChannels
        => this._mentionedChannels;

    [JsonIgnore]
    internal List<DiscordChannel> _mentionedChannels = [];

    /// <summary>
    /// Gets files attached to this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordAttachment> Attachments
        => this._attachments;

    [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordAttachment> _attachments = [];

    /// <summary>
    /// Gets embeds attached to this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordEmbed> Embeds
        => this._embeds;

    [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordEmbed> _embeds = [];

    /// <summary>
    /// Gets reactions used on this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordReaction> Reactions
        => this._reactions;

    [JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordReaction> _reactions = [];

    /*
    /// <summary>
    /// Gets the nonce sent with the message, if the message was sent by the client.
    /// </summary>
    [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? Nonce { get; internal set; }
    */

    /// <summary>
    /// Gets whether the message is pinned.
    /// </summary>
    [JsonProperty("pinned", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Pinned { get; internal set; }

    /// <summary>
    /// Gets the id of the webhook that generated this message.
    /// </summary>
    [JsonProperty("webhook_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? WebhookId { get; internal set; }

    /// <summary>
    /// Gets the type of the message.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public MessageType? MessageType { get; internal set; }

    /// <summary>
    /// Gets the message activity in the Rich Presence embed.
    /// </summary>
    [JsonProperty("activity", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMessageActivity? Activity { get; internal set; }

    /// <summary>
    /// Gets the message application in the Rich Presence embed.
    /// </summary>
    [JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMessageApplication? Application { get; internal set; }

    [JsonProperty("message_reference", NullValueHandling = NullValueHandling.Ignore)]
    internal InternalDiscordMessageReference? _internalReference { get; set; }

    /// <summary>
    /// Gets the original message reference from the crossposted message.
    /// </summary>
    [JsonIgnore]
    public DiscordMessageReference? Reference
        => this._internalReference.HasValue ? this?.InternalBuildMessageReference() : null;

    /// <summary>
    /// Gets the bitwise flags for this message.
    /// </summary>
    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public MessageFlags? Flags { get; internal set; }

    /// <summary>
    /// Gets whether the message originated from a webhook.
    /// </summary>
    [JsonIgnore]
    public bool? WebhookMessage
        => this.WebhookId != null;

    /// <summary>
    /// Gets the jump link to this message.
    /// </summary>
    [JsonIgnore]
    public Uri JumpLink
    {
        get
        {
            string gid = this.Channel is DiscordDmChannel ? "@me" : this.Channel?.GuildId?.ToString(CultureInfo.InvariantCulture) ?? "@me";
            string cid = this.ChannelId.ToString(CultureInfo.InvariantCulture);
            string mid = this.Id.ToString(CultureInfo.InvariantCulture);

            return new Uri($"https://discord.com/channels/{gid}/{cid}/{mid}");
        }
    }

    /// <summary>
    /// Gets stickers for this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordMessageSticker>? Stickers
        => this._stickers;

    [JsonProperty("sticker_items", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordMessageSticker> _stickers = [];

    [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong? _guildId { get; set; }

    /// <summary>
    /// Gets the message object for the referenced message
    /// </summary>
    [JsonProperty("referenced_message", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMessage? ReferencedMessage { get; internal set; }

    /// <summary>
    /// Gets whether the message is a response to an interaction.
    /// </summary>
    [JsonProperty("interaction", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMessageInteraction? Interaction { get; internal set; }

    /// <summary>
    /// Gets the id of the interaction application, if a response to an interaction.
    /// </summary>
    [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? ApplicationId { get; internal set; }

    internal DiscordMessageReference InternalBuildMessageReference()
    {
        DiscordClient client = (DiscordClient)this.Discord;
        ulong? guildId = this._internalReference?.GuildId;
        ulong? channelId = this._internalReference?.ChannelId;
        ulong? messageId = this._internalReference?.MessageId;

        DiscordMessageReference reference = new();

        if (guildId.HasValue)
        {
            reference.Guild = client!._guilds.TryGetValue(guildId.Value, out DiscordGuild? g)
                ? g
                : new DiscordGuild
                {
                    Id = guildId.Value,
                    Discord = client
                };
        }

        DiscordChannel channel = client.InternalGetCachedChannel(channelId!.Value);

        if (channel is null)
        {
            reference.Channel = new DiscordChannel
            {
                Id = channelId.Value,
                Discord = client
            };

            if (guildId.HasValue)
            {
                reference.Channel.GuildId = guildId.Value;
            }
        }

        else
        {
            reference.Channel = channel;
        }

        if (client.MessageCache != null && client.MessageCache.TryGet(messageId!.Value, out DiscordMessage? msg))
        {
            reference.Message = msg;
        }
        else
        {
            reference.Message = new DiscordMessage
            {
                ChannelId = this.ChannelId,
                Discord = client
            };

            if (messageId.HasValue)
            {
                reference.Message.Id = messageId.Value;
            }
        }

        return reference;
    }

    private IMention[] GetMentions()
    {
        List<IMention> mentions = [];

        if (this.ReferencedMessage is not null && this._mentionedUsers.Any(r => r.Id == this.ReferencedMessage.Author?.Id))
        {
            mentions.Add(new RepliedUserMention()); // Return null to allow all mentions
        }

        if ((this._mentionedUsers?.Count ?? 0) > 0)
        {
            mentions.AddRange(this._mentionedUsers!.Select(m => (IMention)new UserMention(m)));
        }

        if ((this._mentionedRoleIds?.Count ?? 0) > 0)
        {
            mentions.AddRange(this._mentionedRoleIds!.Select(r => (IMention)new RoleMention(r)));
        }

        return [.. mentions];
    }

    internal void PopulateMentions()
    {
        DiscordGuild? guild = this.Channel?.Guild;
        this._mentionedUsers ??= [];
        this._mentionedRoles ??= [];
        this._mentionedChannels ??= [];

        // Create a Hashset that will replace 'this._mentionedUsers'.
        HashSet<DiscordUser> mentionedUsers = new(new DiscordUserComparer());

        foreach (DiscordUser usr in this._mentionedUsers)
        {
            // Assign the Discord instance and update the user cache.
            usr.Discord = this.Discord;
            this.Discord.UpdateUserCache(usr);

            if (guild is not null && usr is not DiscordMember && guild._members.TryGetValue(usr.Id, out DiscordMember? cachedMember))
            {
                // If the message is from a guild, but a discord member isn't provided, try to get the discord member out of guild members cache.
                mentionedUsers.Add(cachedMember);
            }
            else
            {
                // Add provided user otherwise.
                mentionedUsers.Add(usr);
            }
        }

        // Replace 'this._mentionedUsers'.
        this._mentionedUsers = [.. mentionedUsers];

        if (guild is not null && !string.IsNullOrWhiteSpace(this.Content))
        {
            this._mentionedChannels = this._mentionedChannels.Union(Utilities.GetChannelMentions(this).Select(guild.GetChannel)).ToList();
            this._mentionedRoles = this._mentionedRoles.Union(this._mentionedRoleIds.Select(guild.GetRole)).ToList();

            //uncomment if this breaks
            //mentionedUsers.UnionWith(Utilities.GetUserMentions(this).Select(this.Discord.GetCachedOrEmptyUserInternal));
            //this._mentionedRoles = this._mentionedRoles.Union(Utilities.GetRoleMentions(this).Select(xid => guild.GetRole(xid))).ToList();
        }
    }

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="content">New content.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> ModifyAsync(Optional<string> content)
        => await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, content, default, this.GetMentions(), default, [], null, default);

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="embed">New embed.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> ModifyAsync(Optional<DiscordEmbed> embed = default)
        => await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, default, embed.HasValue ? [embed.Value] : Array.Empty<DiscordEmbed>(), this.GetMentions(), default, [], null, default);

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="content">New content.</param>
    /// <param name="embed">New embed.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> ModifyAsync(Optional<string> content, Optional<DiscordEmbed> embed = default)
        => await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, content, embed.HasValue ? [embed.Value] : Array.Empty<DiscordEmbed>(), this.GetMentions(), default, [], null, default);

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="content">New content.</param>
    /// <param name="embeds">New embeds.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> ModifyAsync(Optional<string> content, Optional<IEnumerable<DiscordEmbed>> embeds = default)
        => await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, content, embeds, this.GetMentions(), default, [], null, default);

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="builder">The builder of the message to edit.</param>
    /// <param name="suppressEmbeds">Whether to suppress embeds on the message.</param>
    /// <param name="attachments">Attached files to keep.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> ModifyAsync(DiscordMessageBuilder builder, bool suppressEmbeds = false, IEnumerable<DiscordAttachment>? attachments = default)
    {
        builder.Validate();
        return await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, builder.Content, new Optional<IEnumerable<DiscordEmbed>>(builder.Embeds), builder._mentions, builder.Components, builder.Files, suppressEmbeds ? MessageFlags.SuppressedEmbeds : null, attachments);
    }

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="action">The builder of the message to edit.</param>
    /// <param name="suppressEmbeds">Whether to suppress embeds on the message.</param>
    /// <param name="attachments">Attached files to keep.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> ModifyAsync(Action<DiscordMessageBuilder> action, bool suppressEmbeds = false, IEnumerable<DiscordAttachment>? attachments = default)
    {
        DiscordMessageBuilder builder = new(this);
        action(builder);
        builder.Validate();
        return await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, builder.Content, new Optional<IEnumerable<DiscordEmbed>>(builder.Embeds), builder._mentions, builder.Components, builder.Files, suppressEmbeds ? MessageFlags.SuppressedEmbeds : null, attachments);
    }

    /// <summary>
    /// Modifies the visibility of embeds in this message.
    /// </summary>
    /// <param name="hideEmbeds">Whether to hide all embeds.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ModifyEmbedSuppressionAsync(bool hideEmbeds)
        => await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, default, default, default, default, [], hideEmbeds ? MessageFlags.SuppressedEmbeds : null, default);

    /// <summary>
    /// Deletes the message.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteAsync(string? reason = null)
        => await this.Discord.ApiClient.DeleteMessageAsync(this.ChannelId, this.Id, reason);

    /// <summary>
    /// Pins the message in its channel.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task PinAsync()
        => await this.Discord.ApiClient.PinMessageAsync(this.ChannelId, this.Id);

    /// <summary>
    /// Unpins the message in its channel.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task UnpinAsync()
        => await this.Discord.ApiClient.UnpinMessageAsync(this.ChannelId, this.Id);

    /// <summary>
    /// Responds to the message. This produces a reply.
    /// </summary>
    /// <param name="content">Message content to respond with.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> RespondAsync(string content)
        => await this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, content, null, replyMessageId: this.Id, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Responds to the message. This produces a reply.
    /// </summary>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> RespondAsync(DiscordEmbed embed)
        => await this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, null, embed != null ? new[] { embed } : null, replyMessageId: this.Id, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Responds to the message. This produces a reply.
    /// </summary>
    /// <param name="content">Message content to respond with.</param>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> RespondAsync(string content, DiscordEmbed embed)
        => await this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, content, embed != null ? new[] { embed } : null, replyMessageId: this.Id, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Responds to the message. This produces a reply.
    /// </summary>
    /// <param name="builder">The Discord message builder.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> RespondAsync(DiscordMessageBuilder builder)
        => await this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, builder.WithReply(this.Id, mention: false, failOnInvalidReply: false));

    /// <summary>
    /// Responds to the message. This produces a reply.
    /// </summary>
    /// <param name="action">The Discord message builder.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> RespondAsync(Action<DiscordMessageBuilder> action)
    {
        DiscordMessageBuilder builder = new();
        action(builder);
        return await this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, builder.WithReply(this.Id, mention: false, failOnInvalidReply: false));
    }

    /// <summary>
    /// Creates a new thread within this channel from this message.
    /// </summary>
    /// <param name="name">The name of the thread.</param>
    /// <param name="archiveAfter">The auto archive duration of the thread. Three and seven day archive options are locked behind level 2 and level 3 server boosts respectively.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The created thread.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordThreadChannel> CreateThreadAsync(string name, AutoArchiveDuration archiveAfter, string? reason = null)
    {
        return this.Channel?.Type is not ChannelType.Text and not ChannelType.News
            ? throw new InvalidOperationException("Threads can only be created within text or news channels.")
            : await this.Discord.ApiClient.CreateThreadFromMessageAsync(this.Channel.Id, this.Id, name, archiveAfter, reason);
    }

    /// <summary>
    /// Creates a reaction to this message.
    /// </summary>
    /// <param name="emoji">The emoji you want to react with, either an emoji or name:id</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AddReactions"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task CreateReactionAsync(DiscordEmoji emoji)
        => await this.Discord.ApiClient.CreateReactionAsync(this.ChannelId, this.Id, emoji.ToReactionString());

    /// <summary>
    /// Deletes your own reaction
    /// </summary>
    /// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteOwnReactionAsync(DiscordEmoji emoji)
        => await this.Discord.ApiClient.DeleteOwnReactionAsync(this.ChannelId, this.Id, emoji.ToReactionString());

    /// <summary>
    /// Deletes another user's reaction.
    /// </summary>
    /// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id.</param>
    /// <param name="user">Member you want to remove the reaction for</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteReactionAsync(DiscordEmoji emoji, DiscordUser user, string? reason = null)
        => await this.Discord.ApiClient.DeleteUserReactionAsync(this.ChannelId, this.Id, user.Id, emoji.ToReactionString(), reason);

    /// <summary>
    /// Gets users that reacted with this emoji.
    /// </summary>
    /// <param name="emoji">Emoji to react with.</param>
    /// <param name="limit">Limit of users to fetch.</param>
    /// <param name="after">Fetch users after this user's id.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(DiscordEmoji emoji, int limit = 25, ulong? after = null)
        => this.GetReactionsInternalAsync(emoji, limit, after);

    /// <summary>
    /// Deletes all reactions for this message.
    /// </summary>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteAllReactionsAsync(string? reason = null)
        => await this.Discord.ApiClient.DeleteAllReactionsAsync(this.ChannelId, this.Id, reason);

    /// <summary>
    /// Deletes all reactions of a specific reaction for this message.
    /// </summary>
    /// <param name="emoji">The emoji to clear, either an emoji or name:id.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteReactionsEmojiAsync(DiscordEmoji emoji)
        => await this.Discord.ApiClient.DeleteReactionsEmojiAsync(this.ChannelId, this.Id, emoji.ToReactionString());

    private async Task<IReadOnlyList<DiscordUser>> GetReactionsInternalAsync(DiscordEmoji emoji, int limit = 25, ulong? after = null)
    {
        if (limit < 0)
        {
            throw new ArgumentException("Cannot get a negative number of reactions' users.");
        }

        if (limit == 0)
        {
            return [];
        }

        List<DiscordUser> users = new(limit);
        int remaining = limit;
        ulong? last = after;

        do
        {
            int fetchSize = remaining > 100 ? 100 : remaining;
            IReadOnlyList<DiscordUser> fetch = await this.Discord.ApiClient.GetReactionsAsync(this.ChannelId, this.Id, emoji.ToReactionString(), last, fetchSize);

            remaining -= fetch.Count;

            if (fetch.Count == 0)
            {
                break;
            }

            users.AddRange(fetch);
            last = fetch[^1]?.Id;
        } while (remaining > 0);

        return new ReadOnlyCollection<DiscordUser>(users);
    }

    /// <summary>
    /// Returns a string representation of this message.
    /// </summary>
    /// <returns>String representation of this message.</returns>
    public override string ToString() => $"Message {this.Id}; Attachment count: {this._attachments.Count}; Embed count: {this._embeds.Count}; Contents: {this.Content}";

    /// <summary>
    /// Checks whether this <see cref="DiscordMessage"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordMessage"/>.</returns>
    public override bool Equals(object? obj) => this.Equals(obj as DiscordMessage);

    /// <summary>
    /// Checks whether this <see cref="DiscordMessage"/> is equal to another <see cref="DiscordMessage"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordMessage"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordMessage"/> is equal to this <see cref="DiscordMessage"/>.</returns>
    public bool Equals(DiscordMessage? e) => e is not null && (ReferenceEquals(this, e) || (this.Id == e.Id && this.ChannelId == e.ChannelId));

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordMessage"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordMessage"/>.</returns>
    public override int GetHashCode()
    {
        int hash = 13;

        hash = (hash * 7) + this.Id.GetHashCode();
        hash = (hash * 7) + this.ChannelId.GetHashCode();

        return hash;
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordMessage"/> objects are equal.
    /// </summary>
    /// <param name="e1">First message to compare.</param>
    /// <param name="e2">Second message to compare.</param>
    /// <returns>Whether the two messages are equal.</returns>
    public static bool operator ==(DiscordMessage? e1, DiscordMessage? e2) 
        => (e1 is not null || e2 is null) && (e1 is null || e2 is not null) && ((e1 is null && e2 is null) || (e1!.Id == e2!.Id && e1.ChannelId == e2.ChannelId));

    /// <summary>
    /// Gets whether the two <see cref="DiscordMessage"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First message to compare.</param>
    /// <param name="e2">Second message to compare.</param>
    /// <returns>Whether the two messages are not equal.</returns>
    public static bool operator !=(DiscordMessage e1, DiscordMessage e2)
        => !(e1 == e2);
}
