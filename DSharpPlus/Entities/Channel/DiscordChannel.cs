using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a discord channel.
/// </summary>
[JsonConverter(typeof(DiscordForumChannelJsonConverter))]
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
        => this.ParentId.HasValue ? this.Guild.GetChannel(this.ParentId.Value) : null;

    /// <summary>
    /// Gets the name of this channel.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the type of this channel.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public virtual ChannelType Type { get; internal set; }

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
        => this.Type == ChannelType.Private || this.Type == ChannelType.Group;

    /// <summary>
    /// Gets whether this channel is a channel category.
    /// </summary>
    [JsonIgnore]
    public bool IsCategory
        => this.Type == ChannelType.Category;

    /// <summary>
    /// Gets whether this channel is a thread.
    /// </summary>
    [JsonIgnore]
    public bool IsThread
        => this.Type == ChannelType.PrivateThread || this.Type == ChannelType.PublicThread || this.Type == ChannelType.NewsThread;

    /// <summary>
    /// Gets the guild to which this channel belongs.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild Guild
        => this.GuildId.HasValue && this.Discord.Guilds.TryGetValue(this.GuildId.Value, out DiscordGuild? guild) ? guild : null;

    /// <summary>
    /// Gets a collection of permission overwrites for this channel.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordOverwrite> PermissionOverwrites
        => this._permissionOverwritesLazy.Value;

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
    /// <remarks>For forum posts, this ID may point to an invalid mesage (e.g. the OP deleted the initial forum message).</remarks>
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
    public IReadOnlyList<DiscordChannel> Children
    {
        get
        {
            return !this.IsCategory
                ? throw new ArgumentException("Only channel categories contain children.")
                : this.Guild._channels.Values.Where(e => e.ParentId == this.Id).ToList();
        }
    }

    /// <summary>
    /// Gets this channel's threads. This applies only to text and news channels.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordThreadChannel> Threads
    {
        get
        {
            return this.Type is not (ChannelType.Text or ChannelType.News or ChannelType.GuildForum)
                ? throw new ArgumentException("Only text channels can have threads.")
                : this.Guild._threads.Values.Where(e => e.ParentId == this.Id).ToArray();
        }
    }

    /// <summary>
    /// Gets the list of members currently in the channel (if voice channel), or members who can see the channel (otherwise).
    /// </summary>
    [JsonIgnore]
    public virtual IReadOnlyList<DiscordMember> Users
    {
        get
        {
            return this.Guild == null
                ? throw new InvalidOperationException("Cannot query users outside of guild channels.")
                : (IReadOnlyList<DiscordMember>)(this.Type == ChannelType.Voice || this.Type == ChannelType.Stage
                ? this.Guild.Members.Values.Where(x => x.VoiceState?.ChannelId == this.Id).ToList()
                : this.Guild.Members.Values.Where(x => (this.PermissionsFor(x) & Permissions.AccessChannels) == Permissions.AccessChannels).ToList());
        }
    }

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
        => this.RtcRegionId != null ? this.Discord.VoiceRegions[this.RtcRegionId] : null;

    /// <summary>
    /// Gets the permissions of the user who invoked the command in this channel.
    /// <para>Only sent on the resolved channels of interaction responses for application commands.</para>
    /// </summary>
    [JsonProperty("permissions")]
    public Permissions? UserPermissions { get; internal set; }

    internal DiscordChannel() => this._permissionOverwritesLazy = new Lazy<IReadOnlyList<DiscordOverwrite>>(() => new ReadOnlyCollection<DiscordOverwrite>(this._permissionOverwrites));

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
    public async Task<DiscordMessage> SendMessageAsync(string content)
    {
        return !Utilities.IsTextableChannel(this)
            ? throw new ArgumentException($"{this.Type} channels do not support sending text messages.")
            : await this.Discord.ApiClient.CreateMessageAsync(this.Id, content, null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);
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
    public async Task<DiscordMessage> SendMessageAsync(DiscordEmbed embed)
    {
        return !Utilities.IsTextableChannel(this)
            ? throw new ArgumentException($"{this.Type} channels do not support sending text messages.")
            : await this.Discord.ApiClient.CreateMessageAsync(this.Id, null, embed != null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);
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
    public async Task<DiscordMessage> SendMessageAsync(string content, DiscordEmbed embed)
    {
        return !Utilities.IsTextableChannel(this)
            ? throw new ArgumentException($"{this.Type} channels do not support sending text messages.")
            : await this.Discord.ApiClient.CreateMessageAsync(this.Id, content, embed != null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);
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
    public async Task<DiscordMessage> SendMessageAsync(DiscordMessageBuilder builder)
    {
        return !Utilities.IsTextableChannel(this)
            ? throw new ArgumentException($"{this.Type} channels do not support sending text messages.")
            : await this.Discord.ApiClient.CreateMessageAsync(this.Id, builder);
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
    public async Task<DiscordMessage> SendMessageAsync(Action<DiscordMessageBuilder> action)
    {
        if (!Utilities.IsTextableChannel(this))
        {
            throw new ArgumentException($"{this.Type} channels do not support sending text messages.");
        }

        DiscordMessageBuilder builder = new();
        action(builder);

        return await this.Discord.ApiClient.CreateMessageAsync(this.Id, builder);
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
        => this.Type is not (ChannelType.Voice or ChannelType.Stage) ? throw new InvalidOperationException("Events can only be created on voice an stage chnanels") :
            this.Guild.CreateEventAsync(name, description, this.Id, this.Type is ChannelType.Stage ? ScheduledGuildEventType.StageInstance : ScheduledGuildEventType.VoiceChannel, privacyLevel, start, end);

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
    public async Task DeleteAsync(string reason = null)
        => await this.Discord.ApiClient.DeleteChannelAsync(this.Id, reason);

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
        if (this.Guild == null)
        {
            throw new InvalidOperationException("Non-guild channels cannot be cloned.");
        }

        List<DiscordOverwriteBuilder> ovrs = new();
        foreach (DiscordOverwrite ovr in this._permissionOverwrites)
        {
            ovrs.Add(await new DiscordOverwriteBuilder(member: null).FromAsync(ovr));
        }

        int? bitrate = this.Bitrate;
        int? userLimit = this.UserLimit;
        Optional<int?> perUserRateLimit = this.PerUserRateLimit;

        if (this.Type != ChannelType.Voice)
        {
            bitrate = null;
            userLimit = null;
        }

        if (this.Type != ChannelType.Text)
        {
            perUserRateLimit = Optional.FromNoValue<int?>();
        }

        return await this.Guild.CreateChannelAsync(this.Name, this.Type, this.Parent, this.Topic, bitrate, userLimit, ovrs, this.IsNSFW, perUserRateLimit, this.QualityMode, this.Position, reason);
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
    public async Task<DiscordMessage> GetMessageAsync(ulong id, bool skipCache = false)
    {
        return !skipCache
            && this.Discord.Configuration.MessageCacheSize > 0
            && this.Discord is DiscordClient dc
            && dc.MessageCache != null
            && dc.MessageCache.TryGet(id, out DiscordMessage? msg)
            ? msg
            : await this.Discord.ApiClient.GetMessageAsync(this.Id, id);
    }

    /// <summary>
    /// Modifies the current channel.
    /// </summary>
    /// <param name="action">Action to perform on this channel</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ModifyAsync(Action<ChannelEditModel> action)
    {
        ChannelEditModel mdl = new();
        action(mdl);
        await this.Discord.ApiClient.ModifyChannelAsync
        (
            this.Id,
            mdl.Name,
            mdl.Position,
            mdl.Topic,
            mdl.Nsfw,
            mdl.Parent.HasValue ? mdl.Parent.Value?.Id : default(Optional<ulong?>),
            mdl.Bitrate,
            mdl.Userlimit,
            mdl.PerUserRateLimit,
            mdl.RtcRegion.IfPresent(r => r?.Id),
            mdl.QualityMode,
            mdl.Type,
            mdl.PermissionOverwrites,
            mdl.Flags,
            mdl.AvailableTags,
            mdl.DefaultAutoArchiveDuration,
            mdl.DefaultReaction,
            mdl.DefaultThreadRateLimit,
            mdl.DefaultSortOrder,
            mdl.DefaultForumLayout,
            mdl.AuditLogReason
        );
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
    public async Task ModifyPositionAsync(int position, string reason = null, bool? lockPermissions = null, ulong? parentId = null)
    {
        if (this.Guild is null)
        {
            throw new InvalidOperationException("Cannot modify order of non-guild channels.");
        }

        DiscordChannel[] chns = this.Guild._channels.Values.Where(xc => xc.Type == this.Type).OrderBy(xc => xc.Position).ToArray();
        RestGuildChannelReorderPayload[] pmds = new RestGuildChannelReorderPayload[chns.Length];
        for (int i = 0; i < chns.Length; i++)
        {
            pmds[i] = new()
            {
                ChannelId = chns[i].Id,
                Position = chns[i].Id == this.Id ? position : chns[i].Position >= position ? chns[i].Position + 1 : chns[i].Position,
                LockPermissions = chns[i].Id == this.Id ? lockPermissions : null,
                ParentId = chns[i].Id == this.Id ? parentId : null
            };
        }

        await this.Discord.ApiClient.ModifyGuildChannelPositionAsync(this.Guild.Id, pmds, reason);
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
    public IAsyncEnumerable<DiscordMessage> GetMessagesBeforeAsync(ulong before, int limit = 100)
        => this.GetMessagesInternalAsync(limit, before, null, null);

    /// <summary>
    /// Returns a list of messages after a certain message.
    /// <param name="limit">The amount of messages to fetch.</param>
    /// <param name="after">Message to fetch after from.</param>
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public IAsyncEnumerable<DiscordMessage> GetMessagesAfterAsync(ulong after, int limit = 100)
        => this.GetMessagesInternalAsync(limit, null, after, null);

    /// <summary>
    /// Returns a list of messages around a certain message.
    /// <param name="limit">The amount of messages to fetch.</param>
    /// <param name="around">Message to fetch around from.</param>
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public IAsyncEnumerable<DiscordMessage> GetMessagesAroundAsync(ulong around, int limit = 100)
        => this.GetMessagesInternalAsync(limit, null, null, around);

    /// <summary>
    /// Returns a list of messages from the last message in the channel.
    /// <param name="limit">The amount of messages to fetch.</param>
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public IAsyncEnumerable<DiscordMessage> GetMessagesAsync(int limit = 100) =>
        this.GetMessagesInternalAsync(limit, null, null, null);

    private async IAsyncEnumerable<DiscordMessage> GetMessagesInternalAsync(int limit = 100, ulong? before = null, ulong? after = null, ulong? around = null)
    {
        if (!Utilities.IsTextableChannel(this))
        {
            throw new ArgumentException($"Cannot get the messages of a {this.Type} channel.");
        }

        if (limit < 0)
        {
            throw new ArgumentException("Cannot get a negative number of messages.");
        }

        if (limit == 0)
        {
            yield break;
        }

        //return this.Discord.ApiClient.GetChannelMessagesAsync(this.Id, limit, before, after, around);
        if (limit > 100 && around != null)
        {
            throw new InvalidOperationException("Cannot get more than 100 messages around the specified ID.");
        }

        List<DiscordMessage> msgs = new(limit);
        int remaining = limit;
        ulong? last = null;
        bool isbefore = before != null;

        int lastCount;
        do
        {
            int fetchSize = remaining > 100 ? 100 : remaining;
            IReadOnlyList<DiscordMessage> fetchedMessages = await this.Discord.ApiClient.GetChannelMessagesAsync(this.Id, fetchSize, isbefore ? last ?? before : null, !isbefore ? last ?? after : null, around);

            lastCount = fetchedMessages.Count;
            remaining -= lastCount;
            
            //We sort the returned messages by ID so that they are in order in case Discord switches the order AGAIN.
            DiscordMessage[] sortedMessageArray = fetchedMessages.ToArray();
            Array.Sort(sortedMessageArray, (x, y) => x.Id.CompareTo(y.Id));
            

            if (!isbefore)
            {
                foreach (DiscordMessage msg in sortedMessageArray)
                {
                    yield return msg;
                }
                last = sortedMessageArray.LastOrDefault()?.Id;
            }
            else
            {
                for (int i = sortedMessageArray.Length - 1; i >= 0; i--)
                {
                    yield return sortedMessageArray[i];
                }
                last = sortedMessageArray.FirstOrDefault()?.Id;
            }
        }
        while (remaining > 0 && lastCount > 0 && lastCount == 100);
    }

    /// <summary>
    /// Gets the threads that are public and archived for this channel.
    /// </summary>
    /// <returns>A <seealso cref="ThreadQueryResult"/> containing the threads for this query and if an other call will yield more threads.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<ThreadQueryResult> ListPublicArchivedThreadsAsync(DateTimeOffset? before = null, int limit = 0)
    {
        return this.Type != ChannelType.Text && this.Type != ChannelType.News
            ? throw new InvalidOperationException()
            : await this.Discord.ApiClient.ListPublicArchivedThreadsAsync(this.GuildId.Value, this.Id, before?.ToString("o"), limit);
    }

    /// <summary>
    /// Gets the threads that are private and archived for this channel.
    /// </summary>
    /// <returns>A <seealso cref="ThreadQueryResult"/> containing the threads for this query and if an other call will yield more threads.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ReadMessageHistory"/> and the <see cref="Permissions.ManageThreads"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<ThreadQueryResult> ListPrivateArchivedThreadsAsync(DateTimeOffset? before = null, int limit = 0)
    {
        return this.Type != ChannelType.Text && this.Type != ChannelType.News
            ? throw new InvalidOperationException()
            : await this.Discord.ApiClient.ListPrivateArchivedThreadsAsync(this.GuildId.Value, this.Id, limit, before?.ToString("o"));
    }

    /// <summary>
    /// Gets the private and archived threads that the current member has joined in this channel.
    /// </summary>
    /// <returns>A <seealso cref="ThreadQueryResult"/> containing the threads for this query and if an other call will yield more threads.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<ThreadQueryResult> ListJoinedPrivateArchivedThreadsAsync(DateTimeOffset? before = null, int limit = 0)
    {
        return this.Type != ChannelType.Text && this.Type != ChannelType.News
            ? throw new InvalidOperationException()
            : await this.Discord.ApiClient.ListJoinedPrivateArchivedThreadsAsync(this.GuildId.Value, this.Id, limit, (ulong?)before?.ToUnixTimeSeconds());
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
        ulong[] msgs = messages.Where(x => x.Channel.Id == this.Id).Select(x => x.Id).ToArray();
        if (messages == null || !msgs.Any())
        {
            throw new ArgumentException("You need to specify at least one message to delete.");
        }

        if (msgs.Count() < 2)
        {
            await this.Discord.ApiClient.DeleteMessageAsync(this.Id, msgs.Single(), reason);
            return;
        }

        for (int i = 0; i < msgs.Count(); i += 100)
        {
            await this.Discord.ApiClient.DeleteMessagesAsync(this.Id, msgs.Skip(i).Take(100), reason);
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
    public async Task DeleteMessageAsync(DiscordMessage message, string reason = null)
        => await this.Discord.ApiClient.DeleteMessageAsync(this.Id, message.Id, reason);

    /// <summary>
    /// Returns a list of invite objects
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.CreateInstantInvite"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordInvite>> GetInvitesAsync()
    {
        return this.Guild == null
            ? throw new ArgumentException("Cannot get the invites of a channel that does not belong to a guild.")
            : await this.Discord.ApiClient.GetChannelInvitesAsync(this.Id);
    }

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
    public async Task<DiscordInvite> CreateInviteAsync(int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false, string reason = null, InviteTargetType? targetType = null, ulong? targetUserId = null, ulong? targetApplicationId = null)
        => await this.Discord.ApiClient.CreateChannelInviteAsync(this.Id, max_age, max_uses, temporary, unique, reason, targetType, targetUserId, targetApplicationId);

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
    public async Task AddOverwriteAsync(DiscordMember member, Permissions allow = Permissions.None, Permissions deny = Permissions.None, string reason = null)
        => await this.Discord.ApiClient.EditChannelPermissionsAsync(this.Id, member.Id, allow, deny, "member", reason);

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
    public async Task AddOverwriteAsync(DiscordRole role, Permissions allow = Permissions.None, Permissions deny = Permissions.None, string reason = null)
        => await this.Discord.ApiClient.EditChannelPermissionsAsync(this.Id, role.Id, allow, deny, "role", reason);

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
    public async Task DeleteOverwriteAsync(DiscordMember member, string reason = null)
        => await this.Discord.ApiClient.DeleteChannelPermissionAsync(this.Id, member.Id, reason);

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
    public async Task DeleteOverwriteAsync(DiscordRole role, string reason = null)
        => await this.Discord.ApiClient.DeleteChannelPermissionAsync(this.Id, role.Id, reason);

    /// <summary>
    /// Post a typing indicator
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task TriggerTypingAsync()
    {
        if (!Utilities.IsTextableChannel(this))
        {
            throw new ArgumentException("Cannot start typing in a non-text channel.");
        }
        else
        {
            await this.Discord.ApiClient.TriggerTypingAsync(this.Id);
        }
    }

    /// <summary>
    /// Returns all pinned messages
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync()
    {
        return !Utilities.IsTextableChannel(this) || this.Type is ChannelType.Voice
            ? throw new ArgumentException("A non-text channel does not have pinned messages.")
            : await this.Discord.ApiClient.GetPinnedMessagesAsync(this.Id);
    }

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
            using ImageTool imgtool = new(avatar.Value);
            av64 = imgtool.GetBase64();
        }
        else if (avatar.HasValue)
        {
            av64 = null;
        }

        return await this.Discord.ApiClient.CreateWebhookAsync(this.Id, name, av64, reason);
    }

    /// <summary>
    /// Returns a list of webhooks
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordWebhook>> GetWebhooksAsync()
        => await this.Discord.ApiClient.GetChannelWebhooksAsync(this.Id);

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
        if (this.Type != ChannelType.Voice && this.Type != ChannelType.Stage)
        {
            throw new ArgumentException("Cannot place a member in a non-voice channel!"); // be a little more angry, let em learn!!1
        }

        await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, member.Id, default, default, default,
            default, this.Id, default, null);
    }

    /// <summary>
    /// Follows a news channel
    /// </summary>
    /// <param name="targetChannel">Channel to crosspost messages to</param>
    /// <exception cref="ArgumentException">Thrown when trying to follow a non-news channel</exception>
    /// <exception cref="UnauthorizedException">Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> on the target channel</exception>
    public async Task<DiscordFollowedChannel> FollowAsync(DiscordChannel targetChannel)
    {
        return this.Type != ChannelType.News
            ? throw new ArgumentException("Cannot follow a non-news channel.")
            : await this.Discord.ApiClient.FollowChannelAsync(this.Id, targetChannel.Id);
    }

    /// <summary>
    /// Publishes a message in a news channel to following channels
    /// </summary>
    /// <param name="message">Message to publish</param>
    /// <exception cref="ArgumentException">Thrown when the message has already been crossposted</exception>
    /// <exception cref="UnauthorizedException">
    ///     Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> and/or <see cref="Permissions.SendMessages"/>
    /// </exception>
    public async Task<DiscordMessage> CrosspostMessageAsync(DiscordMessage message)
    {
        return (message.Flags & MessageFlags.Crossposted) == MessageFlags.Crossposted
            ? throw new ArgumentException("Message is already crossposted.")
            : await this.Discord.ApiClient.CrosspostMessageAsync(this.Id, message.Id);
    }

    /// <summary>
    /// Updates the current user's suppress state in this channel, if stage channel.
    /// </summary>
    /// <param name="suppress">Toggles the suppress state.</param>
    /// <param name="requestToSpeakTimestamp">Sets the time the user requested to speak.</param>
    /// <exception cref="ArgumentException">Thrown when the channel is not a stage channel.</exception>
    public async Task UpdateCurrentUserVoiceStateAsync(bool? suppress, DateTimeOffset? requestToSpeakTimestamp = null)
    {
        if (this.Type != ChannelType.Stage)
        {
            throw new ArgumentException("Voice state can only be updated in a stage channel.");
        }

        await this.Discord.ApiClient.UpdateCurrentUserVoiceStateAsync(this.GuildId.Value, this.Id, suppress, requestToSpeakTimestamp);
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
        return this.Type != ChannelType.Stage
            ? throw new ArgumentException("A stage instance can only be created in a stage channel.")
            : await this.Discord.ApiClient.CreateStageInstanceAsync(this.Id, topic, privacyLevel, reason);
    }

    /// <summary>
    /// Gets the stage instance in this stage channel.
    /// </summary>
    /// <returns>The stage instance in the channel.</returns>
    public async Task<DiscordStageInstance> GetStageInstanceAsync()
    {
        return this.Type != ChannelType.Stage
            ? throw new ArgumentException("A stage instance can only be created in a stage channel.")
            : await this.Discord.ApiClient.GetStageInstanceAsync(this.Id);
    }

    /// <summary>
    /// Modifies the stage instance in this stage channel.
    /// </summary>
    /// <param name="action">Action to perform.</param>
    /// <returns>The modified stage instance.</returns>
    public async Task<DiscordStageInstance> ModifyStageInstanceAsync(Action<StageInstanceEditModel> action)
    {
        if (this.Type != ChannelType.Stage)
        {
            throw new ArgumentException("A stage instance can only be created in a stage channel.");
        }

        StageInstanceEditModel mdl = new();
        action(mdl);
        return await this.Discord.ApiClient.ModifyStageInstanceAsync(this.Id, mdl.Topic, mdl.PrivacyLevel, mdl.AuditLogReason);
    }

    /// <summary>
    /// Deletes the stage instance in this stage channel.
    /// </summary>
    /// <param name="reason">The reason the stage instance was deleted.</param>
    public async Task DeleteStageInstanceAsync(string reason = null)
        => await this.Discord.ApiClient.DeleteStageInstanceAsync(this.Id, reason);

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
        if (this.IsThread)
        {
            return this.Parent.PermissionsFor(mbr);
        }

        if (this.IsPrivate || this.Guild == null)
        {
            return Permissions.None;
        }

        if (this.Guild.OwnerId == mbr.Id)
        {
            return PermissionMethods.FULL_PERMS;
        }

        Permissions perms;

        // assign @everyone permissions
        DiscordRole everyoneRole = this.Guild.EveryoneRole;
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
            .Select(xr => this._permissionOverwrites.FirstOrDefault(xo => xo.Id == xr.Id))
            .Where(xo => xo != null)
            .ToList();

        // assign channel permission overwrites for @everyone pseudo-role
        DiscordOverwrite? everyoneOverwrites = this._permissionOverwrites.FirstOrDefault(xo => xo.Id == everyoneRole.Id);
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
        DiscordOverwrite? mbOverrides = this._permissionOverwrites.FirstOrDefault(xo => xo.Id == mbr.Id);
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
#pragma warning disable IDE0046 // we don't want this to become a double ternary
        if (this.Type == ChannelType.Category)
        {
            return $"Channel Category {this.Name} ({this.Id})";
        }

        return this.Type == ChannelType.Text || this.Type == ChannelType.News
            ? $"Channel #{this.Name} ({this.Id})"
            : !string.IsNullOrWhiteSpace(this.Name) ? $"Channel {this.Name} ({this.Id})" : $"Channel {this.Id}";
#pragma warning restore IDE0046
    }

    #region ThreadMethods

    /// <summary>
    /// Creates a new thread within this channel from the given message.
    /// </summary>
    /// <param name="message">Message to create the thread from.</param>
    /// <param name="name">The name of the thread.</param>
    /// <param name="archiveAfter">The auto archive duration of the thread.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The created thread.</returns>
    /// <exception cref="NotFoundException">Thrown when the channel or message does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordThreadChannel> CreateThreadAsync(DiscordMessage message, string name, AutoArchiveDuration archiveAfter, string reason = null)
    {
        if (this.Type != ChannelType.Text && this.Type != ChannelType.News)
        {
            throw new ArgumentException("Threads can only be created within text or news channels.");
        }
        else if (message.ChannelId != this.Id)
        {
            throw new ArgumentException("You must use a message from this channel to create a thread.");
        }

        DiscordThreadChannel threadChannel = await this.Discord.ApiClient.CreateThreadFromMessageAsync(this.Id, message.Id, name, archiveAfter, reason);
        this.Guild._threads.AddOrUpdate(threadChannel.Id, threadChannel, (key, old) => threadChannel);
        return threadChannel;
    }

    /// <summary>
    /// Creates a new thread within this channel.
    /// </summary>
    /// <param name="name">The name of the thread.</param>
    /// <param name="archiveAfter">The auto archive duration of the thread.</param>
    /// <param name="threadType">The type of thread to create, either a public, news or, private thread.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The created thread.</returns>
    /// <exception cref="NotFoundException">Thrown when the channel or message does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordThreadChannel> CreateThreadAsync(string name, AutoArchiveDuration archiveAfter, ChannelType threadType, string reason = null)
    {
        if (this.Type != ChannelType.Text && this.Type != ChannelType.News)
        {
            throw new InvalidOperationException("Threads can only be created within text or news channels.");
        }
        else if (this.Type != ChannelType.News && threadType == ChannelType.NewsThread)
        {
            throw new InvalidOperationException("News threads can only be created within a news channels.");
        }
        else if (threadType != ChannelType.PublicThread && threadType != ChannelType.PrivateThread && threadType != ChannelType.NewsThread)
        {
            throw new ArgumentException("Given channel type for creating a thread is not a valid type of thread.");
        }

        DiscordThreadChannel threadChannel = await this.Discord.ApiClient.CreateThreadAsync(this.Id, name, archiveAfter, threadType, reason);
        this.Guild._threads.AddOrUpdate(threadChannel.Id, threadChannel, (key, old) => threadChannel);
        return threadChannel;
    }

    #endregion

    #endregion

    /// <summary>
    /// Checks whether this <see cref="DiscordChannel"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordChannel"/>.</returns>
    public override bool Equals(object obj) => this.Equals(obj as DiscordChannel);

    /// <summary>
    /// Checks whether this <see cref="DiscordChannel"/> is equal to another <see cref="DiscordChannel"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordChannel"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordChannel"/> is equal to this <see cref="DiscordChannel"/>.</returns>
    public bool Equals(DiscordChannel e) => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordChannel"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordChannel"/>.</returns>
    public override int GetHashCode() => this.Id.GetHashCode();

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

        return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
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
