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
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a discord channel.
/// </summary>
public class DiscordChannel : SnowflakeObject, IEquatable<DiscordChannel>
{
    /// <summary>
    /// Gets ID of the guild to which this channel belongs.
    /// </summary>
    [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? GuildId { get; internal set; }

    /// <summary>
    /// Gets ID of the category that contains this channel. For threads, gets the ID of the channel this thread was created in.
    /// </summary>
    [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Include)]
    public ulong? ParentId { get; internal set; }

    /// <summary>
    /// Gets the category that contains this channel. For threads, gets the channel this thread was created in.
    /// </summary>
    [JsonIgnore]
    public DiscordChannel Parent
        => ParentId.HasValue ? Guild.GetChannel(ParentId.Value) : null;

    /// <summary>
    /// Gets the name of this channel.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the type of this channel.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public ChannelType Type { get; internal set; }

    /// <summary>
    /// Gets the position of this channel.
    /// </summary>
    [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
    public int Position { get; internal set; }

    /// <summary>
    /// Gets whether this channel is a DM channel.
    /// </summary>
    [JsonIgnore]
    public bool IsPrivate
        => Type == ChannelType.Private || Type == ChannelType.Group;

    /// <summary>
    /// Gets whether this channel is a channel category.
    /// </summary>
    [JsonIgnore]
    public bool IsCategory
        => Type == ChannelType.Category;

    /// <summary>
    /// Gets whether this channel is a thread.
    /// </summary>
    [JsonIgnore]
    public bool IsThread
        => Type == ChannelType.PrivateThread || Type == ChannelType.PublicThread || Type == ChannelType.NewsThread;

    /// <summary>
    /// Gets the guild to which this channel belongs.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild Guild
        => GuildId.HasValue && Discord.Guilds.TryGetValue(GuildId.Value, out DiscordGuild? guild) ? guild : null;

    /// <summary>
    /// Gets a collection of permission overwrites for this channel.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordOverwrite> PermissionOverwrites
        => _permissionOverwritesLazy.Value;

    [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordOverwrite> _permissionOverwrites = new();
    [JsonIgnore]
    private readonly Lazy<IReadOnlyList<DiscordOverwrite>> _permissionOverwritesLazy;

    /// <summary>
    /// Gets the channel's topic. This is applicable to text channels only.
    /// </summary>
    [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
    public string Topic { get; internal set; }

    /// <summary>
    /// Gets the ID of the last message sent in this channel. This is applicable to text channels only.
    /// </summary>
    [JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? LastMessageId { get; internal set; }

    /// <summary>
    /// Gets this channel's bitrate. This is applicable to voice channels only.
    /// </summary>
    [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
    public int? Bitrate { get; internal set; }

    /// <summary>
    /// Gets this channel's user limit. This is applicable to voice channels only.
    /// </summary>
    [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
    public int? UserLimit { get; internal set; }

    /// <summary>
    /// <para>Gets the slow mode delay configured for this channel.</para>
    /// <para>All bots, as well as users with <see cref="Permissions.ManageChannels"/> or <see cref="Permissions.ManageMessages"/> permissions in the channel are exempt from slow mode.</para>
    /// </summary>
    [JsonProperty("rate_limit_per_user")]
    public int? PerUserRateLimit { get; internal set; }

    /// <summary>
    /// Gets this channel's video quality mode. This is applicable to voice channels only.
    /// </summary>
    [JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
    public VideoQualityMode? QualityMode { get; internal set; }

    /// <summary>
    /// Gets when the last pinned message was pinned.
    /// </summary>
    [JsonProperty("last_pin_timestamp", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? LastPinTimestamp { get; internal set; }

    /// <summary>
    /// Gets this channel's mention string.
    /// </summary>
    [JsonIgnore]
    public string Mention
        => Formatter.Mention(this);

    /// <summary>
    /// Gets this channel's children. This applies only to channel categories.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordChannel> Children => !IsCategory
                ? throw new ArgumentException("Only channel categories contain children.")
                : Guild._channels.Values.Where(e => e.ParentId == Id).ToList();

    /// <summary>
    /// Gets this channel's threads. This applies only to text and news channels.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordThreadChannel> Threads => Type is not (ChannelType.Text or ChannelType.News)
                ? throw new ArgumentException("Only text channels can have threads.")
                : Guild._threads.Values.Where(e => e.ParentId == Id).ToImmutableArray();

    /// <summary>
    /// Gets the list of members currently in the channel (if voice channel), or members who can see the channel (otherwise).
    /// </summary>
    [JsonIgnore]
    public virtual IReadOnlyList<DiscordMember> Users => Guild == null
                ? throw new InvalidOperationException("Cannot query users outside of guild channels.")
                : (IReadOnlyList<DiscordMember>)(Type == ChannelType.Voice || Type == ChannelType.Stage
                ? Guild.Members.Values.Where(x => x.VoiceState?.ChannelId == Id).ToList()
                : Guild.Members.Values.Where(x => (PermissionsFor(x) & Permissions.AccessChannels) == Permissions.AccessChannels).ToList());

    /// <summary>
    /// Gets whether this channel is an NSFW channel.
    /// </summary>
    [JsonProperty("nsfw")]
    public bool IsNSFW { get; internal set; }

    [JsonProperty("rtc_region", NullValueHandling = NullValueHandling.Ignore)]
    internal string RtcRegionId { get; set; }

    /// <summary>
    /// Gets this channel's region override (if voice channel).
    /// </summary>
    [JsonIgnore]
    public DiscordVoiceRegion RtcRegion
        => RtcRegionId != null ? Discord.VoiceRegions[RtcRegionId] : null;

    /// <summary>
    /// Gets the permissions of the user who invoked the command in this channel.
    /// <para>Only sent on the resolved channels of interaction responses for application commands.</para>
    /// </summary>
    [JsonProperty("permissions")]
    public Permissions? UserPermissions { get; internal set; }

    internal DiscordChannel() => _permissionOverwritesLazy = new Lazy<IReadOnlyList<DiscordOverwrite>>(() => new ReadOnlyCollection<DiscordOverwrite>(_permissionOverwrites));

    #region Methods

    /// <summary>
    /// Sends a message to this channel.
    /// </summary>
    /// <param name="content">Content of the message to send.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission if TTS is true and <see cref="Permissions.SendTtsMessages"/> if TTS is true.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordMessage> SendMessageAsync(string content)
    {
        return !Utilities.IsTextableChannel(this)
            ? throw new ArgumentException($"{Type} channels do not support sending text messages.")
            : Discord.ApiClient.CreateMessageAsync(Id, content, null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);
    }

    /// <summary>
    /// Sends a message to this channel.
    /// </summary>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission if TTS is true and <see cref="Permissions.SendTtsMessages"/> if TTS is true.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordMessage> SendMessageAsync(DiscordEmbed embed)
    {
        return !Utilities.IsTextableChannel(this)
            ? throw new ArgumentException($"{Type} channels do not support sending text messages.")
            : Discord.ApiClient.CreateMessageAsync(Id, null, embed != null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);
    }

    /// <summary>
    /// Sends a message to this channel.
    /// </summary>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <param name="content">Content of the message to send.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission if TTS is true and <see cref="Permissions.SendTtsMessages"/> if TTS is true.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordMessage> SendMessageAsync(string content, DiscordEmbed embed)
    {
        return !Utilities.IsTextableChannel(this)
            ? throw new ArgumentException($"{Type} channels do not support sending text messages.")
            : Discord.ApiClient.CreateMessageAsync(Id, content, embed != null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);
    }

    /// <summary>
    /// Sends a message to this channel.
    /// </summary>
    /// <param name="builder">The builder with all the items to send.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission TTS is true and <see cref="Permissions.SendTtsMessages"/> if TTS is true.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordMessage> SendMessageAsync(DiscordMessageBuilder builder)
    {
        return !Utilities.IsTextableChannel(this)
            ? throw new ArgumentException($"{Type} channels do not support sending text messages.")
            : Discord.ApiClient.CreateMessageAsync(Id, builder);
    }

    /// <summary>
    /// Sends a message to this channel.
    /// </summary>
    /// <param name="action">The builder with all the items to send.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission TTS is true and <see cref="Permissions.SendTtsMessages"/> if TTS is true.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordMessage> SendMessageAsync(Action<DiscordMessageBuilder> action)
    {
        if (!Utilities.IsTextableChannel(this))
        {
            throw new ArgumentException($"{Type} channels do not support sending text messages.");
        }

        DiscordMessageBuilder builder = new DiscordMessageBuilder();
        action(builder);


        return Discord.ApiClient.CreateMessageAsync(Id, builder);
    }

    /// <summary>
    /// Creates an event bound to this channel.
    /// </summary>
    /// <param name="name">The name of the event, up to 100 characters.</param>
    /// <param name="description">The description of this event, up to 1000 characters.</param>
    /// <param name="privacyLevel">The privacy level. Currently only <see cref="ScheduledGuildEventPrivacyLevel.GuildOnly"/> is supported</param>
    /// <param name="start">When this event starts.</param>
    /// <param name="end">When this event ends. External events require an end time.</param>
    /// <returns>The created event.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Task<DiscordScheduledGuildEvent> CreateGuildEventAsync(string name, string description, ScheduledGuildEventPrivacyLevel privacyLevel, DateTimeOffset start, DateTimeOffset? end)
        => Type is not (ChannelType.Voice or ChannelType.Stage) ? throw new InvalidOperationException("Events can only be created on voice an stage chnanels") :
            Guild.CreateEventAsync(name, description, Id, Type is ChannelType.Stage ? ScheduledGuildEventType.StageInstance : ScheduledGuildEventType.VoiceChannel, privacyLevel, start, end);

    // Please send memes to Naamloos#2887 at discord <3 thank you

    /// <summary>
    /// Deletes a guild channel
    /// </summary>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteAsync(string reason = null)
        => Discord.ApiClient.DeleteChannelAsync(Id, reason);

    /// <summary>
    /// Clones this channel. This operation will create a channel with identical settings to this one. Note that this will not copy messages.
    /// </summary>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>Newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordChannel> CloneAsync(string reason = null)
    {
        if (Guild == null)
        {
            throw new InvalidOperationException("Non-guild channels cannot be cloned.");
        }

        List<DiscordOverwriteBuilder> ovrs = new List<DiscordOverwriteBuilder>();
        foreach (DiscordOverwrite ovr in _permissionOverwrites)
        {
            ovrs.Add(await new DiscordOverwriteBuilder(member: null).FromAsync(ovr).ConfigureAwait(false));
        }

        int? bitrate = Bitrate;
        int? userLimit = UserLimit;
        Optional<int?> perUserRateLimit = PerUserRateLimit;

        if (Type != ChannelType.Voice)
        {
            bitrate = null;
            userLimit = null;
        }
        if (Type != ChannelType.Text)
        {
            perUserRateLimit = Optional.FromNoValue<int?>();
        }

        return await Guild.CreateChannelAsync(Name, Type, Parent, Topic, bitrate, userLimit, ovrs, IsNSFW, perUserRateLimit, QualityMode, Position, reason).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a specific message
    /// </summary>
    /// <param name="id">The ID of the message</param>
    /// <param name="skipCache">Whether to always make a REST request.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <remarks>Cached message objects will not be returned if <see cref="DiscordConfiguration.MessageCacheSize"/> is set to zero, if the client does not have the <see cref="DiscordIntents.GuildMessages"/> or <see cref="DiscordIntents.DirectMessages"/> intents, or if the discord client is a <see cref="DiscordShardedClient"/>.</remarks>
    public async Task<DiscordMessage> GetMessageAsync(ulong id, bool skipCache = false) => !skipCache
            && Discord.Configuration.MessageCacheSize > 0
            && Discord is DiscordClient dc
            && dc.MessageCache != null
            && dc.MessageCache.TryGet(xm => xm.Id == id && xm.ChannelId == Id, out DiscordMessage? msg)
            ? msg
            : await Discord.ApiClient.GetMessageAsync(Id, id).ConfigureAwait(false);

    /// <summary>
    /// Modifies the current channel.
    /// </summary>
    /// <param name="action">Action to perform on this channel</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task ModifyAsync(Action<ChannelEditModel> action)
    {
        ChannelEditModel mdl = new ChannelEditModel();
        action(mdl);
        return Discord.ApiClient.ModifyChannelAsync(Id, mdl.Name, mdl.Position, mdl.Topic, mdl.Nsfw,
            mdl.Parent.HasValue ? mdl.Parent.Value?.Id : default(Optional<ulong?>), mdl.Bitrate, mdl.Userlimit, mdl.PerUserRateLimit, mdl.RtcRegion.IfPresent(r => r?.Id),
            mdl.QualityMode, mdl.Type, mdl.PermissionOverwrites, mdl.AuditLogReason);
    }

    /// <summary>
    /// Updates the channel position
    /// </summary>
    /// <param name="position">Position the channel should be moved to.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <param name="lockPermissions">Whether to sync channel permissions with the parent, if moving to a new category.</param>
    /// <param name="parentId">The new parent ID if the channel is to be moved to a new category.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task ModifyPositionAsync(int position, string reason = null, bool? lockPermissions = null, ulong? parentId = null)
    {
        if (Guild == null)
        {
            throw new InvalidOperationException("Cannot modify order of non-guild channels.");
        }

        DiscordChannel[] chns = Guild._channels.Values.Where(xc => xc.Type == Type).OrderBy(xc => xc.Position).ToArray();
        RestGuildChannelReorderPayload[] pmds = new RestGuildChannelReorderPayload[chns.Length];
        for (int i = 0; i < chns.Length; i++)
        {
            pmds[i] = new RestGuildChannelReorderPayload
            {
                ChannelId = chns[i].Id,
                Position = chns[i].Id == Id ? position : chns[i].Position >= position ? chns[i].Position + 1 : chns[i].Position,
                LockPermissions = chns[i].Id == Id ? lockPermissions : null,
                ParentId = chns[i].Id == Id ? parentId : null
            };
        }

        return Discord.ApiClient.ModifyGuildChannelPositionAsync(Guild.Id, pmds, reason);
    }

    /// <summary>
    /// Returns a list of messages before a certain message.
    /// <param name="limit">The amount of messages to fetch.</param>
    /// <param name="before">Message to fetch before from.</param>
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordMessage>> GetMessagesBeforeAsync(ulong before, int limit = 100)
        => GetMessagesInternalAsync(limit, before, null, null);

    /// <summary>
    /// Returns a list of messages after a certain message.
    /// <param name="limit">The amount of messages to fetch.</param>
    /// <param name="after">Message to fetch after from.</param>
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordMessage>> GetMessagesAfterAsync(ulong after, int limit = 100)
        => GetMessagesInternalAsync(limit, null, after, null);

    /// <summary>
    /// Returns a list of messages around a certain message.
    /// <param name="limit">The amount of messages to fetch.</param>
    /// <param name="around">Message to fetch around from.</param>
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordMessage>> GetMessagesAroundAsync(ulong around, int limit = 100)
        => GetMessagesInternalAsync(limit, null, null, around);

    /// <summary>
    /// Returns a list of messages from the last message in the channel.
    /// <param name="limit">The amount of messages to fetch.</param>
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordMessage>> GetMessagesAsync(int limit = 100) =>
        GetMessagesInternalAsync(limit, null, null, null);

    private async Task<IReadOnlyList<DiscordMessage>> GetMessagesInternalAsync(int limit = 100, ulong? before = null, ulong? after = null, ulong? around = null)
    {
        if (!Utilities.IsTextableChannel(this))
        {
            throw new ArgumentException($"Cannot get the messages of a {Type} channel.");
        }

        if (limit < 0)
        {
            throw new ArgumentException("Cannot get a negative number of messages.");
        }

        if (limit == 0)
        {
            return Array.Empty<DiscordMessage>();
        }

        //return this.Discord.ApiClient.GetChannelMessagesAsync(this.Id, limit, before, after, around);
        if (limit > 100 && around != null)
        {
            throw new InvalidOperationException("Cannot get more than 100 messages around the specified ID.");
        }

        List<DiscordMessage> msgs = new List<DiscordMessage>(limit);
        int remaining = limit;
        ulong? last = null;
        bool isAfter = after != null;

        int lastCount;
        do
        {
            int fetchSize = remaining > 100 ? 100 : remaining;
            IReadOnlyList<DiscordMessage> fetch = await Discord.ApiClient.GetChannelMessagesAsync(Id, fetchSize, !isAfter ? last ?? before : null, isAfter ? last ?? after : null, around).ConfigureAwait(false);

            lastCount = fetch.Count;
            remaining -= lastCount;

            if (!isAfter)
            {
                msgs.AddRange(fetch);
                last = fetch.LastOrDefault()?.Id;
            }
            else
            {
                msgs.InsertRange(0, fetch);
                last = fetch.FirstOrDefault()?.Id;
            }
        }
        while (remaining > 0 && lastCount > 0);

        return new ReadOnlyCollection<DiscordMessage>(msgs);
    }

    /// <summary>
    /// Gets the threads that are public and archived for this channel.
    /// </summary>
    /// <returns>A <seealso cref="ThreadQueryResult"/> containing the threads for this query and if an other call will yield more threads.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<ThreadQueryResult> ListPublicArchivedThreadsAsync(DateTimeOffset? before = null, int limit = 0)
    {
        return Type != ChannelType.Text && Type != ChannelType.News
            ? throw new InvalidOperationException()
            : Discord.ApiClient.ListPublicArchivedThreadsAsync(GuildId.Value, Id, (ulong?)before?.ToUnixTimeSeconds(), limit);
    }

    /// <summary>
    /// Gets the threads that are private and archived for this channel.
    /// </summary>
    /// <returns>A <seealso cref="ThreadQueryResult"/> containing the threads for this query and if an other call will yield more threads.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ReadMessageHistory"/> and the <see cref="Permissions.ManageThreads"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<ThreadQueryResult> ListPrivateArchivedThreadsAsync(DateTimeOffset? before = null, int limit = 0)
    {
        return Type != ChannelType.Text && Type != ChannelType.News
            ? throw new InvalidOperationException()
            : Discord.ApiClient.ListPrivateArchivedThreadsAsync(GuildId.Value, Id, (ulong?)before?.ToUnixTimeSeconds(), limit);
    }

    /// <summary>
    /// Gets the private and archived threads that the current member has joined in this channel.
    /// </summary>
    /// <returns>A <seealso cref="ThreadQueryResult"/> containing the threads for this query and if an other call will yield more threads.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<ThreadQueryResult> ListJoinedPrivateArchivedThreadsAsync(DateTimeOffset? before = null, int limit = 0)
    {
        return Type != ChannelType.Text && Type != ChannelType.News
            ? throw new InvalidOperationException()
            : Discord.ApiClient.ListJoinedPrivateArchivedThreadsAsync(GuildId.Value, Id, (ulong?)before?.ToUnixTimeSeconds(), limit);
    }

    /// <summary>
    /// Deletes multiple messages if they are less than 14 days old.  If they are older, none of the messages will be deleted and you will receive a <see cref="BadRequestException"/> error.
    /// </summary>
    /// <param name="messages">A collection of messages to delete.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteMessagesAsync(IEnumerable<DiscordMessage> messages, string reason = null)
    {
        // don't enumerate more than once
        ulong[] msgs = messages.Where(x => x.Channel.Id == Id).Select(x => x.Id).ToArray();
        if (messages == null || !msgs.Any())
        {
            throw new ArgumentException("You need to specify at least one message to delete.");
        }

        if (msgs.Count() < 2)
        {
            await Discord.ApiClient.DeleteMessageAsync(Id, msgs.Single(), reason).ConfigureAwait(false);
            return;
        }

        for (int i = 0; i < msgs.Count(); i += 100)
        {
            await Discord.ApiClient.DeleteMessagesAsync(Id, msgs.Skip(i).Take(100), reason).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Deletes a message
    /// </summary>
    /// <param name="message">The message to be deleted.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteMessageAsync(DiscordMessage message, string reason = null)
        => Discord.ApiClient.DeleteMessageAsync(Id, message.Id, reason);

    /// <summary>
    /// Returns a list of invite objects
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.CreateInstantInvite"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordInvite>> GetInvitesAsync() => Guild == null
            ? throw new ArgumentException("Cannot get the invites of a channel that does not belong to a guild.")
            : Discord.ApiClient.GetChannelInvitesAsync(Id);

    /// <summary>
    /// Create a new invite object
    /// </summary>
    /// <param name="max_age">Duration of invite in seconds before expiry, or 0 for never.  Defaults to 86400.</param>
    /// <param name="max_uses">Max number of uses or 0 for unlimited.  Defaults to 0</param>
    /// <param name="temporary">Whether this invite only grants temporary membership.  Defaults to false.</param>
    /// <param name="unique">If true, don't try to reuse a similar invite (useful for creating many unique one time use invites)</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <param name="targetType">The target type of the invite, for stream and embedded application invites.</param>
    /// <param name="targetUserId">The ID of the target user.</param>
    /// <param name="targetApplicationId">The ID of the target application.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.CreateInstantInvite"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordInvite> CreateInviteAsync(int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false, string reason = null, InviteTargetType? targetType = null, ulong? targetUserId = null, ulong? targetApplicationId = null)
        => Discord.ApiClient.CreateChannelInviteAsync(Id, max_age, max_uses, temporary, unique, reason, targetType, targetUserId, targetApplicationId);

    /// <summary>
    /// Adds a channel permission overwrite for specified member.
    /// </summary>
    /// <param name="member">The member to have the permission added.</param>
    /// <param name="allow">The permissions to allow.</param>
    /// <param name="deny">The permissions to deny.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task AddOverwriteAsync(DiscordMember member, Permissions allow = Permissions.None, Permissions deny = Permissions.None, string reason = null)
        => Discord.ApiClient.EditChannelPermissionsAsync(Id, member.Id, allow, deny, "member", reason);

    /// <summary>
    /// Adds a channel permission overwrite for specified role.
    /// </summary>
    /// <param name="role">The role to have the permission added.</param>
    /// <param name="allow">The permissions to allow.</param>
    /// <param name="deny">The permissions to deny.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task AddOverwriteAsync(DiscordRole role, Permissions allow = Permissions.None, Permissions deny = Permissions.None, string reason = null)
        => Discord.ApiClient.EditChannelPermissionsAsync(Id, role.Id, allow, deny, "role", reason);

    /// <summary>
    /// Deletes a channel permission overwrite for the specified member.
    /// </summary>
    /// <param name="member">The member to have the permission deleted.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteOverwriteAsync(DiscordMember member, string reason = null)
        => Discord.ApiClient.DeleteChannelPermissionAsync(Id, member.Id, reason);

    /// <summary>
    /// Deletes a channel permission overwrite for the specified role.
    /// </summary>
    /// <param name="role">The role to have the permission deleted.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteOverwriteAsync(DiscordRole role, string reason = null)
        => Discord.ApiClient.DeleteChannelPermissionAsync(Id, role.Id, reason);

    /// <summary>
    /// Post a typing indicator
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task TriggerTypingAsync() => !Utilities.IsTextableChannel(this)
            ? throw new ArgumentException("Cannot start typing in a non-text channel.")
            : Discord.ApiClient.TriggerTypingAsync(Id);

    /// <summary>
    /// Returns all pinned messages
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync() => !Utilities.IsTextableChannel(this) || Type is ChannelType.Voice
            ? throw new ArgumentException("A non-text channel does not have pinned messages.")
            : Discord.ApiClient.GetPinnedMessagesAsync(Id);

    /// <summary>
    /// Create a new webhook
    /// </summary>
    /// <param name="name">The name of the webhook.</param>
    /// <param name="avatar">The image for the default webhook avatar.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordWebhook> CreateWebhookAsync(string name, Optional<Stream> avatar = default, string reason = null)
    {
        Optional<string> av64 = Optional.FromNoValue<string>();
        if (avatar.HasValue && avatar.Value != null)
        {
            using (ImageTool imgtool = new ImageTool(avatar.Value))
            {
                av64 = imgtool.GetBase64();
            }
        }
        else if (avatar.HasValue)
        {
            av64 = null;
        }

        return await Discord.ApiClient.CreateWebhookAsync(Id, name, av64, reason).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a list of webhooks
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordWebhook>> GetWebhooksAsync()
        => Discord.ApiClient.GetChannelWebhooksAsync(Id);

    /// <summary>
    /// Moves a member to this voice channel
    /// </summary>
    /// <param name="member">The member to be moved.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.MoveMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exists or if the Member does not exists.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task PlaceMemberAsync(DiscordMember member)
    {
        if (Type != ChannelType.Voice && Type != ChannelType.Stage)
        {
            throw new ArgumentException("Cannot place a member in a non-voice channel!"); // be a little more angry, let em learn!!1
        }

        await Discord.ApiClient.ModifyGuildMemberAsync(Guild.Id, member.Id, default, default, default,
            default, Id, default, null).ConfigureAwait(false);
    }

    /// <summary>
    /// Follows a news channel
    /// </summary>
    /// <param name="targetChannel">Channel to crosspost messages to</param>
    /// <exception cref="ArgumentException">Thrown when trying to follow a non-news channel</exception>
    /// <exception cref="UnauthorizedException">Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> on the target channel</exception>
    public Task<DiscordFollowedChannel> FollowAsync(DiscordChannel targetChannel) => Type != ChannelType.News
            ? throw new ArgumentException("Cannot follow a non-news channel.")
            : Discord.ApiClient.FollowChannelAsync(Id, targetChannel.Id);

    /// <summary>
    /// Publishes a message in a news channel to following channels
    /// </summary>
    /// <param name="message">Message to publish</param>
    /// <exception cref="ArgumentException">Thrown when the message has already been crossposted</exception>
    /// <exception cref="UnauthorizedException">
    ///     Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> and/or <see cref="Permissions.SendMessages"/>
    /// </exception>
    public Task<DiscordMessage> CrosspostMessageAsync(DiscordMessage message) => (message.Flags & MessageFlags.Crossposted) == MessageFlags.Crossposted
            ? throw new ArgumentException("Message is already crossposted.")
            : Discord.ApiClient.CrosspostMessageAsync(Id, message.Id);

    /// <summary>
    /// Updates the current user's suppress state in this channel, if stage channel.
    /// </summary>
    /// <param name="suppress">Toggles the suppress state.</param>
    /// <param name="requestToSpeakTimestamp">Sets the time the user requested to speak.</param>
    /// <exception cref="ArgumentException">Thrown when the channel is not a stage channel.</exception>
    public async Task UpdateCurrentUserVoiceStateAsync(bool? suppress, DateTimeOffset? requestToSpeakTimestamp = null)
    {
        if (Type != ChannelType.Stage)
        {
            throw new ArgumentException("Voice state can only be updated in a stage channel.");
        }

        await Discord.ApiClient.UpdateCurrentUserVoiceStateAsync(GuildId.Value, Id, suppress, requestToSpeakTimestamp).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a stage instance in this stage channel.
    /// </summary>
    /// <param name="topic">The topic of the stage instance.</param>
    /// <param name="privacyLevel">The privacy level of the stage instance.</param>
    /// <param name="reason">The reason the stage instance was created.</param>
    /// <returns>The created stage instance.</returns>
    public async Task<DiscordStageInstance> CreateStageInstanceAsync(string topic, PrivacyLevel? privacyLevel = null, string reason = null)
    {
        return Type != ChannelType.Stage
            ? throw new ArgumentException("A stage instance can only be created in a stage channel.")
            : await Discord.ApiClient.CreateStageInstanceAsync(Id, topic, privacyLevel, reason).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the stage instance in this stage channel.
    /// </summary>
    /// <returns>The stage instance in the channel.</returns>
    public async Task<DiscordStageInstance> GetStageInstanceAsync()
    {
        return Type != ChannelType.Stage
            ? throw new ArgumentException("A stage instance can only be created in a stage channel.")
            : await Discord.ApiClient.GetStageInstanceAsync(Id).ConfigureAwait(false);
    }

    /// <summary>
    /// Modifies the stage instance in this stage channel.
    /// </summary>
    /// <param name="action">Action to perform.</param>
    /// <returns>The modified stage instance.</returns>
    public async Task<DiscordStageInstance> ModifyStageInstanceAsync(Action<StageInstanceEditModel> action)
    {
        if (Type != ChannelType.Stage)
        {
            throw new ArgumentException("A stage instance can only be created in a stage channel.");
        }

        StageInstanceEditModel mdl = new StageInstanceEditModel();
        action(mdl);
        return await Discord.ApiClient.ModifyStageInstanceAsync(Id, mdl.Topic, mdl.PrivacyLevel, mdl.AuditLogReason).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the stage instance in this stage channel.
    /// </summary>
    /// <param name="reason">The reason the stage instance was deleted.</param>
    public Task DeleteStageInstanceAsync(string reason = null)
        => Discord.ApiClient.DeleteStageInstanceAsync(Id, reason);

    /// <summary>
    /// Calculates permissions for a given member.
    /// </summary>
    /// <param name="mbr">Member to calculate permissions for.</param>
    /// <returns>Calculated permissions for a given member.</returns>
    public Permissions PermissionsFor(DiscordMember mbr)
    {
        // future note: might be able to simplify @everyone role checks to just check any role ... but I'm not sure
        // xoxo, ~uwx
        //
        // you should use a single tilde
        // ~emzi

        // user > role > everyone
        // allow > deny > undefined
        // =>
        // user allow > user deny > role allow > role deny > everyone allow > everyone deny
        // thanks to meew0


        // Two notes about this: //
        // One: Threads are always synced to their parent. //
        // Two: Threads always have a parent present(?). //
        // If this is a thread, calculate on the parent; doing this on a thread does not work. //
        if (IsThread)
        {
            return Parent.PermissionsFor(mbr);
        }

        if (IsPrivate || Guild == null)
        {
            return Permissions.None;
        }

        if (Guild.OwnerId == mbr.Id)
        {
            return PermissionMethods.FULL_PERMS;
        }

        Permissions perms;

        // assign @everyone permissions
        DiscordRole everyoneRole = Guild.EveryoneRole;
        perms = everyoneRole.Permissions;

        // roles that member is in
        DiscordRole[] mbRoles = mbr.Roles.Where(xr => xr.Id != everyoneRole.Id).ToArray();

        // assign permissions from member's roles (in order)
        perms |= mbRoles.Aggregate(Permissions.None, (c, role) => c | role.Permissions);

        // Administrator grants all permissions and cannot be overridden
        if ((perms & Permissions.Administrator) == Permissions.Administrator)
        {
            return PermissionMethods.FULL_PERMS;
        }

        // channel overrides for roles that member is in
        List<DiscordOverwrite?> mbRoleOverrides = mbRoles
            .Select(xr => _permissionOverwrites.FirstOrDefault(xo => xo.Id == xr.Id))
            .Where(xo => xo != null)
            .ToList();

        // assign channel permission overwrites for @everyone pseudo-role
        DiscordOverwrite? everyoneOverwrites = _permissionOverwrites.FirstOrDefault(xo => xo.Id == everyoneRole.Id);
        if (everyoneOverwrites != null)
        {
            perms &= ~everyoneOverwrites.Denied;
            perms |= everyoneOverwrites.Allowed;
        }

        // assign channel permission overwrites for member's roles (explicit deny)
        perms &= ~mbRoleOverrides.Aggregate(Permissions.None, (c, overs) => c | overs.Denied);
        // assign channel permission overwrites for member's roles (explicit allow)
        perms |= mbRoleOverrides.Aggregate(Permissions.None, (c, overs) => c | overs.Allowed);

        // channel overrides for just this member
        DiscordOverwrite? mbOverrides = _permissionOverwrites.FirstOrDefault(xo => xo.Id == mbr.Id);
        if (mbOverrides == null)
        {
            return perms;
        }

        // assign channel permission overwrites for just this member
        perms &= ~mbOverrides.Denied;
        perms |= mbOverrides.Allowed;

        return perms;
    }

    /// <summary>
    /// Returns a string representation of this channel.
    /// </summary>
    /// <returns>String representation of this channel.</returns>
    public override string ToString()
    {
        if (Type == ChannelType.Category)
        {
            return $"Channel Category {Name} ({Id})";
        }

        return Type == ChannelType.Text || Type == ChannelType.News
            ? $"Channel #{Name} ({Id})"
            : !string.IsNullOrWhiteSpace(Name) ? $"Channel {Name} ({Id})" : $"Channel {Id}";
    }

    #region ThreadMethods

    /// <summary>
    /// Creates a new thread within this channel from the given message.
    /// </summary>
    /// <param name="message">Message to create the thread from.</param>
    /// <param name="name">The name of the thread.</param>
    /// <param name="archiveAfter">The auto archive duration of the thread. 3 day and 7 day archive durations require a level 1 and 2 server boost respectively.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The created thread.</returns>
    /// <exception cref="NotFoundException">Thrown when the channel or message does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordThreadChannel> CreateThreadAsync(DiscordMessage message, string name, AutoArchiveDuration archiveAfter, string reason = null)
    {
        if (Type != ChannelType.Text && Type != ChannelType.News)
        {
            throw new ArgumentException("Threads can only be created within text or news channels.");
        }
        else if (message.ChannelId != Id)
        {
            throw new ArgumentException("You must use a message from this channel to create a thread.");
        }

        DiscordThreadChannel threadChannel = await Discord.ApiClient.CreateThreadFromMessageAsync(Id, message.Id, name, archiveAfter, reason);
        Guild._threads.AddOrUpdate(threadChannel.Id, threadChannel, (key, old) => threadChannel);
        return threadChannel;
    }

    /// <summary>
    /// Creates a new thread within this channel.
    /// </summary>
    /// <param name="name">The name of the thread.</param>
    /// <param name="archiveAfter">The auto archive duration of the thread. 3 day and 7 day archive durations require a level 1 and 2 server boost respectively.</param>
    /// <param name="threadType">The type of thread to create, either a public, news or, private thread. Private threads requires a level 2 server boost and can only be created within channels of type <see cref="ChannelType.Text"/>.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The created thread.</returns>
    /// <exception cref="NotFoundException">Thrown when the channel or message does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordThreadChannel> CreateThreadAsync(string name, AutoArchiveDuration archiveAfter, ChannelType threadType, string reason = null)
    {
        if (Type != ChannelType.Text && Type != ChannelType.News)
        {
            throw new InvalidOperationException("Threads can only be created within text or news channels.");
        }
        else if (Type != ChannelType.News && threadType == ChannelType.NewsThread)
        {
            throw new InvalidOperationException("News threads can only be created within a news channels.");
        }
        else if (threadType != ChannelType.PublicThread && threadType != ChannelType.PrivateThread && threadType != ChannelType.NewsThread)
        {
            throw new ArgumentException("Given channel type for creating a thread is not a valid type of thread.");
        }

        DiscordThreadChannel threadChannel = await Discord.ApiClient.CreateThreadAsync(Id, name, archiveAfter, threadType, reason);
        Guild._threads.AddOrUpdate(threadChannel.Id, threadChannel, (key, old) => threadChannel);
        return threadChannel;
    }

    #endregion

    #endregion

    /// <summary>
    /// Checks whether this <see cref="DiscordChannel"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordChannel"/>.</returns>
    public override bool Equals(object obj) => Equals(obj as DiscordChannel);

    /// <summary>
    /// Checks whether this <see cref="DiscordChannel"/> is equal to another <see cref="DiscordChannel"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordChannel"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordChannel"/> is equal to this <see cref="DiscordChannel"/>.</returns>
    public bool Equals(DiscordChannel e)
    {
        return e is null ? false : ReferenceEquals(this, e) || Id == e.Id;
    }

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordChannel"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordChannel"/>.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="DiscordChannel"/> objects are equal.
    /// </summary>
    /// <param name="e1">First channel to compare.</param>
    /// <param name="e2">Second channel to compare.</param>
    /// <returns>Whether the two channels are equal.</returns>
    public static bool operator ==(DiscordChannel e1, DiscordChannel e2)
    {
        object? o1 = e1 as object;
        object? o2 = e2 as object;

        return (o1 == null && o2 != null) || (o1 != null && o2 == null) ? false : (o1 == null && o2 == null) || e1.Id == e2.Id;
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordChannel"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First channel to compare.</param>
    /// <param name="e2">Second channel to compare.</param>
    /// <returns>Whether the two channels are not equal.</returns>
    public static bool operator !=(DiscordChannel e1, DiscordChannel e2)
        => !(e1 == e2);
}
