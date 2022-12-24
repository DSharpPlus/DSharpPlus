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
    internal DiscordMessage() => _jumpLink = new Lazy<Uri>(() =>
                                      {
                                          string gid = Channel is DiscordDmChannel ? "@me" : Channel.GuildId.Value.ToString(CultureInfo.InvariantCulture);
                                          string cid = ChannelId.ToString(CultureInfo.InvariantCulture);
                                          string mid = Id.ToString(CultureInfo.InvariantCulture);

                                          return new Uri($"https://discord.com/channels/{gid}/{cid}/{mid}");
                                      });

    internal DiscordMessage(DiscordMessage other)
        : this()
    {
        Discord = other.Discord;

        _attachments = new List<DiscordAttachment>(other._attachments);
        _embeds = new List<DiscordEmbed>(other._embeds);

        if (other._mentionedChannels != null)
        {
            _mentionedChannels = new List<DiscordChannel>(other._mentionedChannels);
        }

        if (other._mentionedRoles != null)
        {
            _mentionedRoles = new List<DiscordRole>(other._mentionedRoles);
        }

        if (other._mentionedRoleIds != null)
        {
            _mentionedRoleIds = new List<ulong>(other._mentionedRoleIds);
        }

        _mentionedUsers = new List<DiscordUser>(other._mentionedUsers);
        _reactions = new List<DiscordReaction>(other._reactions);
        _stickers = new List<DiscordMessageSticker>(other._stickers);

        Author = other.Author;
        ChannelId = other.ChannelId;
        Content = other.Content;
        EditedTimestamp = other.EditedTimestamp;
        Id = other.Id;
        IsTTS = other.IsTTS;
        MessageType = other.MessageType;
        Pinned = other.Pinned;
        _timestampRaw = other._timestampRaw;
        WebhookId = other.WebhookId;
        ApplicationId = other.ApplicationId;
    }

    /// <summary>
    /// Gets the channel in which the message was sent.
    /// </summary>
    [JsonIgnore]
    public DiscordChannel Channel
    {
        get => (Discord as DiscordClient)?.InternalGetCachedChannel(ChannelId) ?? (Discord as DiscordClient)?.InternalGetCachedThread(ChannelId) ?? _channel;
        internal set => _channel = value;
    }

    private DiscordChannel _channel;

    /// <summary>
    /// Gets the ID of the channel in which the message was sent.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the components this message was sent with.
    /// </summary>
    [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordActionRowComponent> Components { get; internal set; }

    /// <summary>
    /// Gets the user or member that sent the message.
    /// </summary>
    [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUser Author { get; internal set; }

    /// <summary>
    /// Gets the message's content.
    /// </summary>
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    public string Content { get; internal set; }

    /// <summary>
    /// Gets the message's creation timestamp.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset Timestamp => _timestampRaw ?? CreationTimestamp;

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
    public bool IsEdited => EditedTimestamp != null;

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
        => _mentionedUsers;

    [JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordUser> _mentionedUsers;

    // TODO this will probably throw an exception in DMs since it tries to wrap around a null List...
    // this is probably low priority but need to find out a clean way to solve it...
    /// <summary>
    /// Gets roles mentioned by this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordRole> MentionedRoles
        => _mentionedRoles;

    [JsonIgnore]
    internal List<DiscordRole> _mentionedRoles;

    [JsonProperty("mention_roles")]
    internal List<ulong> _mentionedRoleIds;

    /// <summary>
    /// Gets channels mentioned by this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordChannel> MentionedChannels
        => _mentionedChannels;

    [JsonIgnore]
    internal List<DiscordChannel> _mentionedChannels;

    /// <summary>
    /// Gets files attached to this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordAttachment> Attachments
        => _attachments;

    [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordAttachment> _attachments = new();

    /// <summary>
    /// Gets embeds attached to this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordEmbed> Embeds
        => _embeds;

    [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordEmbed> _embeds = new();

    /// <summary>
    /// Gets reactions used on this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordReaction> Reactions
        => _reactions;

    [JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordReaction> _reactions = new();

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
    public bool Pinned { get; internal set; }

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
    public DiscordMessageActivity Activity { get; internal set; }

    /// <summary>
    /// Gets the message application in the Rich Presence embed.
    /// </summary>
    [JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMessageApplication Application { get; internal set; }

    [JsonProperty("message_reference", NullValueHandling = NullValueHandling.Ignore)]
    internal InternalDiscordMessageReference? _internalReference { get; set; }

    /// <summary>
    /// Gets the original message reference from the crossposted message.
    /// </summary>
    [JsonIgnore]
    public DiscordMessageReference Reference
        => _internalReference.HasValue ? this?.InternalBuildMessageReference() : null;

    /// <summary>
    /// Gets the bitwise flags for this message.
    /// </summary>
    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public MessageFlags? Flags { get; internal set; }

    /// <summary>
    /// Gets whether the message originated from a webhook.
    /// </summary>
    [JsonIgnore]
    public bool WebhookMessage
        => WebhookId != null;

    /// <summary>
    /// Gets the jump link to this message.
    /// </summary>
    [JsonIgnore]
    public Uri JumpLink => _jumpLink.Value;
    private readonly Lazy<Uri> _jumpLink;

    /// <summary>
    /// Gets stickers for this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordMessageSticker> Stickers
        => _stickers;

    [JsonProperty("sticker_items", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordMessageSticker> _stickers = new();

    [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong? _guildId { get; set; }

    /// <summary>
    /// Gets the message object for the referenced message
    /// </summary>
    [JsonProperty("referenced_message", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMessage ReferencedMessage { get; internal set; }

    /// <summary>
    /// Gets whether the message is a response to an interaction.
    /// </summary>
    [JsonProperty("interaction", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMessageInteraction Interaction { get; internal set; }

    /// <summary>
    /// Gets the id of the interaction application, if a response to an interaction.
    /// </summary>
    [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? ApplicationId { get; internal set; }

    internal DiscordMessageReference InternalBuildMessageReference()
    {
        DiscordClient? client = Discord as DiscordClient;
        ulong? guildId = _internalReference.Value.GuildId;
        ulong? channelId = _internalReference.Value.ChannelId;
        ulong? messageId = _internalReference.Value.MessageId;

        DiscordMessageReference reference = new DiscordMessageReference();

        if (guildId.HasValue)
        {
            reference.Guild = client._guilds.TryGetValue(guildId.Value, out DiscordGuild? g)
                ? g
                : new DiscordGuild
                {
                    Id = guildId.Value,
                    Discord = client
                };
        }

        DiscordChannel channel = client.InternalGetCachedChannel(channelId.Value);

        if (channel == null)
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

        if (client.MessageCache != null && client.MessageCache.TryGet(m => m.Id == messageId.Value && m.ChannelId == channelId, out DiscordMessage? msg))
        {
            reference.Message = msg;
        }
        else
        {
            reference.Message = new DiscordMessage
            {
                ChannelId = ChannelId,
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
        List<IMention> mentions = new List<IMention>();

        if (ReferencedMessage != null && _mentionedUsers.Any(r => r.Id == ReferencedMessage.Author.Id))
        {
            mentions.Add(new RepliedUserMention()); // Return null to allow all mentions
        }

        if (_mentionedUsers?.Any() ?? false)
        {
            mentions.AddRange(_mentionedUsers.Select(m => (IMention)new UserMention(m)));
        }

        if (_mentionedRoleIds?.Any() ?? false)
        {
            mentions.AddRange(_mentionedRoleIds.Select(r => (IMention)new RoleMention(r)));
        }

        return mentions.ToArray();
    }

    internal void PopulateMentions()
    {
        DiscordGuild? guild = Channel?.Guild;
        _mentionedUsers ??= new List<DiscordUser>();
        _mentionedRoles ??= new List<DiscordRole>();
        _mentionedChannels ??= new List<DiscordChannel>();

        HashSet<DiscordUser> mentionedUsers = new HashSet<DiscordUser>(new DiscordUserComparer());
        if (guild != null)
        {
            foreach (DiscordUser usr in _mentionedUsers)
            {
                DiscordMember? member = usr as DiscordMember;
                if (member != null)
                {
                    Discord.UpdateUserCache(new DiscordUser()
                    {
                        Id = member.Id,
                        Username = member.Username,
                        Discriminator = member.Discriminator,
                        AvatarHash = member.AvatarHash,
                        _bannerColor = member._bannerColor,
                        BannerHash = member.BannerHash,
                        IsBot = member.IsBot,
                        MfaEnabled = member.MfaEnabled,
                        Verified = member.Verified,
                        Email = member.Email,
                        PremiumType = member.PremiumType,
                        Locale = member.Locale,
                        Flags = member.Flags,
                        OAuthFlags = member.OAuthFlags,
                        Discord = Discord
                    });
                }
                else
                {
                    usr.Discord = Discord;
                    Discord.UpdateUserCache(usr);
                }

                mentionedUsers.Add(member ?? (guild._members.TryGetValue(usr.Id, out DiscordMember? cachedMember) ? cachedMember : usr));
            }
        }
        if (!string.IsNullOrWhiteSpace(Content))
        {
            //uncomment if this breaks
            //mentionedUsers.UnionWith(Utilities.GetUserMentions(this).Select(this.Discord.GetCachedOrEmptyUserInternal));

            if (guild != null)
            {
                //uncomment if this breaks
                //this._mentionedRoles = this._mentionedRoles.Union(Utilities.GetRoleMentions(this).Select(xid => guild.GetRole(xid))).ToList();
                _mentionedRoles = _mentionedRoles.Union(_mentionedRoleIds.Select(xid => guild.GetRole(xid))).ToList();
                _mentionedChannels = _mentionedChannels.Union(Utilities.GetChannelMentions(this).Select(xid => guild.GetChannel(xid))).ToList();
            }
        }
        _mentionedUsers = mentionedUsers.ToList();
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
    public Task<DiscordMessage> ModifyAsync(Optional<string> content)
        => Discord.ApiClient.EditMessageAsync(ChannelId, Id, content, default, GetMentions(), default, Array.Empty<DiscordMessageFile>(), null, default);

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="embed">New embed.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordMessage> ModifyAsync(Optional<DiscordEmbed> embed = default)
        => Discord.ApiClient.EditMessageAsync(ChannelId, Id, default, embed.HasValue ? new[] { embed.Value } : Array.Empty<DiscordEmbed>(), GetMentions(), default, Array.Empty<DiscordMessageFile>(), null, default);

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
    public Task<DiscordMessage> ModifyAsync(Optional<string> content, Optional<DiscordEmbed> embed = default)
        => Discord.ApiClient.EditMessageAsync(ChannelId, Id, content, embed.HasValue ? new[] { embed.Value } : Array.Empty<DiscordEmbed>(), GetMentions(), default, Array.Empty<DiscordMessageFile>(), null, default);

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
    public Task<DiscordMessage> ModifyAsync(Optional<string> content, Optional<IEnumerable<DiscordEmbed>> embeds = default)
        => Discord.ApiClient.EditMessageAsync(ChannelId, Id, content, embeds, GetMentions(), default, Array.Empty<DiscordMessageFile>(), null, default);

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
    public async Task<DiscordMessage> ModifyAsync(DiscordMessageBuilder builder, bool suppressEmbeds = false, IEnumerable<DiscordAttachment> attachments = default)
    {
        builder.Validate(true);
        return await Discord.ApiClient.EditMessageAsync(ChannelId, Id, builder.Content, new Optional<IEnumerable<DiscordEmbed>>(builder.Embeds), builder._mentions, builder.Components, builder.Files, suppressEmbeds ? MessageFlags.SuppressedEmbeds : null, attachments).ConfigureAwait(false);
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
    public async Task<DiscordMessage> ModifyAsync(Action<DiscordMessageBuilder> action, bool suppressEmbeds = false, IEnumerable<DiscordAttachment> attachments = default)
    {
        DiscordMessageBuilder builder = new DiscordMessageBuilder();
        action(builder);
        builder.Validate(true);
        return await Discord.ApiClient.EditMessageAsync(ChannelId, Id, builder.Content, new Optional<IEnumerable<DiscordEmbed>>(builder.Embeds), builder._mentions, builder.Components, builder.Files, suppressEmbeds ? MessageFlags.SuppressedEmbeds : null, attachments).ConfigureAwait(false);
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
    public Task ModifyEmbedSuppressionAsync(bool hideEmbeds)
        => Discord.ApiClient.EditMessageAsync(ChannelId, Id, default, default, default, default, Array.Empty<DiscordMessageFile>(), hideEmbeds ? MessageFlags.SuppressedEmbeds : null, default);

    /// <summary>
    /// Deletes the message.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteAsync(string reason = null)
        => Discord.ApiClient.DeleteMessageAsync(ChannelId, Id, reason);

    /// <summary>
    /// Pins the message in its channel.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task PinAsync()
        => Discord.ApiClient.PinMessageAsync(ChannelId, Id);

    /// <summary>
    /// Unpins the message in its channel.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task UnpinAsync()
        => Discord.ApiClient.UnpinMessageAsync(ChannelId, Id);

    /// <summary>
    /// Responds to the message. This produces a reply.
    /// </summary>
    /// <param name="content">Message content to respond with.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordMessage> RespondAsync(string content)
        => Discord.ApiClient.CreateMessageAsync(ChannelId, content, null, replyMessageId: Id, mentionReply: false, failOnInvalidReply: false);

    /// <summary>
    /// Responds to the message. This produces a reply.
    /// </summary>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordMessage> RespondAsync(DiscordEmbed embed)
        => Discord.ApiClient.CreateMessageAsync(ChannelId, null, embed != null ? new[] { embed } : null, replyMessageId: Id, mentionReply: false, failOnInvalidReply: false);

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
    public Task<DiscordMessage> RespondAsync(string content, DiscordEmbed embed)
        => Discord.ApiClient.CreateMessageAsync(ChannelId, content, embed != null ? new[] { embed } : null, replyMessageId: Id, mentionReply: false, failOnInvalidReply: false);

    /// <summary>
    /// Responds to the message. This produces a reply.
    /// </summary>
    /// <param name="builder">The Discord message builder.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordMessage> RespondAsync(DiscordMessageBuilder builder)
        => Discord.ApiClient.CreateMessageAsync(ChannelId, builder.WithReply(Id, mention: false, failOnInvalidReply: false));

    /// <summary>
    /// Responds to the message. This produces a reply.
    /// </summary>
    /// <param name="action">The Discord message builder.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordMessage> RespondAsync(Action<DiscordMessageBuilder> action)
    {
        DiscordMessageBuilder builder = new DiscordMessageBuilder();
        action(builder);
        return Discord.ApiClient.CreateMessageAsync(ChannelId, builder.WithReply(Id, mention: false, failOnInvalidReply: false));
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
    public Task<DiscordThreadChannel> CreateThreadAsync(string name, AutoArchiveDuration archiveAfter, string reason = null)
    {
        return Channel.Type != ChannelType.Text && Channel.Type != ChannelType.News
            ? throw new InvalidOperationException("Threads can only be created within text or news channels.")
            : Discord.ApiClient.CreateThreadFromMessageAsync(Channel.Id, Id, name, archiveAfter, reason);
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
    public Task CreateReactionAsync(DiscordEmoji emoji)
        => Discord.ApiClient.CreateReactionAsync(ChannelId, Id, emoji.ToReactionString());

    /// <summary>
    /// Deletes your own reaction
    /// </summary>
    /// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteOwnReactionAsync(DiscordEmoji emoji)
        => Discord.ApiClient.DeleteOwnReactionAsync(ChannelId, Id, emoji.ToReactionString());

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
    public Task DeleteReactionAsync(DiscordEmoji emoji, DiscordUser user, string reason = null)
        => Discord.ApiClient.DeleteUserReactionAsync(ChannelId, Id, user.Id, emoji.ToReactionString(), reason);

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
        => GetReactionsInternalAsync(emoji, limit, after);

    /// <summary>
    /// Deletes all reactions for this message.
    /// </summary>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteAllReactionsAsync(string reason = null)
        => Discord.ApiClient.DeleteAllReactionsAsync(ChannelId, Id, reason);

    /// <summary>
    /// Deletes all reactions of a specific reaction for this message.
    /// </summary>
    /// <param name="emoji">The emoji to clear, either an emoji or name:id.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteReactionsEmojiAsync(DiscordEmoji emoji)
        => Discord.ApiClient.DeleteReactionsEmojiAsync(ChannelId, Id, emoji.ToReactionString());

    private async Task<IReadOnlyList<DiscordUser>> GetReactionsInternalAsync(DiscordEmoji emoji, int limit = 25, ulong? after = null)
    {
        if (limit < 0)
        {
            throw new ArgumentException("Cannot get a negative number of reactions' users.");
        }

        if (limit == 0)
        {
            return Array.Empty<DiscordUser>();
        }

        List<DiscordUser> users = new List<DiscordUser>(limit);
        int remaining = limit;
        ulong? last = after;

        int lastCount;
        do
        {
            int fetchSize = remaining > 100 ? 100 : remaining;
            IReadOnlyList<DiscordUser> fetch = await Discord.ApiClient.GetReactionsAsync(Channel.Id, Id, emoji.ToReactionString(), last, fetchSize).ConfigureAwait(false);

            lastCount = fetch.Count;
            remaining -= lastCount;

            users.AddRange(fetch);
            last = fetch.LastOrDefault()?.Id;
        } while (remaining > 0 && lastCount > 0);

        return new ReadOnlyCollection<DiscordUser>(users);
    }

    /// <summary>
    /// Returns a string representation of this message.
    /// </summary>
    /// <returns>String representation of this message.</returns>
    public override string ToString() => $"Message {Id}; Attachment count: {_attachments.Count}; Embed count: {_embeds.Count}; Contents: {Content}";

    /// <summary>
    /// Checks whether this <see cref="DiscordMessage"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordMessage"/>.</returns>
    public override bool Equals(object obj) => Equals(obj as DiscordMessage);

    /// <summary>
    /// Checks whether this <see cref="DiscordMessage"/> is equal to another <see cref="DiscordMessage"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordMessage"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordMessage"/> is equal to this <see cref="DiscordMessage"/>.</returns>
    public bool Equals(DiscordMessage e)
    {
        return e is null ? false : ReferenceEquals(this, e) || (Id == e.Id && ChannelId == e.ChannelId);
    }

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordMessage"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordMessage"/>.</returns>
    public override int GetHashCode()
    {
        int hash = 13;

        hash = (hash * 7) + Id.GetHashCode();
        hash = (hash * 7) + ChannelId.GetHashCode();

        return hash;
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordMessage"/> objects are equal.
    /// </summary>
    /// <param name="e1">First message to compare.</param>
    /// <param name="e2">Second message to compare.</param>
    /// <returns>Whether the two messages are equal.</returns>
    public static bool operator ==(DiscordMessage e1, DiscordMessage e2)
    {
        object? o1 = e1 as object;
        object? o2 = e2 as object;

        return (o1 == null && o2 != null) || (o1 != null && o2 == null)
            ? false
            : (o1 == null && o2 == null) || (e1.Id == e2.Id && e1.ChannelId == e2.ChannelId);
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordMessage"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First message to compare.</param>
    /// <param name="e2">Second message to compare.</param>
    /// <returns>Whether the two messages are not equal.</returns>
    public static bool operator !=(DiscordMessage e1, DiscordMessage e2)
        => !(e1 == e2);
}
