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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord guild.
/// </summary>
public class DiscordGuild : SnowflakeObject, IEquatable<DiscordGuild>
{
    /// <summary>
    /// Gets the guild's name.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the guild icon's hash.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
    public string IconHash { get; internal set; }

    /// <summary>
    /// Gets the guild icon's url.
    /// </summary>
    [JsonIgnore]
    public string IconUrl
        => GetIconUrl(ImageFormat.Auto, 1024);

    /// <summary>
    /// Gets the guild splash's hash.
    /// </summary>
    [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
    public string SplashHash { get; internal set; }

    /// <summary>
    /// Gets the guild splash's url.
    /// </summary>
    [JsonIgnore]
    public string SplashUrl
        => !string.IsNullOrWhiteSpace(SplashHash) ? $"https://cdn.discordapp.com/splashes/{Id.ToString(CultureInfo.InvariantCulture)}/{SplashHash}.jpg" : null;

    /// <summary>
    /// Gets the guild discovery splash's hash.
    /// </summary>
    [JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
    public string DiscoverySplashHash { get; internal set; }

    /// <summary>
    /// Gets the guild discovery splash's url.
    /// </summary>
    [JsonIgnore]
    public string DiscoverySplashUrl
        => !string.IsNullOrWhiteSpace(DiscoverySplashHash) ? $"https://cdn.discordapp.com/discovery-splashes/{Id.ToString(CultureInfo.InvariantCulture)}/{DiscoverySplashHash}.jpg" : null;

    /// <summary>
    /// Gets the preferred locale of this guild.
    /// <para>This is used for server discovery and notices from Discord. Defaults to en-US.</para>
    /// </summary>
    [JsonProperty("preferred_locale", NullValueHandling = NullValueHandling.Ignore)]
    public string PreferredLocale { get; internal set; }

    /// <summary>
    /// Gets the ID of the guild's owner.
    /// </summary>
    [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong OwnerId { get; internal set; }

    /// <summary>
    /// Gets permissions for the user in the guild (does not include channel overrides)
    /// </summary>
    [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
    public Permissions? Permissions { get; set; }

    /// <summary>
    /// Gets the guild's owner.
    /// </summary>
    [JsonIgnore]
    public DiscordMember Owner
        => Members.TryGetValue(OwnerId, out DiscordMember? owner)
            ? owner
            : Discord.ApiClient.GetGuildMemberAsync(Id, OwnerId).ConfigureAwait(false).GetAwaiter().GetResult();

    /// <summary>
    /// Gets the guild's voice region ID.
    /// </summary>
    [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
    internal string _voiceRegionId { get; set; }

    /// <summary>
    /// Gets the guild's voice region.
    /// </summary>
    [JsonIgnore]
    public DiscordVoiceRegion VoiceRegion
        => Discord.VoiceRegions[_voiceRegionId];

    /// <summary>
    /// Gets the guild's AFK voice channel ID.
    /// </summary>
    [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong _afkChannelId { get; set; } = 0;

    /// <summary>
    /// Gets the guild's AFK voice channel.
    /// </summary>
    [JsonIgnore]
    public DiscordChannel AfkChannel
        => GetChannel(_afkChannelId);

    /// <summary>
    /// Gets the guild's AFK timeout.
    /// </summary>
    [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
    public int AfkTimeout { get; internal set; }

    /// <summary>
    /// Gets the guild's verification level.
    /// </summary>
    [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
    public VerificationLevel VerificationLevel { get; internal set; }

    /// <summary>
    /// Gets the guild's default notification settings.
    /// </summary>
    [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
    public DefaultMessageNotifications DefaultMessageNotifications { get; internal set; }

    /// <summary>
    /// Gets the guild's explicit content filter settings.
    /// </summary>
    [JsonProperty("explicit_content_filter")]
    public ExplicitContentFilter ExplicitContentFilter { get; internal set; }

    /// <summary>
    /// Gets the guild's nsfw level.
    /// </summary>
    [JsonProperty("nsfw_level")]
    public NsfwLevel NsfwLevel { get; internal set; }

    [JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Include)]
    internal ulong? _systemChannelId { get; set; }

    /// <summary>
    /// Gets the channel where system messages (such as boost and welcome messages) are sent.
    /// </summary>
    [JsonIgnore]
    public DiscordChannel SystemChannel => _systemChannelId.HasValue
        ? GetChannel(_systemChannelId.Value)
        : null;

    /// <summary>
    /// Gets the settings for this guild's system channel.
    /// </summary>
    [JsonProperty("system_channel_flags")]
    public SystemChannelFlags SystemChannelFlags { get; internal set; }

    /// <summary>
    /// Gets whether this guild's widget is enabled.
    /// </summary>
    [JsonProperty("widget_enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool? WidgetEnabled { get; internal set; }

    [JsonProperty("widget_channel_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong? _widgetChannelId { get; set; }

    /// <summary>
    /// Gets the widget channel for this guild.
    /// </summary>
    [JsonIgnore]
    public DiscordChannel WidgetChannel => _widgetChannelId.HasValue
        ? GetChannel(_widgetChannelId.Value)
        : null;

    [JsonProperty("rules_channel_id")]
    internal ulong? _rulesChannelId { get; set; }

    /// <summary>
    /// Gets the rules channel for this guild.
    /// <para>This is only available if the guild is considered "discoverable".</para>
    /// </summary>
    [JsonIgnore]
    public DiscordChannel RulesChannel => _rulesChannelId.HasValue
        ? GetChannel(_rulesChannelId.Value)
        : null;

    [JsonProperty("public_updates_channel_id")]
    internal ulong? _publicUpdatesChannelId { get; set; }

    /// <summary>
    /// Gets the public updates channel (where admins and moderators receive messages from Discord) for this guild.
    /// <para>This is only available if the guild is considered "discoverable".</para>
    /// </summary>
    [JsonIgnore]
    public DiscordChannel PublicUpdatesChannel => _publicUpdatesChannelId.HasValue
        ? GetChannel(_publicUpdatesChannelId.Value)
        : null;

    /// <summary>
    /// Gets the application ID of this guild if it is bot created.
    /// </summary>
    [JsonProperty("application_id")]
    public ulong? ApplicationId { get; internal set; }

    /// <summary>
    /// Sceduled events for this guild.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordScheduledGuildEvent> ScheduledEvents
        => new ReadOnlyConcurrentDictionary<ulong, DiscordScheduledGuildEvent>(_scheduledEvents);

    [JsonProperty("guild_scheduled_events")]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordScheduledGuildEvent> _scheduledEvents = new();


    /// <summary>
    /// Gets a collection of this guild's roles.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordRole> Roles => new ReadOnlyConcurrentDictionary<ulong, DiscordRole>(_roles);

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordRole> _roles;


    /// <summary>
    /// Gets a collection of this guild's stickers.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordMessageSticker> Stickers => new ReadOnlyConcurrentDictionary<ulong, DiscordMessageSticker>(_stickers);

    [JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordMessageSticker> _stickers = new();


    /// <summary>
    /// Gets a collection of this guild's emojis.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordEmoji> Emojis => new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(_emojis);

    [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordEmoji> _emojis;

    /// <summary>
    /// Gets a collection of this guild's features.
    /// </summary>
    [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<string> Features { get; internal set; }

    /// <summary>
    /// Gets the required multi-factor authentication level for this guild.
    /// </summary>
    [JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
    public MfaLevel MfaLevel { get; internal set; }

    /// <summary>
    /// Gets this guild's join date.
    /// </summary>
    [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset JoinedAt { get; internal set; }

    /// <summary>
    /// Gets whether this guild is considered to be a large guild.
    /// </summary>
    [JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsLarge { get; internal set; }

    /// <summary>
    /// Gets whether this guild is unavailable.
    /// </summary>
    [JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsUnavailable { get; internal set; }

    /// <summary>
    /// Gets the total number of members in this guild.
    /// </summary>
    [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
    public int MemberCount { get; internal set; }

    /// <summary>
    /// Gets the maximum amount of members allowed for this guild.
    /// </summary>
    [JsonProperty("max_members")]
    public int? MaxMembers { get; internal set; }

    /// <summary>
    /// Gets the maximum amount of presences allowed for this guild.
    /// </summary>
    [JsonProperty("max_presences")]
    public int? MaxPresences { get; internal set; }

#pragma warning disable CS1734
    /// <summary>
    /// Gets the approximate number of members in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name="withCounts"></paramref> set to true.
    /// </summary>
    [JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
    public int? ApproximateMemberCount { get; internal set; }

    /// <summary>
    /// Gets the approximate number of presences in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name="withCounts"></paramref> set to true.
    /// </summary>
    [JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
    public int? ApproximatePresenceCount { get; internal set; }
#pragma warning restore CS1734

    /// <summary>
    /// Gets the maximum amount of users allowed per video channel.
    /// </summary>
    [JsonProperty("max_video_channel_users", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxVideoChannelUsers { get; internal set; }

    /// <summary>
    /// Gets a dictionary of all the voice states for this guilds. The key for this dictionary is the ID of the user
    /// the voice state corresponds to.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordVoiceState> VoiceStates => new ReadOnlyConcurrentDictionary<ulong, DiscordVoiceState>(_voiceStates);

    [JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordVoiceState> _voiceStates;

    /// <summary>
    /// Gets a dictionary of all the members that belong to this guild. The dictionary's key is the member ID.
    /// </summary>
    [JsonIgnore] // TODO overhead of => vs Lazy? it's a struct
    public IReadOnlyDictionary<ulong, DiscordMember> Members => new ReadOnlyConcurrentDictionary<ulong, DiscordMember>(_members);

    [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordMember> _members;

    /// <summary>
    /// Gets a dictionary of all the channels associated with this guild. The dictionary's key is the channel ID.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordChannel> Channels => new ReadOnlyConcurrentDictionary<ulong, DiscordChannel>(_channels);

    [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordChannel> _channels;

    /// <summary>
    /// Gets a dictionary of all the active threads associated with this guild the user has permission to view. The dictionary's key is the channel ID.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordThreadChannel> Threads => new ReadOnlyConcurrentDictionary<ulong, DiscordThreadChannel>(_threads);

    [JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordThreadChannel> _threads = new();

    internal ConcurrentDictionary<string, DiscordInvite> _invites;

    /// <summary>
    /// Gets the guild member for current user.
    /// </summary>
    [JsonIgnore]
    public DiscordMember CurrentMember
        => _current_member_lazy.Value;

    [JsonIgnore]
    private readonly Lazy<DiscordMember> _current_member_lazy;

    /// <summary>
    /// Gets the @everyone role for this guild.
    /// </summary>
    [JsonIgnore]
    public DiscordRole EveryoneRole
        => GetRole(Id);

    [JsonIgnore]
    internal bool _isOwner;

    /// <summary>
    /// Gets whether the current user is the guild's owner.
    /// </summary>
    [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsOwner
    {
        get => _isOwner || OwnerId == Discord.CurrentUser.Id;
        internal set => _isOwner = value;
    }

    /// <summary>
    /// Gets the vanity URL code for this guild, when applicable.
    /// </summary>
    [JsonProperty("vanity_url_code")]
    public string VanityUrlCode { get; internal set; }

    /// <summary>
    /// Gets the guild description, when applicable.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; internal set; }

    /// <summary>
    /// Gets this guild's banner hash, when applicable.
    /// </summary>
    [JsonProperty("banner")]
    public string Banner { get; internal set; }

    /// <summary>
    /// Gets this guild's banner in url form.
    /// </summary>
    [JsonIgnore]
    public string BannerUrl
        => !string.IsNullOrWhiteSpace(Banner) ? $"https://cdn.discordapp.com/banners/{Id}/{Banner}" : null;

    /// <summary>
    /// Gets this guild's premium tier (Nitro boosting).
    /// </summary>
    [JsonProperty("premium_tier")]
    public PremiumTier PremiumTier { get; internal set; }

    /// <summary>
    /// Gets the amount of members that boosted this guild.
    /// </summary>
    [JsonProperty("premium_subscription_count", NullValueHandling = NullValueHandling.Ignore)]
    public int? PremiumSubscriptionCount { get; internal set; }

    /// <summary>
    /// Whether the guild has the boost progress bar enabled.
    /// </summary>
    [JsonProperty("premium_progress_bar_enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool PremiumProgressBarEnabled { get; internal set; }

    /// <summary>
    /// Gets whether this guild is designated as NSFW.
    /// </summary>
    [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsNSFW { get; internal set; }

    /// <summary>
    /// Gets the stage instances in this guild.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordStageInstance> StageInstances => new ReadOnlyConcurrentDictionary<ulong, DiscordStageInstance>(_stageInstances);

    [JsonProperty("stage_instances", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordStageInstance> _stageInstances;

    // Seriously discord?

    // I need to work on this
    //
    // /// <summary>
    // /// Gets channels ordered in a manner in which they'd be ordered in the UI of the discord client.
    // /// </summary>
    // [JsonIgnore]
    // public IEnumerable<DiscordChannel> OrderedChannels
    //    => this._channels.OrderBy(xc => xc.Parent?.Position).ThenBy(xc => xc.Type).ThenBy(xc => xc.Position);

    [JsonIgnore]
    internal bool _isSynced { get; set; }

    internal DiscordGuild()
    {
        _current_member_lazy = new Lazy<DiscordMember>(() => (_members != null && _members.TryGetValue(Discord.CurrentUser.Id, out DiscordMember? member)) ? member : null);
        _invites = new ConcurrentDictionary<string, DiscordInvite>();
    }

    #region Guild Methods

    /// <summary>
    /// Gets guild's icon URL, in requested format and size.
    /// </summary>
    /// <param name="imageFormat">The image format of the icon to get.</param>
    /// <param name="imageSize">The maximum size of the icon. Must be a power of two, minimum 16, maximum 4096.</param>
    /// <returns>The URL of the guild's icon.</returns>
    public string GetIconUrl(ImageFormat imageFormat, ushort imageSize = 1024)
    {

        if (string.IsNullOrWhiteSpace(IconHash))
        {
            return null;
        }

        if (imageFormat == ImageFormat.Unknown)
        {
            throw new ArgumentException("You must specify valid image format.", nameof(imageFormat));
        }

        // Makes sure the image size is in between Discord's allowed range.
        if (imageSize < 16 || imageSize > 4096)
        {
            throw new ArgumentOutOfRangeException(nameof(imageSize), imageSize, "Image Size is not in between 16 and 4096.");
        }

        // Checks to see if the image size is not a power of two.
        if (!(imageSize is not 0 && (imageSize & (imageSize - 1)) is 0))
        {
            throw new ArgumentOutOfRangeException(nameof(imageSize), imageSize, "Image size is not a power of two.");
        }

        // Get the string variants of the method parameters to use in the urls.
        string stringImageFormat = imageFormat switch
        {
            ImageFormat.Gif => "gif",
            ImageFormat.Jpeg => "jpg",
            ImageFormat.Png => "png",
            ImageFormat.WebP => "webp",
            ImageFormat.Auto => !string.IsNullOrWhiteSpace(IconHash) ? (IconHash.StartsWith("a_") ? "gif" : "png") : "png",
            _ => throw new ArgumentOutOfRangeException(nameof(imageFormat)),
        };
        string stringImageSize = imageSize.ToString(CultureInfo.InvariantCulture);

        return $"https://cdn.discordapp.com{Endpoints.ICONS}/{Id}/{IconHash}.{stringImageFormat}?size={stringImageSize}";

    }

    /// <summary>
    /// Creates a new scheduled event in this guild.
    /// </summary>
    /// <param name="name">The name of the event to create, up to 100 characters.</param>
    /// <param name="description">The description of the event, up to 1000 characters.</param>
    /// <param name="channelId">If a <see cref="ScheduledGuildEventType.StageInstance"/> or <see cref="ScheduledGuildEventType.VoiceChannel"/>, the id of the channel the event will be hosted in</param>
    /// <param name="type">The type of the event. <see paramref="channelId"/> must be supplied if not an external event.</param>
    /// <param name="privacyLevel">The privacy level of thi</param>
    /// <param name="start">When this event starts. Must be in the future, and before the end date.</param>
    /// <param name="end">When this event ends. If supplied, must be in the future and after the end date. This is requred for <see cref="ScheduledGuildEventType.External"/>.</param>
    /// <param name="location">Where this event takes place, up to 100 characters. Only applicable if the type is <see cref="ScheduledGuildEventType.External"/></param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns>The created event.</returns>
    public Task<DiscordScheduledGuildEvent> CreateEventAsync(string name, string description, ulong? channelId, ScheduledGuildEventType type, ScheduledGuildEventPrivacyLevel privacyLevel, DateTimeOffset start, DateTimeOffset? end, string location = null, string reason = null)
    {
        if (start <= DateTimeOffset.Now)
        {
            throw new ArgumentOutOfRangeException("The start time for an event must be in the future.");
        }

        if (end != null && end <= start)
        {
            throw new ArgumentOutOfRangeException("The end time for an event must be after the start time.");
        }

        DiscordScheduledGuildEventMetadata metadata = null;
        switch (type)
        {
            case ScheduledGuildEventType.StageInstance or ScheduledGuildEventType.VoiceChannel when channelId == null:
                throw new ArgumentException($"{nameof(channelId)} must not be null when type is {type}", nameof(channelId));
            case ScheduledGuildEventType.External when channelId != null:
                throw new ArgumentException($"{nameof(channelId)} must be null when using external event type", nameof(channelId));
            case ScheduledGuildEventType.External when location == null:
                throw new ArgumentException($"{nameof(location)} must not be null when using external event type", nameof(location));
            case ScheduledGuildEventType.External when end == null:
                throw new ArgumentException($"{nameof(end)} must not be null when using external event type", nameof(end));
        }
        if (!string.IsNullOrEmpty(location))
        {
            metadata = new DiscordScheduledGuildEventMetadata()
            {
                Location = location
            };
        }

        return Discord.ApiClient.CreateScheduledGuildEventAsync(Id, name, description, channelId, start, end, type, privacyLevel, metadata, reason);
    }

    /// <summary>
    /// Starts a scheduled event in this guild.
    /// </summary>
    /// <param name="guildEvent">The event to cancel.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Task StartEventAsync(DiscordScheduledGuildEvent guildEvent)
    {
        return guildEvent.Status is not ScheduledGuildEventStatus.Scheduled
            ? throw new InvalidOperationException("The event must be scheduled for it to be started.")
            : ModifyEventAsync(guildEvent, m => m.Status = ScheduledGuildEventStatus.Active);
    }

    /// <summary>
    /// Cancels an event. The event must be scheduled for it to be cancelled.
    /// </summary>
    /// <param name="guildEvent">The event to delete.</param>
    public Task CancelEventAsync(DiscordScheduledGuildEvent guildEvent)
    {
        return guildEvent.Status is not ScheduledGuildEventStatus.Scheduled
            ? throw new InvalidOperationException("The event must be scheduled for it to be cancelled.")
            : ModifyEventAsync(guildEvent, m => m.Status = ScheduledGuildEventStatus.Cancelled);
    }

    /// <summary>
    /// Modifies an existing scheduled event in this guild.
    /// </summary>
    /// <param name="guildEvent">The event to modify.</param>
    /// <param name="mdl">The action to perform on this event</param>
    /// <param name="reason">The reason this event is being modified</param>
    /// <returns>The modified object</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task ModifyEventAsync(DiscordScheduledGuildEvent guildEvent, Action<ScheduledGuildEventEditModel> mdl, string reason = null)
    {
        ScheduledGuildEventEditModel model = new ScheduledGuildEventEditModel();
        mdl(model);

        if (model.Type.HasValue && model.Type.Value is not ScheduledGuildEventType.External)
        {
            if (!model.Channel.HasValue)
            {
                throw new ArgumentException("Channel must be supplied if the event is a stage instance or voice channel event.");
            }

            if (model.Type.Value is ScheduledGuildEventType.StageInstance && model.Channel.Value.Type is not ChannelType.Stage)
            {
                throw new ArgumentException("Channel must be a stage channel if the event is a stage instance event.");
            }

            if (model.Type.Value is ScheduledGuildEventType.VoiceChannel && model.Channel.Value.Type is not ChannelType.Voice)
            {
                throw new ArgumentException("Channel must be a voice channel if the event is a voice channel event.");
            }

            if (model.EndTime.HasValue && model.EndTime.Value < guildEvent.StartTime)
            {
                throw new ArgumentException("End time must be after the start time.");
            }
        }

        if (model.Type.HasValue && model.Type.Value is ScheduledGuildEventType.External)
        {
            if (!model.EndTime.HasValue)
            {
                throw new ArgumentException("End must be supplied if the event is an external event.");
            }

            if (!model.Metadata.HasValue || string.IsNullOrEmpty(model.Metadata.Value.Location))
            {
                throw new ArgumentException("Location must be supplied if the event is an external event.");
            }

            if (model.Channel.HasValue && model.Channel.Value != null)
            {
                throw new ArgumentException("Channel must not be supplied if the event is an external event.");
            }
        }

        if (guildEvent.Status is ScheduledGuildEventStatus.Completed)
        {
            throw new ArgumentException("The event must not be completed for it to be modified.");
        }

        if (guildEvent.Status is ScheduledGuildEventStatus.Cancelled)
        {
            throw new ArgumentException("The event must not be cancelled for it to be modified.");
        }

        if (model.Status.HasValue)
        {
            switch (model.Status.Value)
            {
                case ScheduledGuildEventStatus.Scheduled:
                    throw new ArgumentException("Status must not be set to scheduled.");
                case ScheduledGuildEventStatus.Active when guildEvent.Status is not ScheduledGuildEventStatus.Scheduled:
                    throw new ArgumentException("Event status must be scheduled to progress to active.");
                case ScheduledGuildEventStatus.Completed when guildEvent.Status is not ScheduledGuildEventStatus.Active:
                    throw new ArgumentException("Event status must be active to progress to completed.");
                case ScheduledGuildEventStatus.Cancelled when guildEvent.Status is not ScheduledGuildEventStatus.Scheduled:
                    throw new ArgumentException("Event status must be scheduled to progress to cancelled.");
            }
        }

        DiscordScheduledGuildEvent modifiedEvent = await Discord.ApiClient.ModifyScheduledGuildEventAsync(
            Id, guildEvent.Id,
            model.Name, model.Description,
            model.Channel.IfPresent(c => c?.Id),
            model.StartTime, model.EndTime,
            model.Type, model.PrivacyLevel,
            model.Metadata, model.Status, reason).ConfigureAwait(false);

        _scheduledEvents[modifiedEvent.Id] = modifiedEvent;
        return;
    }

    /// <summary>
    /// Deletes an exising scheduled event in this guild.
    /// </summary>
    /// <param name="guildEvent"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public Task DeleteEventAsync(DiscordScheduledGuildEvent guildEvent, string reason = null)
    {
        _scheduledEvents.TryRemove(guildEvent.Id, out _);
        return Discord.ApiClient.DeleteScheduledGuildEventAsync(Id, guildEvent.Id);
    }

    /// <summary>
    /// Gets the currently active or scheduled events in this guild.
    /// </summary>
    /// <param name="withUserCounts">Whether to include number of users subscribed to each event</param>
    /// <returns>The active and scheduled events on the server, if any.</returns>
    public async Task<IReadOnlyList<DiscordScheduledGuildEvent>> GetEventsAsync(bool withUserCounts = false)
    {
        IReadOnlyList<DiscordScheduledGuildEvent> events = await Discord.ApiClient.GetScheduledGuildEventsAsync(Id, withUserCounts);

        foreach (DiscordScheduledGuildEvent @event in events)
        {
            _scheduledEvents[@event.Id] = @event;
        }

        return events;
    }

    /// <summary>
    /// Gets a list of users who are interested in this event.
    /// </summary>
    /// <param name="guildEvent">The event to query users from</param>
    /// <param name="limit">How many users to fetch.</param>
    /// <param name="after">Fetch users after this id. Mutually exclusive with before</param>
    /// <param name="before">Fetch users before this id. Mutually exclusive with after</param>
    public async Task<IReadOnlyList<DiscordUser>> GetEventUsersAsync(DiscordScheduledGuildEvent guildEvent, int limit = 100, ulong? after = null, ulong? before = null)
    {
        int remaining = limit;
        ulong? last = null;
        bool isAfter = after != null;

        List<DiscordUser> users = new List<DiscordUser>();

        int lastCount;
        do
        {
            int fetchSize = remaining > 100 ? 100 : remaining;
            IReadOnlyList<DiscordUser> fetch = await Discord.ApiClient.GetScheduledGuildEventUsersAsync(Id, guildEvent.Id, true, fetchSize, !isAfter ? last ?? before : null, isAfter ? last ?? after : null);

            lastCount = fetch.Count;
            remaining -= lastCount;

            if (!isAfter)
            {
                users.AddRange(fetch);
                last = fetch.LastOrDefault()?.Id;
            }
            else
            {
                users.InsertRange(0, fetch);
                last = fetch.FirstOrDefault()?.Id;
            }
        }
        while (remaining > 0 && lastCount > 0);


        return users.AsReadOnly();
    }

    /// <summary>
    /// Searches the current guild for members who's display name start with the specified name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="limit">The maximum amount of members to return. Max 1000. Defaults to 1.</param>
    /// <returns>The members found, if any.</returns>
    public Task<IReadOnlyList<DiscordMember>> SearchMembersAsync(string name, int? limit = 1)
        => Discord.ApiClient.SearchMembersAsync(Id, name, limit);

    /// <summary>
    /// Adds a new member to this guild
    /// </summary>
    /// <param name="user">User to add</param>
    /// <param name="access_token">User's access token (OAuth2)</param>
    /// <param name="nickname">new nickname</param>
    /// <param name="roles">new roles</param>
    /// <param name="muted">whether this user has to be muted</param>
    /// <param name="deaf">whether this user has to be deafened</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.CreateInstantInvite" /> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the <paramref name="user"/> or <paramref name="access_token"/> is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task AddMemberAsync(DiscordUser user, string access_token, string nickname = null, IEnumerable<DiscordRole> roles = null,
        bool muted = false, bool deaf = false)
        => Discord.ApiClient.AddGuildMemberAsync(Id, user.Id, access_token, nickname, roles, muted, deaf);

    /// <summary>
    /// Deletes this guild. Requires the caller to be the owner of the guild.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client is not the owner of the guild.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteAsync()
        => Discord.ApiClient.DeleteGuildAsync(Id);

    /// <summary>
    /// Modifies this guild.
    /// </summary>
    /// <param name="action">Action to perform on this guild..</param>
    /// <returns>The modified guild object.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuild> ModifyAsync(Action<GuildEditModel> action)
    {
        GuildEditModel mdl = new GuildEditModel();
        action(mdl);

        if (mdl.AfkChannel.HasValue && mdl.AfkChannel.Value.Type != ChannelType.Voice)
        {
            throw new ArgumentException("AFK channel needs to be a voice channel.");
        }

        Optional<string> iconb64 = Optional.FromNoValue<string>();

        if (mdl.Icon.HasValue && mdl.Icon.Value != null)
        {
            using (ImageTool imgtool = new ImageTool(mdl.Icon.Value))
            {
                iconb64 = imgtool.GetBase64();
            }
        }
        else if (mdl.Icon.HasValue)
        {
            iconb64 = null;
        }

        Optional<string> splashb64 = Optional.FromNoValue<string>();

        if (mdl.Splash.HasValue && mdl.Splash.Value != null)
        {
            using (ImageTool imgtool = new ImageTool(mdl.Splash.Value))
            {
                splashb64 = imgtool.GetBase64();
            }
        }
        else if (mdl.Splash.HasValue)
        {
            splashb64 = null;
        }

        Optional<string> bannerb64 = Optional.FromNoValue<string>();

        if (mdl.Banner.HasValue)
        {
            if (mdl.Banner.Value == null)
            {
                bannerb64 = null;
            }
            else
            {
                using (ImageTool imgtool = new ImageTool(mdl.Banner.Value))
                {
                    bannerb64 = imgtool.GetBase64();
                }
            }
        }

        return await Discord.ApiClient.ModifyGuildAsync(Id, mdl.Name, mdl.Region.IfPresent(e => e.Id),
            mdl.VerificationLevel, mdl.DefaultMessageNotifications, mdl.MfaLevel, mdl.ExplicitContentFilter,
            mdl.AfkChannel.IfPresent(e => e?.Id), mdl.AfkTimeout, iconb64, mdl.Owner.IfPresent(e => e.Id), splashb64,
            mdl.SystemChannel.IfPresent(e => e?.Id), bannerb64,
            mdl.Description, mdl.DiscoverySplash, mdl.Features, mdl.PreferredLocale,
            mdl.PublicUpdatesChannel.IfPresent(e => e?.Id), mdl.RulesChannel.IfPresent(e => e?.Id),
            mdl.SystemChannelFlags, mdl.AuditLogReason).ConfigureAwait(false);
    }

    /// <summary>
    /// Batch modifies the role order in the guild.
    /// </summary>
    /// <param name="roles">A dictionary of guild roles indexed by their new role positions.</param>
    /// <param name="reason">An optional Audit log reason on why this action was done.</param>
    /// <returns>A list of all the current guild roles ordered in their new role positions.</returns>
    public async Task<IReadOnlyList<DiscordRole>> ModifyRolePositionsAsync(IDictionary<int, DiscordRole> roles, string reason = null)
    {
        if (roles.Count == 0)
        {
            throw new ArgumentException("Roles cannot be empty.", nameof(roles));
        }

        // Sort the roles by position and create skeleton roles for the payload.
        DiscordRole[] returnedRoles = await Discord.ApiClient.ModifyGuildRolePositionsAsync(Id, roles.Select(x => new RestGuildRoleReorderPayload() { RoleId = x.Value.Id, Position = x.Key }), reason).ConfigureAwait(false);

        // Update the cache as the endpoint returns all roles in the order they were sent.
        _roles = new(returnedRoles.Select(x => new KeyValuePair<ulong, DiscordRole>(x.Id, x)));
        return returnedRoles;
    }

    /// <summary>
    /// Bans a specified member from this guild.
    /// </summary>
    /// <param name="member">Member to ban.</param>
    /// <param name="delete_message_days">How many days to remove messages from.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task BanMemberAsync(DiscordMember member, int delete_message_days = 0, string reason = null)
        => Discord.ApiClient.CreateGuildBanAsync(Id, member.Id, delete_message_days, reason);

    /// <summary>
    /// Bans a specified user by ID. This doesn't require the user to be in this guild.
    /// </summary>
    /// <param name="user_id">ID of the user to ban.</param>
    /// <param name="delete_message_days">How many days to remove messages from.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task BanMemberAsync(ulong user_id, int delete_message_days = 0, string reason = null)
        => Discord.ApiClient.CreateGuildBanAsync(Id, user_id, delete_message_days, reason);

    /// <summary>
    /// Unbans a user from this guild.
    /// </summary>
    /// <param name="user">User to unban.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task UnbanMemberAsync(DiscordUser user, string reason = null)
        => Discord.ApiClient.RemoveGuildBanAsync(Id, user.Id, reason);

    /// <summary>
    /// Unbans a user by ID.
    /// </summary>
    /// <param name="user_id">ID of the user to unban.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task UnbanMemberAsync(ulong user_id, string reason = null)
        => Discord.ApiClient.RemoveGuildBanAsync(Id, user_id, reason);

    /// <summary>
    /// Leaves this guild.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task LeaveAsync()
        => Discord.ApiClient.LeaveGuildAsync(Id);

    /// <summary>
    /// Gets the bans for this guild.
    /// </summary>
    /// <param name="limit">The number of users to return (up to maximum 1000, default 1000).</param>
    /// <param name="before">Consider only users before the given user id.</param>
    /// <param name="after">Consider only users after the given user id.</param>
    /// <returns>Collection of bans in this guild.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordBan>> GetBansAsync(int? limit = null, ulong? before = null, ulong? after = null)
        => Discord.ApiClient.GetGuildBansAsync(Id, limit, before, after);

    /// <summary>
    /// Gets a ban for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the ban for.</param>
    /// <exception cref="NotFoundException">Thrown when the specified user is not banned.</exception>
    /// <returns>The requested ban object.</returns>
    public Task<DiscordBan> GetBanAsync(ulong userId)
        => Discord.ApiClient.GetGuildBanAsync(Id, userId);

    /// <summary>
    /// Gets a ban for a specific user.
    /// </summary>
    /// <param name="user">The user to get the ban for.</param>
    /// <exception cref="NotFoundException">Thrown when the specified user is not banned.</exception>
    /// <returns>The requested ban object.</returns>
    public Task<DiscordBan> GetBanAsync(DiscordUser user)
        => GetBanAsync(user.Id);

    /// <summary>
    /// Creates a new text channel in this guild.
    /// </summary>
    /// <param name="name">Name of the new channel.</param>
    /// <param name="parent">Category to put this channel in.</param>
    /// <param name="topic">Topic of the channel.</param>
    /// <param name="overwrites">Permission overwrites for this channel.</param>
    /// <param name="nsfw">Whether the channel is to be flagged as not safe for work.</param>
    /// <param name="position">Sorting position of the channel.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
    /// <returns>The newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordChannel> CreateTextChannelAsync(string name, DiscordChannel parent = null, Optional<string> topic = default, IEnumerable<DiscordOverwriteBuilder> overwrites = null, bool? nsfw = null, Optional<int?> perUserRateLimit = default, int? position = null, string reason = null)
        => CreateChannelAsync(name, ChannelType.Text, parent, topic, null, null, overwrites, nsfw, perUserRateLimit, null, position, reason);

    /// <summary>
    /// Creates a new channel category in this guild.
    /// </summary>
    /// <param name="name">Name of the new category.</param>
    /// <param name="overwrites">Permission overwrites for this category.</param>
    /// <param name="position">Sorting position of the channel.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The newly-created channel category.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordChannel> CreateChannelCategoryAsync(string name, IEnumerable<DiscordOverwriteBuilder> overwrites = null, int? position = null, string reason = null)
        => CreateChannelAsync(name, ChannelType.Category, null, Optional.FromNoValue<string>(), null, null, overwrites, null, Optional.FromNoValue<int?>(), null, position, reason);

    /// <summary>
    /// Creates a new voice channel in this guild.
    /// </summary>
    /// <param name="name">Name of the new channel.</param>
    /// <param name="parent">Category to put this channel in.</param>
    /// <param name="bitrate">Bitrate of the channel.</param>
    /// <param name="user_limit">Maximum number of users in the channel.</param>
    /// <param name="overwrites">Permission overwrites for this channel.</param>
    /// <param name="qualityMode">Video quality mode of the channel.</param>
    /// <param name="position">Sorting position of the channel.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordChannel> CreateVoiceChannelAsync(string name, DiscordChannel parent = null, int? bitrate = null, int? user_limit = null, IEnumerable<DiscordOverwriteBuilder> overwrites = null, VideoQualityMode? qualityMode = null, int? position = null, string reason = null)
        => CreateChannelAsync(name, ChannelType.Voice, parent, Optional.FromNoValue<string>(), bitrate, user_limit, overwrites, null, Optional.FromNoValue<int?>(), qualityMode, position, reason);

    /// <summary>
    /// Creates a new channel in this guild.
    /// </summary>
    /// <param name="name">Name of the new channel.</param>
    /// <param name="type">Type of the new channel.</param>
    /// <param name="parent">Category to put this channel in.</param>
    /// <param name="topic">Topic of the channel.</param>
    /// <param name="bitrate">Bitrate of the channel. Applies to voice only.</param>
    /// <param name="userLimit">Maximum number of users in the channel. Applies to voice only.</param>
    /// <param name="overwrites">Permission overwrites for this channel.</param>
    /// <param name="nsfw">Whether the channel is to be flagged as not safe for work. Applies to text only.</param>
    /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
    /// <param name="qualityMode">Video quality mode of the channel. Applies to voice only.</param>
    /// <param name="position">Sorting position of the channel.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordChannel> CreateChannelAsync(string name, ChannelType type, DiscordChannel parent = null, Optional<string> topic = default, int? bitrate = null, int? userLimit = null, IEnumerable<DiscordOverwriteBuilder> overwrites = null, bool? nsfw = null, Optional<int?> perUserRateLimit = default, VideoQualityMode? qualityMode = null, int? position = null, string reason = null)
    {
        // technically you can create news/store channels but not always
        return type != ChannelType.Text && type != ChannelType.Voice && type != ChannelType.Category && type != ChannelType.News && type != ChannelType.Stage
            ? throw new ArgumentException("Channel type must be text, voice, stage, or category.", nameof(type))
            : type == ChannelType.Category && parent != null
            ? throw new ArgumentException("Cannot specify parent of a channel category.", nameof(parent))
            : Discord.ApiClient.CreateGuildChannelAsync(Id, name, type, parent?.Id, topic, bitrate, userLimit, overwrites, nsfw, perUserRateLimit, qualityMode, position, reason);
    }

    // this is to commemorate the Great DAPI Channel Massacre of 2017-11-19.
    /// <summary>
    /// <para>Deletes all channels in this guild.</para>
    /// <para>Note that this is irreversible. Use carefully!</para>
    /// </summary>
    /// <returns></returns>
    public Task DeleteAllChannelsAsync()
    {
        IEnumerable<Task> tasks = Channels.Values.Select(xc => xc.DeleteAsync());
        return Task.WhenAll(tasks);
    }

    /// <summary>
    /// Estimates the number of users to be pruned.
    /// </summary>
    /// <param name="days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
    /// <param name="includedRoles">The roles to be included in the prune.</param>
    /// <returns>Number of users that will be pruned.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.KickMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<int> GetPruneCountAsync(int days = 7, IEnumerable<DiscordRole> includedRoles = null)
    {
        if (includedRoles != null)
        {
            includedRoles = includedRoles.Where(r => r != null);
            int roleCount = includedRoles.Count();
            DiscordRole[] roleArr = includedRoles.ToArray();
            List<ulong> rawRoleIds = new List<ulong>();

            for (int i = 0; i < roleCount; i++)
            {
                if (_roles.ContainsKey(roleArr[i].Id))
                {
                    rawRoleIds.Add(roleArr[i].Id);
                }
            }

            return Discord.ApiClient.GetGuildPruneCountAsync(Id, days, rawRoleIds);
        }

        return Discord.ApiClient.GetGuildPruneCountAsync(Id, days, null);
    }

    /// <summary>
    /// Prunes inactive users from this guild.
    /// </summary>
    /// <param name="days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
    /// <param name="computePruneCount">Whether to return the prune count after this method completes. This is discouraged for larger guilds.</param>
    /// <param name="includedRoles">The roles to be included in the prune.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>Number of users pruned.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<int?> PruneAsync(int days = 7, bool computePruneCount = true, IEnumerable<DiscordRole> includedRoles = null, string reason = null)
    {
        if (includedRoles != null)
        {
            includedRoles = includedRoles.Where(r => r != null);
            int roleCount = includedRoles.Count();
            DiscordRole[] roleArr = includedRoles.ToArray();
            List<ulong> rawRoleIds = new List<ulong>();

            for (int i = 0; i < roleCount; i++)
            {
                if (_roles.ContainsKey(roleArr[i].Id))
                {
                    rawRoleIds.Add(roleArr[i].Id);
                }
            }

            return Discord.ApiClient.BeginGuildPruneAsync(Id, days, computePruneCount, rawRoleIds, reason);
        }

        return Discord.ApiClient.BeginGuildPruneAsync(Id, days, computePruneCount, null, reason);
    }

    /// <summary>
    /// Gets integrations attached to this guild.
    /// </summary>
    /// <returns>Collection of integrations attached to this guild.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordIntegration>> GetIntegrationsAsync()
        => Discord.ApiClient.GetGuildIntegrationsAsync(Id);

    /// <summary>
    /// Attaches an integration from current user to this guild.
    /// </summary>
    /// <param name="integration">Integration to attach.</param>
    /// <returns>The integration after being attached to the guild.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordIntegration> AttachUserIntegrationAsync(DiscordIntegration integration)
        => Discord.ApiClient.CreateGuildIntegrationAsync(Id, integration.Type, integration.Id);

    /// <summary>
    /// Modifies an integration in this guild.
    /// </summary>
    /// <param name="integration">Integration to modify.</param>
    /// <param name="expire_behaviour">Number of days after which the integration expires.</param>
    /// <param name="expire_grace_period">Length of grace period which allows for renewing the integration.</param>
    /// <param name="enable_emoticons">Whether emotes should be synced from this integration.</param>
    /// <returns>The modified integration.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordIntegration> ModifyIntegrationAsync(DiscordIntegration integration, int expire_behaviour, int expire_grace_period, bool enable_emoticons)
        => Discord.ApiClient.ModifyGuildIntegrationAsync(Id, integration.Id, expire_behaviour, expire_grace_period, enable_emoticons);

    /// <summary>
    /// Removes an integration from this guild.
    /// </summary>
    /// <param name="integration">Integration to remove.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteIntegrationAsync(DiscordIntegration integration, string reason = null)
        => Discord.ApiClient.DeleteGuildIntegrationAsync(Id, integration, reason);

    /// <summary>
    /// Forces re-synchronization of an integration for this guild.
    /// </summary>
    /// <param name="integration">Integration to synchronize.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task SyncIntegrationAsync(DiscordIntegration integration)
        => Discord.ApiClient.SyncGuildIntegrationAsync(Id, integration.Id);

    /// <summary>
    /// Gets the voice regions for this guild.
    /// </summary>
    /// <returns>Voice regions available for this guild.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
    {
        IReadOnlyList<DiscordVoiceRegion> vrs = await Discord.ApiClient.GetGuildVoiceRegionsAsync(Id).ConfigureAwait(false);
        foreach (DiscordVoiceRegion xvr in vrs)
        {
            Discord.InternalVoiceRegions.TryAdd(xvr.Id, xvr);
        }

        return vrs;
    }

    /// <summary>
    /// Gets the active and private threads for this guild.
    /// </summary>
    /// <returns>A list of all the active and private threads the user can access in the server.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<ThreadQueryResult> ListActiveThreadsAsync()
    {
        ThreadQueryResult threads = await Discord.ApiClient.ListActiveThreadsAsync(Id);
        // Gateway handles thread cache (if it does it properly
        /*foreach (var thread in threads)
            this._threads[thread.Id] = thread;*/
        return threads;
    }

    /// <summary>
    /// Gets an invite from this guild from an invite code.
    /// </summary>
    /// <param name="code">The invite code</param>
    /// <returns>An invite, or null if not in cache.</returns>
    public DiscordInvite GetInvite(string code)
        => _invites.TryGetValue(code, out DiscordInvite? invite) ? invite : null;

    /// <summary>
    /// Gets all the invites created for all the channels in this guild.
    /// </summary>
    /// <returns>A collection of invites.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordInvite>> GetInvitesAsync()
    {
        IReadOnlyList<DiscordInvite> res = await Discord.ApiClient.GetGuildInvitesAsync(Id).ConfigureAwait(false);

        DiscordIntents intents = Discord.Configuration.Intents;

        if (!intents.HasIntent(DiscordIntents.GuildInvites))
        {
            for (int i = 0; i < res.Count; i++)
            {
                _invites[res[i].Code] = res[i];
            }
        }

        return res;
    }

    /// <summary>
    /// Gets the vanity invite for this guild.
    /// </summary>
    /// <returns>A partial vanity invite.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordInvite> GetVanityInviteAsync()
        => Discord.ApiClient.GetGuildVanityUrlAsync(Id);


    /// <summary>
    /// Gets all the webhooks created for all the channels in this guild.
    /// </summary>
    /// <returns>A collection of webhooks this guild has.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordWebhook>> GetWebhooksAsync()
        => Discord.ApiClient.GetGuildWebhooksAsync(Id);

    /// <summary>
    /// Gets this guild's widget image.
    /// </summary>
    /// <param name="bannerType">The format of the widget.</param>
    /// <returns>The URL of the widget image.</returns>
    public string GetWidgetImage(WidgetType bannerType = WidgetType.Shield)
    {
        string param = bannerType switch
        {
            WidgetType.Banner1 => "banner1",
            WidgetType.Banner2 => "banner2",
            WidgetType.Banner3 => "banner3",
            WidgetType.Banner4 => "banner4",
            _ => "shield",
        };
        return $"{Net.Endpoints.BASE_URI}{Net.Endpoints.GUILDS}/{Id}{Net.Endpoints.WIDGET_PNG}?style={param}";
    }

    /// <summary>
    /// Gets a member of this guild by their user ID.
    /// </summary>
    /// <param name="userId">ID of the member to get.</param>
    /// <param name="updateCache">Whether to always make a REST request and update the member cache.</param>
    /// <returns>The requested member.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMember> GetMemberAsync(ulong userId, bool updateCache = false)
    {
        if (!updateCache && _members != null && _members.TryGetValue(userId, out DiscordMember? mbr))
        {
            return mbr;
        }

        mbr = await Discord.ApiClient.GetGuildMemberAsync(Id, userId).ConfigureAwait(false);

        DiscordIntents intents = Discord.Configuration.Intents;

        if (intents.HasIntent(DiscordIntents.GuildMembers))
        {
            if (_members != null)
            {
                _members[userId] = mbr;
            }
        }

        return mbr;
    }

    /// <summary>
    /// Retrieves a full list of members from Discord. This method will bypass cache.
    /// </summary>
    /// <returns>A collection of all members in this guild.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordMember>> GetAllMembersAsync()
    {
        List<DiscordMember> recmbr = new();

        int recd = 1000;
        ulong last = 0ul;
        while (recd > 0)
        {
            IReadOnlyList<TransportMember> tms = await Discord.ApiClient.ListGuildMembersAsync(Id, 1000, last == 0 ? null : (ulong?)last).ConfigureAwait(false);
            recd = tms.Count;

            foreach (TransportMember xtm in tms)
            {
                DiscordUser usr = new DiscordUser(xtm.User) { Discord = Discord };

                usr = Discord.UpdateUserCache(usr);

                recmbr.Add(new DiscordMember(xtm) { Discord = Discord, _guild_id = Id });
            }

            TransportMember? tm = tms.LastOrDefault();
            last = tm?.User.Id ?? 0;
        }

        return recmbr.AsReadOnly();
    }

    /// <summary>
    /// Requests that Discord send a list of guild members based on the specified arguments. This method will fire the <see cref="DiscordClient.GuildMembersChunked"/> event.
    /// <para>If no arguments aside from <paramref name="presences"/> and <paramref name="nonce"/> are specified, this will request all guild members.</para>
    /// </summary>
    /// <param name="query">Filters the returned members based on what the username starts with. Either this or <paramref name="userIds"/> must not be null.
    /// The <paramref name="limit"/> must also be greater than 0 if this is specified.</param>
    /// <param name="limit">Total number of members to request. This must be greater than 0 if <paramref name="query"/> is specified.</param>
    /// <param name="presences">Whether to include the <see cref="GuildMembersChunkEventArgs.Presences"/> associated with the fetched members.</param>
    /// <param name="userIds">Whether to limit the request to the specified user ids. Either this or <paramref name="query"/> must not be null.</param>
    /// <param name="nonce">The unique string to identify the response.</param>
    public async Task RequestMembersAsync(string query = "", int limit = 0, bool? presences = null, IEnumerable<ulong> userIds = null, string nonce = null)
    {
        if (Discord is not DiscordClient client)
        {
            throw new InvalidOperationException("This operation is only valid for regular Discord clients.");
        }

        if (query == null && userIds == null)
        {
            throw new ArgumentException("The query and user IDs cannot both be null.");
        }

        if (query != null && userIds != null)
        {
            query = null;
        }

        GatewayRequestGuildMembers grgm = new GatewayRequestGuildMembers(this)
        {
            Query = query,
            Limit = limit >= 0 ? limit : 0,
            Presences = presences,
            UserIds = userIds,
            Nonce = nonce
        };

        GatewayPayload payload = new GatewayPayload
        {
            OpCode = GatewayOpCode.RequestGuildMembers,
            Data = grgm
        };

        string payloadStr = JsonConvert.SerializeObject(payload, Formatting.None);
        await client.WsSendAsync(payloadStr).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all the channels this guild has.
    /// </summary>
    /// <returns>A collection of this guild's channels.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordChannel>> GetChannelsAsync()
        => Discord.ApiClient.GetGuildChannelsAsync(Id);

    /// <summary>
    /// Creates a new role in this guild.
    /// </summary>
    /// <param name="name">Name of the role.</param>
    /// <param name="permissions">Permissions for the role.</param>
    /// <param name="color">Color for the role.</param>
    /// <param name="hoist">Whether the role is to be hoisted.</param>
    /// <param name="mentionable">Whether the role is to be mentionable.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <param name="icon">The icon to add to this role</param>
    /// <param name="emoji">The emoji to add to this role. Must be unicode.</param>
    /// <returns>The newly-created role.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordRole> CreateRoleAsync(string name = null, Permissions? permissions = null, DiscordColor? color = null, bool? hoist = null, bool? mentionable = null, string reason = null, Stream icon = null, DiscordEmoji emoji = null)
        => Discord.ApiClient.CreateGuildRoleAsync(Id, name, permissions, color?.Value, hoist, mentionable, reason, icon, emoji?.ToString());
    /// <summary>
    /// Gets a role from this guild by its ID.
    /// </summary>
    /// <param name="id">ID of the role to get.</param>
    /// <returns>Requested role.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public DiscordRole GetRole(ulong id)
        => _roles.TryGetValue(id, out DiscordRole? role) ? role : null;

    /// <summary>
    /// Gets a channel from this guild by its ID.
    /// </summary>
    /// <param name="id">ID of the channel to get.</param>
    /// <returns>Requested channel.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public DiscordChannel GetChannel(ulong id)
        => (_channels != null && _channels.TryGetValue(id, out DiscordChannel? channel)) ? channel : null;

    /// <summary>
    /// Gets audit log entries for this guild.
    /// </summary>
    /// <param name="limit">Maximum number of entries to fetch.</param>
    /// <param name="by_member">Filter by member responsible.</param>
    /// <param name="action_type">Filter by action type.</param>
    /// <returns>A collection of requested audit log entries.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ViewAuditLog"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordAuditLogEntry>> GetAuditLogsAsync(int? limit = null, DiscordMember by_member = null, AuditLogActionType? action_type = null)
    {
        List<AuditLog> alrs = new List<AuditLog>();
        int ac = 1, tc = 0, rmn = 100;
        ulong last = 0ul;
        while (ac > 0)
        {
            rmn = limit != null ? limit.Value - tc : 100;
            rmn = Math.Min(100, rmn);
            if (rmn <= 0)
            {
                break;
            }

            AuditLog alr = await Discord.ApiClient.GetAuditLogsAsync(Id, rmn, null, last == 0 ? null : (ulong?)last, by_member?.Id, (int?)action_type).ConfigureAwait(false);
            ac = alr.Entries.Count();
            tc += ac;
            if (ac > 0)
            {
                last = alr.Entries.Last().Id;
                alrs.Add(alr);
            }
        }

        IEnumerable<AuditLogUser> amr = alrs.SelectMany(xa => xa.Users)
            .GroupBy(xu => xu.Id)
            .Select(xgu => xgu.First());

        foreach (AuditLogUser? xau in amr)
        {
            if (Discord.UserCache.ContainsKey(xau.Id))
            {
                continue;
            }

            TransportUser xtu = new TransportUser
            {
                Id = xau.Id,
                Username = xau.Username,
                Discriminator = xau.Discriminator,
                AvatarHash = xau.AvatarHash
            };
            DiscordUser xu = new DiscordUser(xtu) { Discord = Discord };
            xu = Discord.UpdateUserCache(xu);
        }

        IEnumerable<AuditLogWebhook> ahr = alrs.SelectMany(xa => xa.Webhooks)
            .GroupBy(xh => xh.Id)
            .Select(xgh => xgh.First());

        IEnumerable<DiscordScheduledGuildEvent> eve = alrs.SelectMany(xr => xr.Events)
            .GroupBy(xa => xa.Id)
            .Select(xu => xu.First());

        IEnumerable<DiscordThreadChannel> thr = alrs.SelectMany(xr => xr.Threads)
            .GroupBy(xa => xa.Id)
            .Select(xu => xu.First());

        Dictionary<ulong, DiscordWebhook> ahd = null;
        if (ahr.Any())
        {
            IReadOnlyList<DiscordWebhook> whr = await GetWebhooksAsync().ConfigureAwait(false);
            Dictionary<ulong, DiscordWebhook> whs = whr.ToDictionary(xh => xh.Id, xh => xh);

            IEnumerable<DiscordWebhook> amh = ahr.Select(xah => whs.TryGetValue(xah.Id, out DiscordWebhook? webhook) ? webhook : new DiscordWebhook { Discord = Discord, Name = xah.Name, Id = xah.Id, AvatarHash = xah.AvatarHash, ChannelId = xah.ChannelId, GuildId = xah.GuildId, Token = xah.Token });
            ahd = amh.ToDictionary(xh => xh.Id, xh => xh);
        }

        Dictionary<ulong, DiscordScheduledGuildEvent> events = null;
        if (eve.Any())
        {
            ConcurrentDictionary<ulong, DiscordScheduledGuildEvent> evb = _scheduledEvents;
            IEnumerable<DiscordScheduledGuildEvent> evf = eve.Select(xa => evb.TryGetValue(xa.Id, out DiscordScheduledGuildEvent? Event) ? Event : new DiscordScheduledGuildEvent { Discord = Discord, Name = xa.Name, Id = xa.Id, ChannelId = xa.ChannelId, GuildId = xa.GuildId, Creator = xa.Creator, Description = xa.Description, EndTime = xa.EndTime, Metadata = xa.Metadata, PrivacyLevel = xa.PrivacyLevel, StartTime = xa.StartTime, Status = xa.Status, Type = xa.Type, UserCount = xa.UserCount });
            events = evf.ToDictionary(xb => xb.Id, xb => xb);
        }

        Dictionary<ulong, DiscordThreadChannel> threads = null;
        if (thr.Any())
        {
            IEnumerable<DiscordThreadChannel> thb = thr.Select(xr => xr ?? new DiscordThreadChannel { Discord = Discord, Id = xr.Id, Name = xr.Name, GuildId = xr.GuildId });
            threads = thb.ToDictionary(xa => xa.Id, xa => xa);
        }

        IEnumerable<DiscordMember> ams = amr.Select(xau => (_members != null && _members.TryGetValue(xau.Id, out DiscordMember? member)) ? member : new DiscordMember { Discord = Discord, Id = xau.Id, _guild_id = Id });
        Dictionary<ulong, DiscordMember> amd = ams.ToDictionary(xm => xm.Id, xm => xm);

        IOrderedEnumerable<AuditLogAction> acs = alrs.SelectMany(xa => xa.Entries).OrderByDescending(xa => xa.Id);
        List<DiscordAuditLogEntry> entries = new List<DiscordAuditLogEntry>();
        foreach (AuditLogAction? xac in acs)
        {
            DiscordAuditLogEntry entry = null;
            ulong t1, t2;
            int t3, t4;
            long t5, t6;
            bool p1, p2;
            switch (xac.ActionType)
            {
                case AuditLogActionType.GuildUpdate:
                    entry = new DiscordAuditLogGuildEntry
                    {
                        Target = this
                    };

                    DiscordAuditLogGuildEntry? entrygld = entry as DiscordAuditLogGuildEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "name":
                                entrygld.NameChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;

                            case "owner_id":
                                entrygld.OwnerChange = new PropertyChange<DiscordMember>
                                {
                                    Before = (_members != null && _members.TryGetValue(xc.OldValueUlong, out DiscordMember? oldMember)) ? oldMember : await GetMemberAsync(xc.OldValueUlong).ConfigureAwait(false),
                                    After = (_members != null && _members.TryGetValue(xc.NewValueUlong, out DiscordMember? newMember)) ? newMember : await GetMemberAsync(xc.NewValueUlong).ConfigureAwait(false)
                                };
                                break;

                            case "icon_hash":
                                entrygld.IconChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString != null ? $"https://cdn.discordapp.com/icons/{Id}/{xc.OldValueString}.webp" : null,
                                    After = xc.OldValueString != null ? $"https://cdn.discordapp.com/icons/{Id}/{xc.NewValueString}.webp" : null
                                };
                                break;

                            case "verification_level":
                                entrygld.VerificationLevelChange = new PropertyChange<VerificationLevel>
                                {
                                    Before = (VerificationLevel)(long)xc.OldValue,
                                    After = (VerificationLevel)(long)xc.NewValue
                                };
                                break;

                            case "afk_channel_id":
                                ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                entrygld.AfkChannelChange = new PropertyChange<DiscordChannel>
                                {
                                    Before = GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id },
                                    After = GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id }
                                };
                                break;

                            case "widget_channel_id":
                                ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                entrygld.EmbedChannelChange = new PropertyChange<DiscordChannel>
                                {
                                    Before = GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id },
                                    After = GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id }
                                };
                                break;

                            case "splash_hash":
                                entrygld.SplashChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString != null ? $"https://cdn.discordapp.com/splashes/{Id}/{xc.OldValueString}.webp?size=2048" : null,
                                    After = xc.NewValueString != null ? $"https://cdn.discordapp.com/splashes/{Id}/{xc.NewValueString}.webp?size=2048" : null
                                };
                                break;

                            case "default_message_notifications":
                                entrygld.NotificationSettingsChange = new PropertyChange<DefaultMessageNotifications>
                                {
                                    Before = (DefaultMessageNotifications)(long)xc.OldValue,
                                    After = (DefaultMessageNotifications)(long)xc.NewValue
                                };
                                break;

                            case "system_channel_id":
                                ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                entrygld.SystemChannelChange = new PropertyChange<DiscordChannel>
                                {
                                    Before = GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id },
                                    After = GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id }
                                };
                                break;

                            case "explicit_content_filter":
                                entrygld.ExplicitContentFilterChange = new PropertyChange<ExplicitContentFilter>
                                {
                                    Before = (ExplicitContentFilter)(long)xc.OldValue,
                                    After = (ExplicitContentFilter)(long)xc.NewValue
                                };
                                break;

                            case "mfa_level":
                                entrygld.MfaLevelChange = new PropertyChange<MfaLevel>
                                {
                                    Before = (MfaLevel)(long)xc.OldValue,
                                    After = (MfaLevel)(long)xc.NewValue
                                };
                                break;

                            case "region":
                                entrygld.RegionChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in guild update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                case AuditLogActionType.ChannelCreate:
                case AuditLogActionType.ChannelDelete:
                case AuditLogActionType.ChannelUpdate:
                    entry = new DiscordAuditLogChannelEntry
                    {
                        Target = GetChannel(xac.TargetId.Value) ?? new DiscordChannel { Id = xac.TargetId.Value, Discord = Discord, GuildId = Id }
                    };

                    DiscordAuditLogChannelEntry? entrychn = entry as DiscordAuditLogChannelEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "name":
                                entrychn.NameChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValue != null ? xc.OldValueString : null,
                                    After = xc.NewValue != null ? xc.NewValueString : null
                                };
                                break;

                            case "type":
                                p1 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                entrychn.TypeChange = new PropertyChange<ChannelType?>
                                {
                                    Before = p1 ? (ChannelType?)t1 : null,
                                    After = p2 ? (ChannelType?)t2 : null
                                };
                                break;

                            case "permission_overwrites":
                                IEnumerable<DiscordOverwrite>? olds = xc.OldValues?.OfType<JObject>()
                                    ?.Select(xjo => xjo.ToDiscordObject<DiscordOverwrite>())
                                    ?.Select(xo => { xo.Discord = Discord; return xo; });

                                IEnumerable<DiscordOverwrite>? news = xc.NewValues?.OfType<JObject>()
                                    ?.Select(xjo => xjo.ToDiscordObject<DiscordOverwrite>())
                                    ?.Select(xo => { xo.Discord = Discord; return xo; });

                                entrychn.OverwriteChange = new PropertyChange<IReadOnlyList<DiscordOverwrite>>
                                {
                                    Before = olds != null ? new ReadOnlyCollection<DiscordOverwrite>(new List<DiscordOverwrite>(olds)) : null,
                                    After = news != null ? new ReadOnlyCollection<DiscordOverwrite>(new List<DiscordOverwrite>(news)) : null
                                };
                                break;

                            case "topic":
                                entrychn.TopicChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;

                            case "nsfw":
                                entrychn.NsfwChange = new PropertyChange<bool?>
                                {
                                    Before = (bool?)xc.OldValue,
                                    After = (bool?)xc.NewValue
                                };
                                break;

                            case "bitrate":
                                entrychn.BitrateChange = new PropertyChange<int?>
                                {
                                    Before = (int?)(long?)xc.OldValue,
                                    After = (int?)(long?)xc.NewValue
                                };
                                break;

                            case "rate_limit_per_user":
                                entrychn.PerUserRateLimitChange = new PropertyChange<int?>
                                {
                                    Before = (int?)(long?)xc.OldValue,
                                    After = (int?)(long?)xc.NewValue
                                };
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in channel update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                case AuditLogActionType.OverwriteCreate:
                case AuditLogActionType.OverwriteDelete:
                case AuditLogActionType.OverwriteUpdate:
                    entry = new DiscordAuditLogOverwriteEntry
                    {
                        Target = GetChannel(xac.TargetId.Value)?.PermissionOverwrites.FirstOrDefault(xo => xo.Id == xac.Options.Id),
                        Channel = GetChannel(xac.TargetId.Value)
                    };

                    DiscordAuditLogOverwriteEntry? entryovr = entry as DiscordAuditLogOverwriteEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "deny":
                                p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                entryovr.DenyChange = new PropertyChange<Permissions?>
                                {
                                    Before = p1 ? (Permissions?)t1 : null,
                                    After = p2 ? (Permissions?)t2 : null
                                };
                                break;

                            case "allow":
                                p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                entryovr.AllowChange = new PropertyChange<Permissions?>
                                {
                                    Before = p1 ? (Permissions?)t1 : null,
                                    After = p2 ? (Permissions?)t2 : null
                                };
                                break;

                            case "type":
                                entryovr.TypeChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;

                            case "id":
                                p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                entryovr.TargetIdChange = new PropertyChange<ulong?>
                                {
                                    Before = p1 ? (ulong?)t1 : null,
                                    After = p2 ? (ulong?)t2 : null
                                };
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in overwrite update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                case AuditLogActionType.Kick:
                    entry = new DiscordAuditLogKickEntry
                    {
                        Target = amd.TryGetValue(xac.TargetId.Value, out DiscordMember? kickMember) ? kickMember : new DiscordMember { Id = xac.TargetId.Value, Discord = Discord, _guild_id = Id }
                    };
                    break;

                case AuditLogActionType.Prune:
                    entry = new DiscordAuditLogPruneEntry
                    {
                        Days = xac.Options.DeleteMemberDays,
                        Toll = xac.Options.MembersRemoved
                    };
                    break;

                case AuditLogActionType.Ban:
                case AuditLogActionType.Unban:
                    entry = new DiscordAuditLogBanEntry
                    {
                        Target = amd.TryGetValue(xac.TargetId.Value, out DiscordMember? unbanMember) ? unbanMember : new DiscordMember { Id = xac.TargetId.Value, Discord = Discord, _guild_id = Id }
                    };
                    break;

                case AuditLogActionType.MemberUpdate:
                case AuditLogActionType.MemberRoleUpdate:
                    entry = new DiscordAuditLogMemberUpdateEntry
                    {
                        Target = amd.TryGetValue(xac.TargetId.Value, out DiscordMember? roleUpdMember) ? roleUpdMember : new DiscordMember { Id = xac.TargetId.Value, Discord = Discord, _guild_id = Id }
                    };

                    DiscordAuditLogMemberUpdateEntry? entrymbu = entry as DiscordAuditLogMemberUpdateEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "nick":
                                entrymbu.NicknameChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;

                            case "deaf":
                                entrymbu.DeafenChange = new PropertyChange<bool?>
                                {
                                    Before = (bool?)xc.OldValue,
                                    After = (bool?)xc.NewValue
                                };
                                break;

                            case "mute":
                                entrymbu.MuteChange = new PropertyChange<bool?>
                                {
                                    Before = (bool?)xc.OldValue,
                                    After = (bool?)xc.NewValue
                                };
                                break;

                            case "communication_disabled_until":
                                entrymbu.TimeoutChange = new PropertyChange<DateTime?>
                                {
                                    Before = xc.OldValue != null ? (DateTime)xc.OldValue : null,
                                    After = xc.NewValue != null ? (DateTime)xc.NewValue : null
                                };
                                break;

                            case "$add":
                                entrymbu.AddedRoles = new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"]).Select(GetRole).ToList());
                                break;

                            case "$remove":
                                entrymbu.RemovedRoles = new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"]).Select(GetRole).ToList());
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in member update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                case AuditLogActionType.RoleCreate:
                case AuditLogActionType.RoleDelete:
                case AuditLogActionType.RoleUpdate:
                    entry = new DiscordAuditLogRoleUpdateEntry
                    {
                        Target = GetRole(xac.TargetId.Value) ?? new DiscordRole { Id = xac.TargetId.Value, Discord = Discord }
                    };

                    DiscordAuditLogRoleUpdateEntry? entryrol = entry as DiscordAuditLogRoleUpdateEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "name":
                                entryrol.NameChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;

                            case "color":
                                p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
                                p2 = int.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

                                entryrol.ColorChange = new PropertyChange<int?>
                                {
                                    Before = p1 ? (int?)t3 : null,
                                    After = p2 ? (int?)t4 : null
                                };
                                break;

                            case "permissions":
                                entryrol.PermissionChange = new PropertyChange<Permissions?>
                                {
                                    Before = xc.OldValue != null ? (Permissions?)long.Parse((string)xc.OldValue) : null,
                                    After = xc.NewValue != null ? (Permissions?)long.Parse((string)xc.NewValue) : null
                                };
                                break;

                            case "position":
                                entryrol.PositionChange = new PropertyChange<int?>
                                {
                                    Before = xc.OldValue != null ? (int?)(long)xc.OldValue : null,
                                    After = xc.NewValue != null ? (int?)(long)xc.NewValue : null,
                                };
                                break;

                            case "mentionable":
                                entryrol.MentionableChange = new PropertyChange<bool?>
                                {
                                    Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
                                    After = xc.NewValue != null ? (bool?)xc.NewValue : null
                                };
                                break;

                            case "hoist":
                                entryrol.HoistChange = new PropertyChange<bool?>
                                {
                                    Before = (bool?)xc.OldValue,
                                    After = (bool?)xc.NewValue
                                };
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in role update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                case AuditLogActionType.InviteCreate:
                case AuditLogActionType.InviteDelete:
                case AuditLogActionType.InviteUpdate:
                    entry = new DiscordAuditLogInviteEntry();

                    DiscordInvite inv = new DiscordInvite
                    {
                        Discord = Discord,
                        Guild = new DiscordInviteGuild
                        {
                            Discord = Discord,
                            Id = Id,
                            Name = Name,
                            SplashHash = SplashHash
                        }
                    };

                    DiscordAuditLogInviteEntry? entryinv = entry as DiscordAuditLogInviteEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "max_age":
                                p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
                                p2 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

                                entryinv.MaxAgeChange = new PropertyChange<int?>
                                {
                                    Before = p1 ? (int?)t3 : null,
                                    After = p2 ? (int?)t4 : null
                                };
                                break;

                            case "code":
                                inv.Code = xc.OldValueString ?? xc.NewValueString;

                                entryinv.CodeChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;

                            case "temporary":
                                entryinv.TemporaryChange = new PropertyChange<bool?>
                                {
                                    Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
                                    After = xc.NewValue != null ? (bool?)xc.NewValue : null
                                };
                                break;

                            case "inviter_id":
                                p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                entryinv.InviterChange = new PropertyChange<DiscordMember>
                                {
                                    Before = amd.TryGetValue(t1, out DiscordMember? propBeforeMember) ? propBeforeMember : new DiscordMember { Id = t1, Discord = Discord, _guild_id = Id },
                                    After = amd.TryGetValue(t2, out DiscordMember? propAfterMember) ? propAfterMember : new DiscordMember { Id = t1, Discord = Discord, _guild_id = Id },
                                };
                                break;

                            case "channel_id":
                                p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                entryinv.ChannelChange = new PropertyChange<DiscordChannel>
                                {
                                    Before = p1 ? GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id } : null,
                                    After = p2 ? GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id } : null
                                };

                                DiscordChannel? ch = entryinv.ChannelChange.Before ?? entryinv.ChannelChange.After;
                                ChannelType? cht = ch?.Type;
                                inv.Channel = new DiscordInviteChannel
                                {
                                    Discord = Discord,
                                    Id = p1 ? t1 : t2,
                                    Name = ch?.Name,
                                    Type = cht != null ? cht.Value : ChannelType.Unknown
                                };
                                break;

                            case "uses":
                                p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
                                p2 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

                                entryinv.UsesChange = new PropertyChange<int?>
                                {
                                    Before = p1 ? (int?)t3 : null,
                                    After = p2 ? (int?)t4 : null
                                };
                                break;

                            case "max_uses":
                                p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
                                p2 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

                                entryinv.MaxUsesChange = new PropertyChange<int?>
                                {
                                    Before = p1 ? (int?)t3 : null,
                                    After = p2 ? (int?)t4 : null
                                };
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in invite update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }

                    entryinv.Target = inv;
                    break;

                case AuditLogActionType.WebhookCreate:
                case AuditLogActionType.WebhookDelete:
                case AuditLogActionType.WebhookUpdate:
                    entry = new DiscordAuditLogWebhookEntry
                    {
                        Target = ahd.TryGetValue(xac.TargetId.Value, out DiscordWebhook? webhook) ? webhook : new DiscordWebhook { Id = xac.TargetId.Value, Discord = Discord }
                    };

                    DiscordAuditLogWebhookEntry? entrywhk = entry as DiscordAuditLogWebhookEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "name":
                                entrywhk.NameChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;

                            case "channel_id":
                                p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                entrywhk.ChannelChange = new PropertyChange<DiscordChannel>
                                {
                                    Before = p1 ? GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id } : null,
                                    After = p2 ? GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id } : null
                                };
                                break;

                            case "type": // ???
                                p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
                                p2 = int.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

                                entrywhk.TypeChange = new PropertyChange<int?>
                                {
                                    Before = p1 ? (int?)t3 : null,
                                    After = p2 ? (int?)t4 : null
                                };
                                break;

                            case "avatar_hash":
                                entrywhk.AvatarHashChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;

                            case "application_id": //Why the fuck does discord send this as a string if it's supposed to be a snowflake
                                entrywhk.ApplicationIdChange = new PropertyChange<ulong?>
                                {
                                    Before = xc.OldValue != null ? Convert.ToUInt64(xc.OldValueString) : null,
                                    After = xc.NewValue != null ? Convert.ToUInt64(xc.NewValueString) : null
                                };
                                break;


                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in webhook update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                case AuditLogActionType.EmojiCreate:
                case AuditLogActionType.EmojiDelete:
                case AuditLogActionType.EmojiUpdate:
                    entry = new DiscordAuditLogEmojiEntry
                    {
                        Target = _emojis.TryGetValue(xac.TargetId.Value, out DiscordEmoji? target) ? target : new DiscordEmoji { Id = xac.TargetId.Value, Discord = Discord }
                    };

                    DiscordAuditLogEmojiEntry? entryemo = entry as DiscordAuditLogEmojiEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "name":
                                entryemo.NameChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in emote update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                case AuditLogActionType.StickerCreate:
                case AuditLogActionType.StickerDelete:
                case AuditLogActionType.StickerUpdate:
                    entry = new DiscordAuditLogStickerEntry
                    {
                        Target = _stickers.TryGetValue(xac.TargetId.Value, out DiscordMessageSticker? sticker) ? sticker : new DiscordMessageSticker { Id = xac.TargetId.Value, Discord = Discord }
                    };

                    DiscordAuditLogStickerEntry? entrysti = entry as DiscordAuditLogStickerEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "name":
                                entrysti.NameChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;
                            case "description":
                                entrysti.DescriptionChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;
                            case "tags":
                                entrysti.TagsChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;
                            case "guild_id":
                                entrysti.GuildIdChange = new PropertyChange<ulong?>
                                {
                                    Before = ulong.TryParse(xc.OldValueString, out ulong ogid) ? ogid : null,
                                    After = ulong.TryParse(xc.NewValueString, out ulong ngid) ? ngid : null
                                };
                                break;
                            case "available":
                                entrysti.AvailabilityChange = new PropertyChange<bool?>
                                {
                                    Before = (bool?)xc.OldValue,
                                    After = (bool?)xc.NewValue,
                                };
                                break;
                            case "asset":
                                entrysti.AssetChange = new PropertyChange<string>
                                {
                                    Before = xc.OldValueString,
                                    After = xc.NewValueString
                                };
                                break;
                            case "id":
                                entrysti.IdChange = new PropertyChange<ulong?>
                                {
                                    Before = ulong.TryParse(xc.OldValueString, out ulong oid) ? oid : null,
                                    After = ulong.TryParse(xc.NewValueString, out ulong nid) ? nid : null
                                };
                                break;
                            case "type":
                                p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
                                p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);
                                entrysti.TypeChange = new PropertyChange<StickerType?>
                                {
                                    Before = p1 ? (StickerType?)t5 : null,
                                    After = p2 ? (StickerType?)t6 : null
                                };
                                break;
                            case "format_type":
                                p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
                                p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);
                                entrysti.FormatChange = new PropertyChange<StickerFormat?>
                                {
                                    Before = p1 ? (StickerFormat?)t5 : null,
                                    After = p2 ? (StickerFormat?)t6 : null
                                };
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in sticker update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                case AuditLogActionType.MessageDelete:
                case AuditLogActionType.MessageBulkDelete:
                    {
                        entry = new DiscordAuditLogMessageEntry();

                        DiscordAuditLogMessageEntry? entrymsg = entry as DiscordAuditLogMessageEntry;

                        if (xac.Options != null)
                        {
                            entrymsg.Channel = GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = Discord, GuildId = Id };
                            entrymsg.MessageCount = xac.Options.Count;
                        }

                        if (entrymsg.Channel != null)
                        {
                            entrymsg.Target = Discord is DiscordClient dc
                                && dc.MessageCache != null
                                && dc.MessageCache.TryGet(xm => xm.Id == xac.TargetId.Value && xm.ChannelId == entrymsg.Channel.Id, out DiscordMessage? msg)
                                ? msg
                                : new DiscordMessage { Discord = Discord, Id = xac.TargetId.Value };
                        }
                        break;
                    }

                case AuditLogActionType.MessagePin:
                case AuditLogActionType.MessageUnpin:
                    {
                        entry = new DiscordAuditLogMessagePinEntry();

                        DiscordAuditLogMessagePinEntry? entrypin = entry as DiscordAuditLogMessagePinEntry;

                        if (Discord is not DiscordClient dc)
                        {
                            break;
                        }

                        if (xac.Options != null)
                        {
                            DiscordMessage message = default;
                            dc.MessageCache?.TryGet(x => x.Id == xac.Options.MessageId && x.ChannelId == xac.Options.ChannelId, out message);

                            entrypin.Channel = GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = Discord, GuildId = Id };
                            entrypin.Message = message ?? new DiscordMessage { Id = xac.Options.MessageId, Discord = Discord };
                        }

                        if (xac.TargetId.HasValue)
                        {
                            dc.UserCache.TryGetValue(xac.TargetId.Value, out DiscordUser? user);
                            entrypin.Target = user ?? new DiscordUser { Id = user.Id, Discord = Discord };
                        }

                        break;
                    }

                case AuditLogActionType.BotAdd:
                    {
                        entry = new DiscordAuditLogBotAddEntry();

                        if (!(Discord is DiscordClient dc && xac.TargetId.HasValue))
                        {
                            break;
                        }

                        dc.UserCache.TryGetValue(xac.TargetId.Value, out DiscordUser? bot);
                        (entry as DiscordAuditLogBotAddEntry).TargetBot = bot ?? new DiscordUser { Id = xac.TargetId.Value, Discord = Discord };

                        break;
                    }

                case AuditLogActionType.MemberMove:
                    entry = new DiscordAuditLogMemberMoveEntry();

                    if (xac.Options == null)
                    {
                        break;
                    }

                    DiscordAuditLogMemberMoveEntry? moveentry = entry as DiscordAuditLogMemberMoveEntry;

                    moveentry.UserCount = xac.Options.Count;
                    moveentry.Channel = GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = Discord, GuildId = Id };
                    break;

                case AuditLogActionType.MemberDisconnect:
                    entry = new DiscordAuditLogMemberDisconnectEntry
                    {
                        UserCount = xac.Options?.Count ?? 0
                    };
                    break;

                case AuditLogActionType.IntegrationCreate:
                case AuditLogActionType.IntegrationDelete:
                case AuditLogActionType.IntegrationUpdate:
                    entry = new DiscordAuditLogIntegrationEntry();

                    DiscordAuditLogIntegrationEntry? integentry = entry as DiscordAuditLogIntegrationEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "enable_emoticons":
                                integentry.EnableEmoticons = new PropertyChange<bool?>
                                {
                                    Before = (bool?)xc.OldValue,
                                    After = (bool?)xc.NewValue
                                };
                                break;
                            case "expire_behavior":
                                integentry.ExpireBehavior = new PropertyChange<int?>
                                {
                                    Before = (int?)xc.OldValue,
                                    After = (int?)xc.NewValue
                                };
                                break;
                            case "expire_grace_period":
                                integentry.ExpireBehavior = new PropertyChange<int?>
                                {
                                    Before = (int?)xc.OldValue,
                                    After = (int?)xc.NewValue
                                };
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in integration update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                case AuditLogActionType.GuildScheduledEventCreate:
                case AuditLogActionType.GuildScheduledEventDelete:
                case AuditLogActionType.GuildScheduledEventUpdate:
                    entry = new DiscordAuditLogGuildScheduledEventEntry()
                    {
                        Target = events.TryGetValue(xac.TargetId.Value, out DiscordScheduledGuildEvent? ta) ? ta : new DiscordScheduledGuildEvent() { Id = xac.TargetId.Value, Discord = Discord },
                    };

                    DiscordAuditLogGuildScheduledEventEntry? evententry = entry as DiscordAuditLogGuildScheduledEventEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "name":
                                evententry.Name = new PropertyChange<string?>
                                {
                                    Before = xc.OldValue != null ? xc.OldValueString : null,
                                    After = xc.NewValue != null ? xc.NewValueString : null
                                };
                                break;
                            case "channel_id":
                                ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);
                                evententry.Channel = new PropertyChange<DiscordChannel?>
                                {
                                    Before = GetChannel(t2) ?? new DiscordChannel { Id = t2, Discord = Discord, GuildId = Id },
                                    After = GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = Discord, GuildId = Id }
                                };
                                break;

                            case "description":
                                evententry.Description = new PropertyChange<string?>
                                {
                                    Before = xc.OldValue != null ? xc.OldValueString : null,
                                    After = xc.NewValue != null ? xc.NewValueString : null
                                };
                                break;

                            case "entity_type":
                                evententry.Type = new PropertyChange<ScheduledGuildEventType?>
                                {
                                    Before = xc.OldValue != null ? (ScheduledGuildEventType)(long)xc.OldValue : null,
                                    After = xc.NewValue != null ? (ScheduledGuildEventType)(long)xc.NewValue : null
                                };
                                break;

                            case "image_hash":
                                evententry.ImageHash = new PropertyChange<string?>
                                {
                                    Before = (string?)xc.OldValue,
                                    After = (string?)xc.NewValue
                                };
                                break;

                            case "location":
                                evententry.Location = new PropertyChange<string?>
                                {
                                    Before = (string?)xc.OldValue,
                                    After = (string?)xc.NewValue
                                };
                                break;

                            case "privacy_level":
                                evententry.PrivacyLevel = new PropertyChange<ScheduledGuildEventPrivacyLevel?>
                                {
                                    Before = xc.OldValue != null ? (ScheduledGuildEventPrivacyLevel)(long)xc.OldValue : null,
                                    After = xc.NewValue != null ? (ScheduledGuildEventPrivacyLevel)(long)xc.NewValue : null
                                };
                                break;

                            case "status":
                                evententry.Status = new PropertyChange<ScheduledGuildEventStatus?>
                                {
                                    Before = xc.OldValue != null ? (ScheduledGuildEventStatus)(long)xc.OldValue : null,
                                    After = xc.NewValue != null ? (ScheduledGuildEventStatus)(long)xc.NewValue : null
                                };
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in scheduled event update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                case AuditLogActionType.ThreadCreate:
                case AuditLogActionType.ThreadDelete:
                case AuditLogActionType.ThreadUpdate:
                    entry = new DiscordAuditLogThreadEventEntry()
                    {
                        Target = Threads.TryGetValue(xac.TargetId.Value, out DiscordThreadChannel? channel) ? channel : new DiscordThreadChannel() { Id = xac.TargetId.Value, Discord = Discord },
                    };

                    DiscordAuditLogThreadEventEntry? threadentry = entry as DiscordAuditLogThreadEventEntry;
                    foreach (AuditLogActionChange xc in xac.Changes)
                    {
                        switch (xc.Key.ToLowerInvariant())
                        {
                            case "name":
                                threadentry.Name = new PropertyChange<string?>
                                {
                                    Before = xc.OldValue != null ? xc.OldValueString : null,
                                    After = xc.NewValue != null ? xc.NewValueString : null
                                };
                                break;

                            case "type":
                                threadentry.Type = new PropertyChange<ChannelType?>
                                {
                                    Before = xc.OldValue != null ? (ChannelType)xc.OldValueLong : null,
                                    After = xc.NewValue != null ? (ChannelType)xc.NewValueLong : null
                                };
                                break;

                            case "archived":
                                threadentry.Archived = new PropertyChange<bool?>
                                {
                                    Before = xc.OldValue != null ? xc.OldValueBool : null,
                                    After = xc.NewValue != null ? xc.NewValueBool : null
                                };
                                break;

                            case "auto_archive_duration":
                                threadentry.AutoArchiveDuration = new PropertyChange<int?>
                                {
                                    Before = xc.OldValue != null ? (int)xc.OldValueLong : null,
                                    After = xc.NewValue != null ? (int)xc.NewValueLong : null
                                };
                                break;

                            case "invitable":
                                threadentry.Invitable = new PropertyChange<bool?>
                                {
                                    Before = xc.OldValue != null ? xc.OldValueBool : null,
                                    After = xc.NewValue != null ? xc.NewValueBool : null
                                };
                                break;

                            case "locked":
                                threadentry.Locked = new PropertyChange<bool?>
                                {
                                    Before = xc.OldValue != null ? xc.OldValueBool : null,
                                    After = xc.NewValue != null ? xc.NewValueBool : null
                                };
                                break;

                            case "rate_limit_per_user":
                                threadentry.PerUserRateLimit = new PropertyChange<int?>
                                {
                                    Before = xc.OldValue != null ? (int)xc.OldValueLong : null,
                                    After = xc.NewValue != null ? (int)xc.NewValueLong : null
                                };
                                break;

                            default:
                                Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in thread update: {Key} - this should be reported to library developers", xc.Key);
                                break;
                        }
                    }
                    break;

                default:
                    Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown audit log action type: {0} - this should be reported to library developers", (int)xac.ActionType);
                    break;
            }

            if (entry == null)
            {
                continue;
            }

            entry.ActionCategory = xac.ActionType switch
            {
                AuditLogActionType.ChannelCreate or AuditLogActionType.EmojiCreate or AuditLogActionType.InviteCreate or AuditLogActionType.OverwriteCreate or AuditLogActionType.RoleCreate or AuditLogActionType.WebhookCreate or AuditLogActionType.IntegrationCreate or AuditLogActionType.StickerCreate => AuditLogActionCategory.Create,
                AuditLogActionType.ChannelDelete or AuditLogActionType.EmojiDelete or AuditLogActionType.InviteDelete or AuditLogActionType.MessageDelete or AuditLogActionType.MessageBulkDelete or AuditLogActionType.OverwriteDelete or AuditLogActionType.RoleDelete or AuditLogActionType.WebhookDelete or AuditLogActionType.IntegrationDelete or AuditLogActionType.StickerDelete => AuditLogActionCategory.Delete,
                AuditLogActionType.ChannelUpdate or AuditLogActionType.EmojiUpdate or AuditLogActionType.InviteUpdate or AuditLogActionType.MemberRoleUpdate or AuditLogActionType.MemberUpdate or AuditLogActionType.OverwriteUpdate or AuditLogActionType.RoleUpdate or AuditLogActionType.WebhookUpdate or AuditLogActionType.IntegrationUpdate or AuditLogActionType.StickerUpdate => AuditLogActionCategory.Update,
                _ => AuditLogActionCategory.Other,
            };
            entry.Discord = Discord;
            entry.ActionType = xac.ActionType;
            entry.Id = xac.Id;
            entry.Reason = xac.Reason;
            entry.UserResponsible = amd[xac.UserId];
            entries.Add(entry);
        }

        return new ReadOnlyCollection<DiscordAuditLogEntry>(entries);
    }

    /// <summary>
    /// Gets all of this guild's custom emojis.
    /// </summary>
    /// <returns>All of this guild's custom emojis.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordGuildEmoji>> GetEmojisAsync()
        => Discord.ApiClient.GetGuildEmojisAsync(Id);

    /// <summary>
    /// Gets this guild's specified custom emoji.
    /// </summary>
    /// <param name="id">ID of the emoji to get.</param>
    /// <returns>The requested custom emoji.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildEmoji> GetEmojiAsync(ulong id)
        => Discord.ApiClient.GetGuildEmojiAsync(Id, id);

    /// <summary>
    /// Creates a new custom emoji for this guild.
    /// </summary>
    /// <param name="name">Name of the new emoji.</param>
    /// <param name="image">Image to use as the emoji.</param>
    /// <param name="roles">Roles for which the emoji will be available. This works only if your application is whitelisted as integration.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns>The newly-created emoji.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojis"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildEmoji> CreateEmojiAsync(string name, Stream image, IEnumerable<DiscordRole> roles = null, string reason = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        name = name.Trim();
        if (name.Length < 2 || name.Length > 50)
        {
            throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.");
        }

        if (image == null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        string image64 = null;
        using (ImageTool imgtool = new ImageTool(image))
        {
            image64 = imgtool.GetBase64();
        }

        return Discord.ApiClient.CreateGuildEmojiAsync(Id, name, image64, roles?.Select(xr => xr.Id), reason);
    }

    /// <summary>
    /// Modifies a this guild's custom emoji.
    /// </summary>
    /// <param name="emoji">Emoji to modify.</param>
    /// <param name="name">New name for the emoji.</param>
    /// <param name="roles">Roles for which the emoji will be available. This works only if your application is whitelisted as integration.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns>The modified emoji.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojis"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildEmoji> ModifyEmojiAsync(DiscordGuildEmoji emoji, string name, IEnumerable<DiscordRole> roles = null, string reason = null)
    {
        if (emoji == null)
        {
            throw new ArgumentNullException(nameof(emoji));
        }

        if (emoji.Guild.Id != Id)
        {
            throw new ArgumentException("This emoji does not belong to this guild.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        name = name.Trim();
        return name.Length < 2 || name.Length > 50
            ? throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.")
            : Discord.ApiClient.ModifyGuildEmojiAsync(Id, emoji.Id, name, roles?.Select(xr => xr.Id), reason);
    }

    /// <summary>
    /// Deletes this guild's custom emoji.
    /// </summary>
    /// <param name="emoji">Emoji to delete.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojis"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteEmojiAsync(DiscordGuildEmoji emoji, string reason = null)
    {
        return emoji == null
            ? throw new ArgumentNullException(nameof(emoji))
            : emoji.Guild.Id != Id
            ? throw new ArgumentException("This emoji does not belong to this guild.")
            : Discord.ApiClient.DeleteGuildEmojiAsync(Id, emoji.Id, reason);
    }

    /// <summary>
    /// <para>Gets the default channel for this guild.</para>
    /// <para>Default channel is the first channel current member can see.</para>
    /// </summary>
    /// <returns>This member's default guild.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public DiscordChannel GetDefaultChannel() => _channels?.Values.Where(xc => xc.Type == ChannelType.Text)
                .OrderBy(xc => xc.Position)
                .FirstOrDefault(xc => (xc.PermissionsFor(CurrentMember) & DSharpPlus.Permissions.AccessChannels) == DSharpPlus.Permissions.AccessChannels);

    /// <summary>
    /// Gets the guild's widget
    /// </summary>
    /// <returns>The guild's widget</returns>
    public Task<DiscordWidget> GetWidgetAsync()
        => Discord.ApiClient.GetGuildWidgetAsync(Id);

    /// <summary>
    /// Gets the guild's widget settings
    /// </summary>
    /// <returns>The guild's widget settings</returns>
    public Task<DiscordWidgetSettings> GetWidgetSettingsAsync()
        => Discord.ApiClient.GetGuildWidgetSettingsAsync(Id);

    /// <summary>
    /// Modifies the guild's widget settings
    /// </summary>
    /// <param name="isEnabled">If the widget is enabled or not</param>
    /// <param name="channel">Widget channel</param>
    /// <param name="reason">Reason the widget settings were modified</param>
    /// <returns>The newly modified widget settings</returns>
    public Task<DiscordWidgetSettings> ModifyWidgetSettingsAsync(bool? isEnabled = null, DiscordChannel channel = null, string reason = null)
        => Discord.ApiClient.ModifyGuildWidgetSettingsAsync(Id, isEnabled, channel?.Id, reason);

    /// <summary>
    /// Gets all of this guild's templates.
    /// </summary>
    /// <returns>All of the guild's templates.</returns>
    /// <exception cref="UnauthorizedException">Throws when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<DiscordGuildTemplate>> GetTemplatesAsync()
        => Discord.ApiClient.GetGuildTemplatesAsync(Id);

    /// <summary>
    /// Creates a guild template.
    /// </summary>
    /// <param name="name">Name of the template.</param>
    /// <param name="description">Description of the template.</param>
    /// <returns>The template created.</returns>
    /// <exception cref="BadRequestException">Throws when a template already exists for the guild or a null parameter is provided for the name.</exception>
    /// <exception cref="UnauthorizedException">Throws when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildTemplate> CreateTemplateAsync(string name, string description = null)
        => Discord.ApiClient.CreateGuildTemplateAsync(Id, name, description);

    /// <summary>
    /// Syncs the template to the current guild's state.
    /// </summary>
    /// <param name="code">The code of the template to sync.</param>
    /// <returns>The template synced.</returns>
    /// <exception cref="NotFoundException">Throws when the template for the code cannot be found</exception>
    /// <exception cref="UnauthorizedException">Throws when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildTemplate> SyncTemplateAsync(string code)
        => Discord.ApiClient.SyncGuildTemplateAsync(Id, code);

    /// <summary>
    /// Modifies the template's metadata.
    /// </summary>
    /// <param name="code">The template's code.</param>
    /// <param name="name">Name of the template.</param>
    /// <param name="description">Description of the template.</param>
    /// <returns>The template modified.</returns>
    /// <exception cref="NotFoundException">Throws when the template for the code cannot be found</exception>
    /// <exception cref="UnauthorizedException">Throws when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildTemplate> ModifyTemplateAsync(string code, string name = null, string description = null)
        => Discord.ApiClient.ModifyGuildTemplateAsync(Id, code, name, description);

    /// <summary>
    /// Deletes the template.
    /// </summary>
    /// <param name="code">The code of the template to delete.</param>
    /// <returns>The deleted template.</returns>
    /// <exception cref="NotFoundException">Throws when the template for the code cannot be found</exception>
    /// <exception cref="UnauthorizedException">Throws when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildTemplate> DeleteTemplateAsync(string code)
        => Discord.ApiClient.DeleteGuildTemplateAsync(Id, code);

    /// <summary>
    /// Gets this guild's membership screening form.
    /// </summary>
    /// <returns>This guild's membership screening form.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildMembershipScreening> GetMembershipScreeningFormAsync()
        => Discord.ApiClient.GetGuildMembershipScreeningFormAsync(Id);

    /// <summary>
    /// Modifies this guild's membership screening form.
    /// </summary>
    /// <param name="action">Action to perform</param>
    /// <returns>The modified screening form.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client doesn't have the <see cref="Permissions.ManageGuild"/> permission, or community is not enabled on this guild.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildMembershipScreening> ModifyMembershipScreeningFormAsync(Action<MembershipScreeningEditModel> action)
    {
        MembershipScreeningEditModel mdl = new MembershipScreeningEditModel();
        action(mdl);
        return Discord.ApiClient.ModifyGuildMembershipScreeningFormAsync(Id, mdl.Enabled, mdl.Fields, mdl.Description);
    }

    /// <summary>
    /// Gets a list of stickers from this guild.
    /// </summary>
    /// <returns></returns>
    public Task<IReadOnlyList<DiscordMessageSticker>> GetStickersAsync()
        => Discord.ApiClient.GetGuildStickersAsync(Id);

    /// <summary>
    /// Gets a sticker from this guild.
    /// </summary>
    /// <param name="stickerId">The id of the sticker.</param>
    /// <returns></returns>
    public Task<DiscordMessageSticker> GetStickerAsync(ulong stickerId)
        => Discord.ApiClient.GetGuildStickerAsync(Id, stickerId);

    /// <summary>
    /// Creates a sticker in this guild. Lottie stickers can only be created on verified and/or partnered servers.
    /// </summary>
    /// <param name="name">The name of the sticker.</param>
    /// <param name="description">The description of the sticker.</param>
    /// <param name="tags">The tags of the sticker. This must be a unicode emoji.</param>
    /// <param name="imageContents">The image content of the sticker.</param>
    /// <param name="format">The image format of the sticker.</param>
    /// <param name="reason">The reason this sticker is being created.</param>

    public Task<DiscordMessageSticker> CreateStickerAsync(string name, string description, string tags, Stream imageContents, StickerFormat format, string reason = null)
    {
        string contentType = null, extension = null;

        if (format == StickerFormat.PNG || format == StickerFormat.APNG)
        {
            contentType = "image/png";
            extension = "png";
        }
        else
        {
            if (!Features.Contains("PARTNERED") && !Features.Contains("VERIFIED"))
            {
                throw new InvalidOperationException("Lottie stickers can only be created on partnered or verified guilds.");
            }

            contentType = "application/json";
            extension = "json";
        }

        return Discord.ApiClient.CreateGuildStickerAsync(Id, name, description ?? string.Empty, tags, new DiscordMessageFile(null, imageContents, null, extension, contentType), reason);
    }

    /// <summary>
    /// Modifies a sticker in this guild.
    /// </summary>
    /// <param name="stickerId">The id of the sticker.</param>
    /// <param name="action">Action to perform.</param>
    /// <param name="reason">Reason for audit log.</param>
    public Task<DiscordMessageSticker> ModifyStickerAsync(ulong stickerId, Action<StickerEditModel> action, string reason = null)
    {
        StickerEditModel mdl = new StickerEditModel();
        action(mdl);
        return Discord.ApiClient.ModifyStickerAsync(Id, stickerId, mdl.Name, mdl.Description, mdl.Tags, reason ?? mdl.AuditLogReason);
    }

    /// <summary>
    /// Modifies a sticker in this guild.
    /// </summary>
    /// <param name="sticker">Sticker to modify.</param>
    /// <param name="action">Action to perform.</param>
    /// <param name="reason">Reason for audit log.</param>
    public Task<DiscordMessageSticker> ModifyStickerAsync(DiscordMessageSticker sticker, Action<StickerEditModel> action, string reason = null)
    {
        StickerEditModel mdl = new StickerEditModel();
        action(mdl);
        return Discord.ApiClient.ModifyStickerAsync(Id, sticker.Id, mdl.Name, mdl.Description, mdl.Tags, reason ?? mdl.AuditLogReason);
    }

    /// <summary>
    /// Deletes a sticker in this guild.
    /// </summary>
    /// <param name="stickerId">The id of the sticker.</param>
    /// <param name="reason">Reason for audit log.</param>
    public Task DeleteStickerAsync(ulong stickerId, string reason = null)
        => Discord.ApiClient.DeleteStickerAsync(Id, stickerId, reason);

    /// <summary>
    /// Deletes a sticker in this guild.
    /// </summary>
    /// <param name="sticker">Sticker to delete.</param>
    /// <param name="reason">Reason for audit log.</param>
    public Task DeleteStickerAsync(DiscordMessageSticker sticker, string reason = null)
        => Discord.ApiClient.DeleteStickerAsync(Id, sticker.Id, reason);

    /// <summary>
    /// Gets all the application commands in this guild.
    /// </summary>
    /// <returns>A list of application commands in this guild.</returns>
    public Task<IReadOnlyList<DiscordApplicationCommand>> GetApplicationCommandsAsync() =>
        Discord.ApiClient.GetGuildApplicationCommandsAsync(Discord.CurrentApplication.Id, Id);

    /// <summary>
    /// Overwrites the existing application commands in this guild. New commands are automatically created and missing commands are automatically delete
    /// </summary>
    /// <param name="commands">The list of commands to overwrite with.</param>
    /// <returns>The list of guild commands</returns>
    public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteApplicationCommandsAsync(IEnumerable<DiscordApplicationCommand> commands) =>
        Discord.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(Discord.CurrentApplication.Id, Id, commands);

    /// <summary>
    /// Creates or overwrites a application command in this guild.
    /// </summary>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command.</returns>
    public Task<DiscordApplicationCommand> CreateApplicationCommandAsync(DiscordApplicationCommand command) =>
        Discord.ApiClient.CreateGuildApplicationCommandAsync(Discord.CurrentApplication.Id, Id, command);

    /// <summary>
    /// Edits a application command in this guild.
    /// </summary>
    /// <param name="commandId">The id of the command to edit.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The edit command.</returns>
    public async Task<DiscordApplicationCommand> EditApplicationCommandAsync(ulong commandId, Action<ApplicationCommandEditModel> action)
    {
        ApplicationCommandEditModel mdl = new ApplicationCommandEditModel();
        action(mdl);
        return await Discord.ApiClient.EditGuildApplicationCommandAsync(Discord.CurrentApplication.Id, Id, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.DefaultPermission, default, default, mdl.AllowDMUsage, mdl.DefaultMemberPermissions).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets this guild's welcome screen.
    /// </summary>
    /// <returns>This guild's welcome screen object.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildWelcomeScreen> GetWelcomeScreenAsync() =>
        Discord.ApiClient.GetGuildWelcomeScreenAsync(Id);

    /// <summary>
    /// Modifies this guild's welcome screen.
    /// </summary>
    /// <param name="action">Action to perform.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns>The modified welcome screen.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client doesn't have the <see cref="Permissions.ManageGuild"/> permission, or community is not enabled on this guild.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildWelcomeScreen> ModifyWelcomeScreenAsync(Action<WelcomeScreenEditModel> action, string reason = null)
    {
        WelcomeScreenEditModel mdl = new WelcomeScreenEditModel();
        action(mdl);
        return await Discord.ApiClient.ModifyGuildWelcomeScreenAsync(Id, mdl.Enabled, mdl.WelcomeChannels, mdl.Description, reason).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all application command permissions in this guild.
    /// </summary>
    /// <returns>A list of permissions.</returns>
    public Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> GetApplicationCommandsPermissionsAsync()
        => Discord.ApiClient.GetGuildApplicationCommandPermissionsAsync(Discord.CurrentApplication.Id, Id);

    /// <summary>
    /// Gets permissions for a application command in this guild.
    /// </summary>
    /// <param name="command">The command to get them for.</param>
    /// <returns>The permissions.</returns>
    public Task<DiscordGuildApplicationCommandPermissions> GetApplicationCommandPermissionsAsync(DiscordApplicationCommand command)
        => Discord.ApiClient.GetApplicationCommandPermissionsAsync(Discord.CurrentApplication.Id, Id, command.Id);

    /// <summary>
    /// Edits permissions for a application command in this guild.
    /// </summary>
    /// <param name="command">The command to edit permissions for.</param>
    /// <param name="permissions">The list of permissions to use.</param>
    /// <returns>The edited permissions.</returns>
    public Task<DiscordGuildApplicationCommandPermissions> EditApplicationCommandPermissionsAsync(DiscordApplicationCommand command, IEnumerable<DiscordApplicationCommandPermission> permissions)
        => Discord.ApiClient.EditApplicationCommandPermissionsAsync(Discord.CurrentApplication.Id, Id, command.Id, permissions);

    /// <summary>
    /// Batch edits permissions for a application command in this guild.
    /// </summary>
    /// <param name="permissions">The list of permissions to use.</param>
    /// <returns>A list of edited permissions.</returns>
    public Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> BatchEditApplicationCommandPermissionsAsync(IEnumerable<DiscordGuildApplicationCommandPermissions> permissions)
        => Discord.ApiClient.BatchEditApplicationCommandPermissionsAsync(Discord.CurrentApplication.Id, Id, permissions);
    #endregion

    /// <summary>
    /// Returns a string representation of this guild.
    /// </summary>
    /// <returns>String representation of this guild.</returns>
    public override string ToString() => $"Guild {Id}; {Name}";

    /// <summary>
    /// Checks whether this <see cref="DiscordGuild"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordGuild"/>.</returns>
    public override bool Equals(object obj) => Equals(obj as DiscordGuild);

    /// <summary>
    /// Checks whether this <see cref="DiscordGuild"/> is equal to another <see cref="DiscordGuild"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordGuild"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordGuild"/> is equal to this <see cref="DiscordGuild"/>.</returns>
    public bool Equals(DiscordGuild e)
    {
        return e is null ? false : ReferenceEquals(this, e) || Id == e.Id;
    }

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordGuild"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordGuild"/>.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="DiscordGuild"/> objects are equal.
    /// </summary>
    /// <param name="e1">First member to compare.</param>
    /// <param name="e2">Second member to compare.</param>
    /// <returns>Whether the two members are equal.</returns>
    public static bool operator ==(DiscordGuild e1, DiscordGuild e2)
    {
        object? o1 = e1 as object;
        object? o2 = e2 as object;

        return (o1 == null && o2 != null) || (o1 != null && o2 == null) ? false : (o1 == null && o2 == null) || e1.Id == e2.Id;
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordGuild"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First member to compare.</param>
    /// <param name="e2">Second member to compare.</param>
    /// <returns>Whether the two members are not equal.</returns>
    public static bool operator !=(DiscordGuild e1, DiscordGuild e2)
        => !(e1 == e2);
}

/// <summary>
/// Represents guild verification level.
/// </summary>
public enum VerificationLevel : int
{
    /// <summary>
    /// No verification. Anyone can join and chat right away.
    /// </summary>
    None = 0,

    /// <summary>
    /// Low verification level. Users are required to have a verified email attached to their account in order to be able to chat.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium verification level. Users are required to have a verified email attached to their account, and account age need to be at least 5 minutes in order to be able to chat.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// (╯°□°）╯︵ ┻━┻ verification level. Users are required to have a verified email attached to their account, account age need to be at least 5 minutes, and they need to be in the server for at least 10 minutes in order to be able to chat.
    /// </summary>
    High = 3,

    /// <summary>
    /// ┻━┻ ﾐヽ(ಠ益ಠ)ノ彡┻━┻ verification level. Users are required to have a verified phone number attached to their account.
    /// </summary>
    Highest = 4
}

/// <summary>
/// Represents default notification level for a guild.
/// </summary>
public enum DefaultMessageNotifications : int
{
    /// <summary>
    /// All messages will trigger push notifications.
    /// </summary>
    AllMessages = 0,

    /// <summary>
    /// Only messages that mention the user (or a role he's in) will trigger push notifications.
    /// </summary>
    MentionsOnly = 1
}

/// <summary>
/// Represents multi-factor authentication level required by a guild to use administrator functionality.
/// </summary>
public enum MfaLevel : int
{
    /// <summary>
    /// Multi-factor authentication is not required to use administrator functionality.
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// Multi-factor authentication is required to use administrator functionality.
    /// </summary>
    Enabled = 1
}

/// <summary>
/// Represents the value of explicit content filter in a guild.
/// </summary>
public enum ExplicitContentFilter : int
{
    /// <summary>
    /// Explicit content filter is disabled.
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// Only messages from members without any roles are scanned.
    /// </summary>
    MembersWithoutRoles = 1,

    /// <summary>
    /// Messages from all members are scanned.
    /// </summary>
    AllMembers = 2
}

/// <summary>
/// Represents the formats for a guild widget.
/// </summary>
public enum WidgetType : int
{
    /// <summary>
    /// The widget is represented in shield format.
    /// <para>This is the default widget type.</para>
    /// </summary>
    Shield = 0,

    /// <summary>
    /// The widget is represented as the first banner type.
    /// </summary>
    Banner1 = 1,

    /// <summary>
    /// The widget is represented as the second banner type.
    /// </summary>
    Banner2 = 2,

    /// <summary>
    /// The widget is represented as the third banner type.
    /// </summary>
    Banner3 = 3,

    /// <summary>
    /// The widget is represented in the fourth banner type.
    /// </summary>
    Banner4 = 4
}
