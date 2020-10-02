#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSharpPlus.Net.Models;
using DSharpPlus.Net.Serialization;
using DSharpPlus.Net.Abstractions;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Entities
{
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
            => !string.IsNullOrWhiteSpace(this.IconHash) ? $"https://cdn.discordapp.com/icons/{this.Id.ToString(CultureInfo.InvariantCulture)}/{IconHash}.jpg" : null;

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
            => !string.IsNullOrWhiteSpace(this.SplashHash) ? $"https://cdn.discordapp.com/splashes/{this.Id.ToString(CultureInfo.InvariantCulture)}/{SplashHash}.jpg" : null;

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
            => !string.IsNullOrWhiteSpace(this.DiscoverySplashHash) ? $"https://cdn.discordapp.com/discovery-splashes/{this.Id.ToString(CultureInfo.InvariantCulture)}/{DiscoverySplashHash}.jpg" : null;

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
        internal ulong OwnerId { get; set; }

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
            => this.Members.TryGetValue(this.OwnerId, out var owner)
                ? owner
                : this.Discord.ApiClient.GetGuildMemberAsync(this.Id, this.OwnerId).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Gets the guild's voice region ID.
        /// </summary>
        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        internal string VoiceRegionId { get; set; }

        /// <summary>
        /// Gets the guild's voice region.
        /// </summary>
        [JsonIgnore]
        public DiscordVoiceRegion VoiceRegion
            => this.Discord.VoiceRegions[this.VoiceRegionId];

        /// <summary>
        /// Gets the guild's AFK voice channel ID.
        /// </summary>
        [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong AfkChannelId { get; set; } = 0;

        /// <summary>
        /// Gets the guild's AFK voice channel.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel AfkChannel
            => this.GetChannel(this.AfkChannelId);

        /// <summary>
        /// Gets the guild's AFK timeout.
        /// </summary>
        [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
        public int AfkTimeout { get; internal set; }

        /// <summary>
        /// Gets whether this guild has the guild embed enabled.
        /// </summary>
        [JsonProperty("embed_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool EmbedEnabled { get; internal set; }

        /// <summary>
        /// Gets the ID of the channel from the guild's embed.
        /// </summary>
        [JsonProperty("embed_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong EmbedChannelId { get; set; }

        /// <summary>
        /// Gets the channel from the guild's embed.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel EmbedChannel
            => this.GetChannel(this.EmbedChannelId);

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

        [JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Include)]
        internal ulong? SystemChannelId { get; set; }

        /// <summary>
        /// Gets the channel where system messages (such as boost and welcome messages) are sent.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel SystemChannel => this.SystemChannelId.HasValue
            ? this.GetChannel(this.SystemChannelId.Value)
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
        internal ulong? WidgetChannelId { get; set; }

        /// <summary>
        /// Gets the widget channel for this guild.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel WidgetChannel => this.WidgetChannelId.HasValue
            ? this.GetChannel(this.WidgetChannelId.Value)
            : null;

        [JsonProperty("rules_channel_id")]
        internal ulong? RulesChannelId { get; set; }

        /// <summary>
        /// Gets the rules channel for this guild.
        /// <para>This is only available if the guild is considered "discoverable".</para>
        /// </summary>
        [JsonIgnore]
        public DiscordChannel RulesChannel => this.RulesChannelId.HasValue
            ? this.GetChannel(this.RulesChannelId.Value)
            : null;

        [JsonProperty("public_updates_channel_id")]
        internal ulong? PublicUpdatesChannelId { get; set; }

        /// <summary>
        /// Gets the public updates channel (where admins and moderators receive messages from Discord) for this guild.
        /// <para>This is only available if the guild is considered "discoverable".</para>
        /// </summary>
        [JsonIgnore]
        public DiscordChannel PublicUpdatesChannel => this.PublicUpdatesChannelId.HasValue
            ? this.GetChannel(this.PublicUpdatesChannelId.Value)
            : null;

        /// <summary>
        /// Gets the application id of this guild if it is bot created.
        /// </summary>
        [JsonProperty("application_id")]
        public ulong? ApplicationId { get; internal set; }

        /// <summary>
        /// Gets a collection of this guild's roles.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordRole> Roles => new ReadOnlyConcurrentDictionary<ulong, DiscordRole>(this._roles);

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordRole> _roles;

        /// <summary>
        /// Gets a collection of this guild's emojis.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordEmoji> Emojis => new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(this._emojis);

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
        /// Gets the approximate number of members in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name = "withCounts"></paramref> set to true.
        /// </summary>
        [JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? ApproximateMemberCount { get; internal set; }

        /// <summary>
        /// Gets the approximate number of presences in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name = "withCounts"></paramref> set to true.
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
        public IReadOnlyDictionary<ulong, DiscordVoiceState> VoiceStates => new ReadOnlyConcurrentDictionary<ulong, DiscordVoiceState>(this._voiceStates);

        [JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordVoiceState> _voiceStates;

        /// <summary>
        /// Gets a dictionary of all the members that belong to this guild. The dictionary's key is the member ID.
        /// </summary>
        [JsonIgnore] // TODO overhead of => vs Lazy? it's a struct
        public IReadOnlyDictionary<ulong, DiscordMember> Members => new ReadOnlyConcurrentDictionary<ulong, DiscordMember>(this._members);

        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordMember> _members;

        /// <summary>
        /// Gets a dictionary of all the channels associated with this guild. The dictionary's key is the channel ID.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordChannel> Channels => new ReadOnlyConcurrentDictionary<ulong, DiscordChannel>(this._channels);

        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordChannel> _channels;

        internal ConcurrentDictionary<string, DiscordInvite> _invites;

        /// <summary>
        /// Gets the guild member for current user.
        /// </summary>
        [JsonIgnore]
        public DiscordMember CurrentMember
            => this._current_member_lazy.Value;

        [JsonIgnore]
        private Lazy<DiscordMember> _current_member_lazy;

        /// <summary>
        /// Gets the @everyone role for this guild.
        /// </summary>
        [JsonIgnore]
        public DiscordRole EveryoneRole
            => this.GetRole(this.Id);

        [JsonIgnore]
        internal bool _isOwner;

        /// <summary>
        /// Gets whether the current user is the guild's owner.
        /// </summary>
        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsOwner
        {
            get => this._isOwner || this.OwnerId == this.Discord.CurrentUser.Id;
            internal set => this._isOwner = value;
        }

        /// <summary>
        /// Gets vanity URL code for this guild, when applicable.
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
            => !string.IsNullOrWhiteSpace(this.Banner) ? $"https://cdn.discordapp.com/banners/{this.Id}/{this.Banner}" : null;

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
        internal bool IsSynced { get; set; }

        internal DiscordGuild()
        { 
            this._current_member_lazy = new Lazy<DiscordMember>(() => this._members.TryGetValue(this.Discord.CurrentUser.Id, out var member) ? member : null);
            this._invites = new ConcurrentDictionary<string, DiscordInvite>();
        }

        #region Guild Methods
        /// <summary>
        /// Adds a new member to this guild
        /// </summary>
        /// <param name="user">User to add</param>
        /// <param name="access_token">User's access token (OAuth2)</param>
        /// <param name="nickname">new nickame</param>
        /// <param name="roles">new roles</param>
        /// <param name="muted">whether this user has to be muted</param>
        /// <param name="deaf">whether this user has to be deafened</param>
        /// <returns></returns>
        public Task AddMemberAsync(DiscordUser user, string access_token, string nickname = null, IEnumerable<DiscordRole> roles = null,
            bool muted = false, bool deaf = false)
            => this.Discord.ApiClient.AddGuildMemberAsync(this.Id, user.Id, access_token, nickname, roles, muted, deaf);

        /// <summary>
        /// Deletes this guild. Requires the caller to be the owner of the guild.
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync()
            => this.Discord.ApiClient.DeleteGuildAsync(this.Id);

        /// <summary>
        /// Modifies this guild.
        /// </summary>
        /// <param name="action">Action to perform on this guild..</param>
        /// <returns>The modified guild object.</returns>
        public async Task<DiscordGuild> ModifyAsync(Action<GuildEditModel> action)
        {
            var mdl = new GuildEditModel();
            action(mdl);
            if (mdl.AfkChannel.HasValue && mdl.AfkChannel.Value.Type != ChannelType.Voice)
                throw new ArgumentException("AFK channel needs to be a voice channel.");

            var iconb64 = Optional.FromNoValue<string>();
            if (mdl.Icon.HasValue && mdl.Icon.Value != null)
                using (var imgtool = new ImageTool(mdl.Icon.Value))
                    iconb64 = imgtool.GetBase64();
            else if (mdl.Icon.HasValue)
                iconb64 = null;

            var splashb64 = Optional.FromNoValue<string>();
            if (mdl.Splash.HasValue && mdl.Splash.Value != null)
                using (var imgtool = new ImageTool(mdl.Splash.Value))
                    splashb64 = imgtool.GetBase64();
            else if (mdl.Splash.HasValue)
                splashb64 = null;

            return await this.Discord.ApiClient.ModifyGuildAsync(this.Id, mdl.Name, mdl.Region.IfPresent(e => e.Id),
                mdl.VerificationLevel, mdl.DefaultMessageNotifications, mdl.MfaLevel, mdl.ExplicitContentFilter,
                mdl.AfkChannel.IfPresent(e => e?.Id), mdl.AfkTimeout, iconb64, mdl.Owner.IfPresent(e => e.Id), splashb64,
                mdl.SystemChannel.IfPresent(e => e?.Id), mdl.AuditLogReason).ConfigureAwait(false);
        }

        /// <summary>
        /// Bans a specified member from this guild.
        /// </summary>
        /// <param name="member">Member to ban.</param>
        /// <param name="delete_message_days">How many days to remove messages from.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task BanMemberAsync(DiscordMember member, int delete_message_days = 0, string reason = null)
            => this.Discord.ApiClient.CreateGuildBanAsync(this.Id, member.Id, delete_message_days, reason);

        /// <summary>
        /// Bans a specified user by ID. This doesn't require the user to be in this guild.
        /// </summary>
        /// <param name="user_id">ID of the user to ban.</param>
        /// <param name="delete_message_days">How many days to remove messages from.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task BanMemberAsync(ulong user_id, int delete_message_days = 0, string reason = null)
            => this.Discord.ApiClient.CreateGuildBanAsync(this.Id, user_id, delete_message_days, reason);

        /// <summary>
        /// Unbans a user from this guild.
        /// </summary>
        /// <param name="user">User to unban.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task UnbanMemberAsync(DiscordUser user, string reason = null)
            => this.Discord.ApiClient.RemoveGuildBanAsync(this.Id, user.Id, reason);

        /// <summary>
        /// Unbans a user by ID.
        /// </summary>
        /// <param name="user_id">ID of the user to unban.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task UnbanMemberAsync(ulong user_id, string reason = null)
            => this.Discord.ApiClient.RemoveGuildBanAsync(this.Id, user_id, reason);

        /// <summary>
        /// Leaves this guild.
        /// </summary>
        /// <returns></returns>
        public Task LeaveAsync()
            => this.Discord.ApiClient.LeaveGuildAsync(Id);

        /// <summary>
        /// Gets the bans for this guild.
        /// </summary>
        /// <returns>Collection of bans in this guild.</returns>
        public Task<IReadOnlyList<DiscordBan>> GetBansAsync()
            => this.Discord.ApiClient.GetGuildBansAsync(Id);

        /// <summary>
        /// Creates a new text channel in this guild.
        /// </summary>
        /// <param name="name">Name of the new channel.</param>
        /// <param name="parent">Category to put this channel in.</param>
        /// <param name="topic">Topic of the channel.</param>
        /// <param name="overwrites">Permission overwrites for this channel.</param>
        /// <param name="nsfw">Whether the channel is to be flagged as not safe for work.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
        /// <returns>The newly-created channel.</returns>
        public Task<DiscordChannel> CreateTextChannelAsync(string name, DiscordChannel parent = null, Optional<string> topic = default, IEnumerable<DiscordOverwriteBuilder> overwrites = null, bool? nsfw = null, Optional<int?> perUserRateLimit = default, string reason = null)
            => this.CreateChannelAsync(name, ChannelType.Text, parent, topic, null, null, overwrites, nsfw, perUserRateLimit, reason);

        /// <summary>
        /// Creates a new channel category in this guild.
        /// </summary>
        /// <param name="name">Name of the new category.</param>
        /// <param name="overwrites">Permission overwrites for this category.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns>The newly-created channel category.</returns>
        public Task<DiscordChannel> CreateChannelCategoryAsync(string name, IEnumerable<DiscordOverwriteBuilder> overwrites = null, string reason = null)
            => this.CreateChannelAsync(name, ChannelType.Category, null, Optional.FromNoValue<string>(), null, null, overwrites, null, Optional.FromNoValue<int?>(), reason);

        /// <summary>
        /// Creates a new voice channel in this guild.
        /// </summary>
        /// <param name="name">Name of the new channel.</param>
        /// <param name="parent">Category to put this channel in.</param>
        /// <param name="bitrate">Bitrate of the channel.</param>
        /// <param name="user_limit">Maximum number of users in the channel.</param>
        /// <param name="overwrites">Permission overwrites for this channel.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns>The newly-created channel.</returns>
        public Task<DiscordChannel> CreateVoiceChannelAsync(string name, DiscordChannel parent = null, int? bitrate = null, int? user_limit = null, IEnumerable<DiscordOverwriteBuilder> overwrites = null, string reason = null)
            => this.CreateChannelAsync(name, ChannelType.Voice, parent, Optional.FromNoValue<string>(), bitrate, user_limit, overwrites, null, Optional.FromNoValue<int?>(), reason);

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
        /// <param name="reason">Reason for audit logs.</param>
        /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
        /// <returns>The newly-created channel.</returns>
        public Task<DiscordChannel> CreateChannelAsync(string name, ChannelType type, DiscordChannel parent = null, Optional<string> topic = default, int? bitrate = null, int? userLimit = null, IEnumerable<DiscordOverwriteBuilder> overwrites = null, bool? nsfw = null, Optional<int?> perUserRateLimit = default, string reason = null)
        {
            // technically you can create news/store channels but not always
            if (type != ChannelType.Text && type != ChannelType.Voice && type != ChannelType.Category && type != ChannelType.News && type != ChannelType.Store)
                throw new ArgumentException("Channel type must be text, voice, or category.", nameof(type));

            if (type == ChannelType.Category && parent != null)
                throw new ArgumentException("Cannot specify parent of a channel category.", nameof(parent));

            return this.Discord.ApiClient.CreateGuildChannelAsync(this.Id, name, type, parent?.Id, topic, bitrate, userLimit, overwrites, nsfw, perUserRateLimit, reason);
        }

        // this is to commemorate the Great DAPI Channel Massacre of 2017-11-19.
        /// <summary>
        /// <para>Deletes all channels in this guild.</para>
        /// <para>Note that this is irreversible. Use carefully!</para>
        /// </summary>
        /// <returns></returns>
        public Task DeleteAllChannelsAsync()
        {
            var tasks = this.Channels.Values.Select(xc => xc.DeleteAsync());
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Estimates the number of users to be pruned.
        /// </summary>
        /// <param name="days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
        /// <param name="includedRoles">The roles to be included in the prune.</param>
        /// <returns>Number of users that will be pruned.</returns>
        public Task<int> GetPruneCountAsync(int days = 7, IEnumerable<DiscordRole> includedRoles = null)
        {
            if (includedRoles != null)
            {
                includedRoles = includedRoles.Where(r => r != null);
                var roleCount = includedRoles.Count();
                var roleArr = includedRoles.ToArray();
                var rawRoleIds = new List<ulong>();

                for (int i = 0; i < roleCount; i++)
                {
                    if (this._roles.ContainsKey(roleArr[i].Id))
                        rawRoleIds.Add(roleArr[i].Id);
                }

                return this.Discord.ApiClient.GetGuildPruneCountAsync(this.Id, days, rawRoleIds);
            }

            return this.Discord.ApiClient.GetGuildPruneCountAsync(this.Id, days, null);
        }

        /// <summary>
        /// Prunes inactive users from this guild.
        /// </summary>
        /// <param name="days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
        /// <param name="computePruneCount">Whether to return the prune count after this method completes. This is discouraged for larger guilds.</param>
        /// <param name="includedRoles">The roles to be included in the prune.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns>Number of users pruned.</returns>
        public Task<int?> PruneAsync(int days = 7, bool computePruneCount = true, IEnumerable<DiscordRole> includedRoles = null, string reason = null)
        {
            if (includedRoles != null)
            {
                includedRoles = includedRoles.Where(r => r != null);
                var roleCount = includedRoles.Count();
                var roleArr = includedRoles.ToArray();
                var rawRoleIds = new List<ulong>();

                for (int i = 0; i < roleCount; i++)
                {
                    if (this._roles.ContainsKey(roleArr[i].Id))
                        rawRoleIds.Add(roleArr[i].Id);
                }

                return this.Discord.ApiClient.BeginGuildPruneAsync(this.Id, days, computePruneCount, rawRoleIds, reason);
            }

            return this.Discord.ApiClient.BeginGuildPruneAsync(this.Id, days, computePruneCount, null, reason);
        }

        /// <summary>
        /// Gets integrations attached to this guild.
        /// </summary>
        /// <returns>Collection of integrations attached to this guild.</returns>
        public Task<IReadOnlyList<DiscordIntegration>> GetIntegrationsAsync()
            => this.Discord.ApiClient.GetGuildIntegrationsAsync(this.Id);

        /// <summary>
        /// Attaches an integration from current user to this guild.
        /// </summary>
        /// <param name="integration">Integration to attach.</param>
        /// <returns>The integration after being attached to the guild.</returns>
        public Task<DiscordIntegration> AttachUserIntegrationAsync(DiscordIntegration integration)
            => this.Discord.ApiClient.CreateGuildIntegrationAsync(Id, integration.Type, integration.Id);

        /// <summary>
        /// Modifies an integration in this guild.
        /// </summary>
        /// <param name="integration">Integration to modify.</param>
        /// <param name="expire_behaviour">Number of days after which the integration expires.</param>
        /// <param name="expire_grace_period">Length of grace period which allows for renewing the integration.</param>
        /// <param name="enable_emoticons">Whether emotes should be synced from this integration.</param>
        /// <returns>The modified integration.</returns>
        public Task<DiscordIntegration> ModifyIntegrationAsync(DiscordIntegration integration, int expire_behaviour, int expire_grace_period, bool enable_emoticons)
            => this.Discord.ApiClient.ModifyGuildIntegrationAsync(Id, integration.Id, expire_behaviour, expire_grace_period, enable_emoticons);

        /// <summary>
        /// Removes an integration from this guild.
        /// </summary>
        /// <param name="integration">Integration to remove.</param>
        /// <returns></returns>
        public Task DeleteIntegrationAsync(DiscordIntegration integration)
            => this.Discord.ApiClient.DeleteGuildIntegrationAsync(Id, integration);

        /// <summary>
        /// Forces re-synchronization of an integration for this guild.
        /// </summary>
        /// <param name="integration">Integration to synchronize.</param>
        /// <returns></returns>
        public Task SyncIntegrationAsync(DiscordIntegration integration)
            => this.Discord.ApiClient.SyncGuildIntegrationAsync(Id, integration.Id);

        /// <summary>
        /// Gets the guild widget.
        /// </summary>
        /// <returns>This guild's widget.</returns>
        public Task<DiscordGuildEmbed> GetEmbedAsync()
            => this.Discord.ApiClient.GetGuildEmbedAsync(Id);

        /// <summary>
        /// Gets the voice regions for this guild.
        /// </summary>
        /// <returns>Voice regions available for this guild.</returns>
        public async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
        {
            var vrs = await this.Discord.ApiClient.GetGuildVoiceRegionsAsync(this.Id).ConfigureAwait(false);
            foreach (var xvr in vrs)
                this.Discord.InternalVoiceRegions.TryAdd(xvr.Id, xvr);

            return vrs;
        }

        /// <summary>
        /// Gets an invite from this guild from an invite code. 
        /// </summary>
        /// <param name="code">The invite code</param>
        /// <returns>An invite, or null if not in cache.</returns>
        public DiscordInvite GetInvite(string code)
            => this._invites.TryGetValue(code, out var invite) ? invite : null;

        /// <summary>
        /// Gets all the invites created for all the channels in this guild.
        /// </summary>
        /// <returns>A collection of invites.</returns>
        public async Task<IReadOnlyList<DiscordInvite>> GetInvitesAsync()
        {
            var res = await this.Discord.ApiClient.GetGuildInvitesAsync(this.Id).ConfigureAwait(false);

            var intents = this.Discord.Configuration.Intents;

            if (!intents.HasValue || (intents.HasValue && intents.Value.HasIntent(DiscordIntents.GuildInvites)))
            {
                for (var i = 0; i < res.Count; i++)
                    this._invites[res[i].Code] = res[i];
            }

            return res;
        }

        /// <summary>
        /// Gets the vanity invite for this guild.
        /// </summary>
        /// <returns>A partial vanity invite.</returns>
        public Task<DiscordInvite> GetVanityInviteAsync()
            => this.Discord.ApiClient.GetGuildVanityUrlAsync(this.Id);


        /// <summary>
        /// Gets all the webhooks created for all the channels in this guild.
        /// </summary>
        /// <returns>A collection of webhooks this guild has.</returns>
        public Task<IReadOnlyList<DiscordWebhook>> GetWebhooksAsync()
            => this.Discord.ApiClient.GetGuildWebhooksAsync(this.Id);

        /// <summary>
        /// Gets this guild's widget image.
        /// </summary>
        /// <param name="bannerType">The format of the widget.</param>
        /// <returns>The URL of the widget image.</returns>
        public string GetWidgetImage(WidgetType bannerType = WidgetType.Shield)
        {
            string param;

            switch (bannerType)
            {
                case WidgetType.Banner1:
                    param = "banner1";
                    break;

                case WidgetType.Banner2:
                    param = "banner2";
                    break;

                case WidgetType.Banner3:
                    param = "banner3";
                    break;

                case WidgetType.Banner4:
                    param = "banner4";
                    break;

                default:
                    param = "shield";
                    break;
            }

            return $"{Net.Endpoints.BASE_URI}{Net.Endpoints.GUILDS}/{this.Id}{Net.Endpoints.WIDGET_PNG}?style={param}";
        }

        /// <summary>
        /// Gets a member of this guild by his user ID.
        /// </summary>
        /// <param name="userId">ID of the member to get.</param>
        /// <returns>The requested member.</returns>
        public async Task<DiscordMember> GetMemberAsync(ulong userId)
        {
            if (this._members.TryGetValue(userId, out var mbr))
                return mbr;

            mbr = await this.Discord.ApiClient.GetGuildMemberAsync(Id, userId).ConfigureAwait(false);

            var intents = this.Discord.Configuration.Intents;

            if (!intents.HasValue || (intents.HasValue && intents.Value.HasIntent(DiscordIntents.GuildMembers)))
                this._members[userId] = mbr;

            return mbr;
        }

        /// <summary>
        /// Retrieves a full list of members from Discord. This method will bypass cache.
        /// </summary>
        /// <returns>A collection of all members in this guild.</returns>
        public async Task<IReadOnlyCollection<DiscordMember>> GetAllMembersAsync()
        {
            var recmbr = new HashSet<DiscordMember>();

            var recd = 1000;
            var last = 0ul;
            while (recd > 0)
            {
                var tms = await this.Discord.ApiClient.ListGuildMembersAsync(this.Id, 1000, last == 0 ? null : (ulong?)last).ConfigureAwait(false);
                recd = tms.Count;

                foreach (var xtm in tms)
                {
                    var usr = new DiscordUser(xtm.User) { Discord = this.Discord };

                    var intents = this.Discord.Configuration.Intents;

                    if (!intents.HasValue || (intents.HasValue && intents.Value.HasIntent(DiscordIntents.GuildMembers)))
                    {
                        usr = this.Discord.UserCache.AddOrUpdate(xtm.User.Id, usr, (id, old) =>
                        {
                            old.Username = usr.Username;
                            old.Discord = usr.Discord;
                            old.AvatarHash = usr.AvatarHash;

                            return old;
                        });
                    }

                    recmbr.Add(new DiscordMember(xtm) { Discord = this.Discord, _guild_id = this.Id });
                }

                var tm = tms.LastOrDefault();
                last = tm?.User.Id ?? 0;
            }

            return new ReadOnlySet<DiscordMember>(recmbr);
        }

        /// <summary>
        /// Requests that Discord send a list of guild members based on the specified arguments. This method will fire the <see cref="DiscordClient.GuildMembersChunked"/> event.
        /// <para>If no arguments aside from <paramref name="presences"/> and <paramref name="nonce"/> are specified, this will request all guild members.</para>
        /// </summary>
        /// <param name="query">Filters the returned members based on what the username starts with. Either this or <paramref name="userIds"/> must not be null. 
        /// The <paramref name="limit"/> must also be greater than 0 if this is specified.</param>
        /// <param name="limit">Total number of members to request. This must be greater than 0 if <paramref name="query"/> is specified.</param>
        /// <param name="presences">Whether to include the <see cref="EventArgs.GuildMembersChunkEventArgs.Presences"/> associated with the fetched members.</param>
        /// <param name="userIds">Whether to limit the request to the specified user ids. Either this or <paramref name="query"/> must not be null.</param>
        /// <param name="nonce">The unique string to identify the response.</param>
        public async Task RequestMembersAsync(string query = "", int limit = 0, bool? presences = null, IEnumerable<ulong> userIds = null, string nonce = null)
        {
            if (!(this.Discord is DiscordClient client))
                throw new InvalidOperationException("This operation is only valid for regular Discord clients.");

            if (query == null && userIds == null)
                throw new ArgumentException("The query and user IDs cannot both be null.");

            if (query != null && userIds != null)
                query = null;

            var grgm = new GatewayRequestGuildMembers(this)
            {
                Query = query,
                Limit = limit >= 0 ? limit : 0,
                Presences = presences,
                UserIds = userIds,
                Nonce = nonce
            };

            var payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.RequestGuildMembers,
                Data = grgm
            };

            var payloadStr = JsonConvert.SerializeObject(payload, Formatting.None);
            await client.WsSendAsync(payloadStr).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all the channels this guild has.
        /// </summary>
        /// <returns>A collection of this guild's channels.</returns>
        public Task<IReadOnlyList<DiscordChannel>> GetChannelsAsync()
            => this.Discord.ApiClient.GetGuildChannelsAsync(this.Id);

        /// <summary>
        /// Creates a new role in this guild.
        /// </summary>
        /// <param name="name">Name of the role.</param>
        /// <param name="permissions">Permissions for the role.</param>
        /// <param name="color">Color for the role.</param>
        /// <param name="hoist">Whether the role is to be hoisted.</param>
        /// <param name="mentionable">Whether the role is to be mentionable.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns>The newly-created role.</returns>
        public Task<DiscordRole> CreateRoleAsync(string name = null, Permissions? permissions = null, DiscordColor? color = null, bool? hoist = null, bool? mentionable = null, string reason = null)
            => this.Discord.ApiClient.CreateGuildRoleAsync(this.Id, name, permissions, color?.Value, hoist, mentionable, reason);

        /// <summary>
        /// Gets a role from this guild by its ID.
        /// </summary>
        /// <param name="id">ID of the role to get.</param>
        /// <returns>Requested role.</returns>
        public DiscordRole GetRole(ulong id)
            => this._roles.TryGetValue(id, out var role) ? role : null;

        /// <summary>
        /// Gets a channel from this guild by its ID.
        /// </summary>
        /// <param name="id">ID of the channel to get.</param>
        /// <returns>Requested channel.</returns>
        public DiscordChannel GetChannel(ulong id)
            => this._channels.TryGetValue(id, out var channel) ? channel : null;

        /// <summary>
        /// Gets audit log entries for this guild.
        /// </summary>
        /// <param name="limit">Maximum number of entries to fetch.</param>
        /// <param name="by_member">Filter by member responsible.</param>
        /// <param name="action_type">Filter by action type.</param>
        /// <returns>A collection of requested audit log entries.</returns>
        public async Task<IReadOnlyList<DiscordAuditLogEntry>> GetAuditLogsAsync(int? limit = null, DiscordMember by_member = null, AuditLogActionType? action_type = null)
        {
            var alrs = new List<AuditLog>();
            int ac = 1, tc = 0, rmn = 100;
            var last = 0ul;
            while (ac > 0)
            {
                rmn = limit != null ? limit.Value - tc : 100;
                rmn = Math.Min(100, rmn);
                if (rmn <= 0) break;

                var alr = await this.Discord.ApiClient.GetAuditLogsAsync(this.Id, rmn, null, last == 0 ? null : (ulong?)last, by_member?.Id, (int?)action_type).ConfigureAwait(false);
                ac = alr.Entries.Count();
                tc += ac;
                if (ac > 0)
                {
                    last = alr.Entries.Last().Id;
                    alrs.Add(alr);
                }
            }

            var amr = alrs.SelectMany(xa => xa.Users)
                .GroupBy(xu => xu.Id)
                .Select(xgu => xgu.First());

            foreach (var xau in amr)
            {
                if (this.Discord.UserCache.ContainsKey(xau.Id))
                    continue;

                var xtu = new TransportUser
                {
                    Id = xau.Id,
                    Username = xau.Username,
                    Discriminator = xau.Discriminator,
                    AvatarHash = xau.AvatarHash
                };
                var xu = new DiscordUser(xtu) { Discord = this.Discord };
                xu = this.Discord.UserCache.AddOrUpdate(xu.Id, xu, (id, old) =>
                {
                    old.Username = xu.Username;
                    old.Discriminator = xu.Discriminator;
                    old.AvatarHash = xu.AvatarHash;
                    return old;
                });
            }

            var ahr = alrs.SelectMany(xa => xa.Webhooks)
                .GroupBy(xh => xh.Id)
                .Select(xgh => xgh.First());

            var ams = amr.Select(xau => this._members.TryGetValue(xau.Id, out var member) ? member : new DiscordMember { Discord = this.Discord, Id = xau.Id, _guild_id = this.Id });
            var amd = ams.ToDictionary(xm => xm.Id, xm => xm);

            Dictionary<ulong, DiscordWebhook> ahd = null;
            if (ahr.Any())
            {
                var whr = await this.GetWebhooksAsync().ConfigureAwait(false);
                var whs = whr.ToDictionary(xh => xh.Id, xh => xh);

                var amh = ahr.Select(xah => whs.TryGetValue(xah.Id, out var webhook) ? webhook : new DiscordWebhook { Discord = this.Discord, Name = xah.Name, Id = xah.Id, AvatarHash = xah.AvatarHash, ChannelId = xah.ChannelId, GuildId = xah.GuildId, Token = xah.Token });
                ahd = amh.ToDictionary(xh => xh.Id, xh => xh);
            }

            var acs = alrs.SelectMany(xa => xa.Entries).OrderByDescending(xa => xa.Id);
            var entries = new List<DiscordAuditLogEntry>();
            foreach (var xac in acs)
            {
                DiscordAuditLogEntry entry = null;
                ulong t1, t2;
                int t3, t4;
                bool p1, p2;
                switch (xac.ActionType)
                {
                    case AuditLogActionType.GuildUpdate:
                        entry = new DiscordAuditLogGuildEntry
                        {
                            Target = this
                        };

                        var entrygld = entry as DiscordAuditLogGuildEntry;
                        foreach (var xc in xac.Changes)
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
                                        Before = this._members.TryGetValue(xc.OldValueUlong, out var oldMember) ? oldMember : await this.GetMemberAsync(xc.OldValueUlong).ConfigureAwait(false),
                                        After = this._members.TryGetValue(xc.NewValueUlong, out var newMember) ? newMember : await this.GetMemberAsync(xc.NewValueUlong).ConfigureAwait(false)
                                    };
                                    break;

                                case "icon_hash":
                                    entrygld.IconChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString != null ? $"https://cdn.discordapp.com/icons/{this.Id}/{xc.OldValueString}.webp" : null,
                                        After = xc.OldValueString != null ? $"https://cdn.discordapp.com/icons/{this.Id}/{xc.NewValueString}.webp" : null
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
                                        Before = this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id },
                                        After = this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id }
                                    };
                                    break;

                                case "widget_channel_id":
                                    ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entrygld.EmbedChannelChange = new PropertyChange<DiscordChannel>
                                    {
                                        Before = this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id },
                                        After = this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id }
                                    };
                                    break;

                                case "splash_hash":
                                    entrygld.SplashChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString != null ? $"https://cdn.discordapp.com/splashes/{this.Id}/{xc.OldValueString}.webp?size=2048" : null,
                                        After = xc.NewValueString != null ? $"https://cdn.discordapp.com/splashes/{this.Id}/{xc.NewValueString}.webp?size=2048" : null
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
                                        Before = this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id },
                                        After = this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id }
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
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in guild update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.ChannelCreate:
                    case AuditLogActionType.ChannelDelete:
                    case AuditLogActionType.ChannelUpdate:
                        entry = new DiscordAuditLogChannelEntry
                        {
                            Target = this.GetChannel(xac.TargetId.Value) ?? new DiscordChannel { Id = xac.TargetId.Value, Discord = this.Discord, GuildId = this.Id }
                        };

                        var entrychn = entry as DiscordAuditLogChannelEntry;
                        foreach (var xc in xac.Changes)
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
                                    var olds = xc.OldValues?.OfType<JObject>()
                                        ?.Select(xjo => xjo.ToObject<DiscordOverwrite>())
                                        ?.Select(xo => { xo.Discord = this.Discord; return xo; });

                                    var news = xc.NewValues?.OfType<JObject>()
                                        ?.Select(xjo => xjo.ToObject<DiscordOverwrite>())
                                        ?.Select(xo => { xo.Discord = this.Discord; return xo; });

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
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in channel update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.OverwriteCreate:
                    case AuditLogActionType.OverwriteDelete:
                    case AuditLogActionType.OverwriteUpdate:
                        entry = new DiscordAuditLogOverwriteEntry
                        {
                            Target = this.GetChannel(xac.TargetId.Value)?.PermissionOverwrites.FirstOrDefault(xo => xo.Id == xac.Options.Id),
                            Channel = this.GetChannel(xac.TargetId.Value)
                        };

                        var entryovr = entry as DiscordAuditLogOverwriteEntry;
                        foreach (var xc in xac.Changes)
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
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in overwrite update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.Kick:
                        entry = new DiscordAuditLogKickEntry
                        {
                            Target = amd.TryGetValue(xac.TargetId.Value, out var kickMember) ? kickMember : new DiscordMember { Id = xac.TargetId.Value, Discord = this.Discord, _guild_id = this.Id }
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
                            Target = amd.TryGetValue(xac.TargetId.Value, out var unbanMember) ? unbanMember : new DiscordMember { Id = xac.TargetId.Value, Discord = this.Discord, _guild_id = this.Id }
                        };
                        break;

                    case AuditLogActionType.MemberUpdate:
                    case AuditLogActionType.MemberRoleUpdate:
                        entry = new DiscordAuditLogMemberUpdateEntry
                        {
                            Target = amd.TryGetValue(xac.TargetId.Value, out var roleUpdMember) ? roleUpdMember : new DiscordMember { Id = xac.TargetId.Value, Discord = this.Discord, _guild_id = this.Id }
                        };

                        var entrymbu = entry as DiscordAuditLogMemberUpdateEntry;
                        foreach (var xc in xac.Changes)
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

                                case "$add":
                                    entrymbu.AddedRoles = new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"]).Select(this.GetRole).ToList());
                                    break;

                                case "$remove":
                                    entrymbu.RemovedRoles = new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"]).Select(this.GetRole).ToList());
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in member update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.RoleCreate:
                    case AuditLogActionType.RoleDelete:
                    case AuditLogActionType.RoleUpdate:
                        entry = new DiscordAuditLogRoleUpdateEntry
                        {
                            Target = this.GetRole(xac.TargetId.Value) ?? new DiscordRole { Id = xac.TargetId.Value, Discord = this.Discord }
                        };

                        var entryrol = entry as DiscordAuditLogRoleUpdateEntry;
                        foreach (var xc in xac.Changes)
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
                                        Before = xc.OldValue != null ? (Permissions?)(long)xc.OldValue : null,
                                        After = xc.NewValue != null ? (Permissions?)(long)xc.NewValue : null
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
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in role update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.InviteCreate:
                    case AuditLogActionType.InviteDelete:
                    case AuditLogActionType.InviteUpdate:
                        entry = new DiscordAuditLogInviteEntry();

                        var inv = new DiscordInvite
                        {
                            Discord = this.Discord,
                            Guild = new DiscordInviteGuild
                            {
                                Discord = this.Discord,
                                Id = this.Id,
                                Name = this.Name,
                                SplashHash = this.SplashHash
                            }
                        };

                        var entryinv = entry as DiscordAuditLogInviteEntry;
                        foreach (var xc in xac.Changes)
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
                                        Before = amd.TryGetValue(t1, out var propBeforeMember) ? propBeforeMember : new DiscordMember { Id = t1, Discord = this.Discord, _guild_id = this.Id },
                                        After = amd.TryGetValue(t2, out var propAfterMember) ? propAfterMember : new DiscordMember { Id = t1, Discord = this.Discord, _guild_id = this.Id },
                                    };
                                    break;

                                case "channel_id":
                                    p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entryinv.ChannelChange = new PropertyChange<DiscordChannel>
                                    {
                                        Before = p1 ? this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null,
                                        After = p2 ? this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null
                                    };

                                    var ch = entryinv.ChannelChange.Before ?? entryinv.ChannelChange.After;
                                    var cht = ch?.Type;
                                    inv.Channel = new DiscordInviteChannel
                                    {
                                        Discord = this.Discord,
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
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in invite update: {0} - this should be reported to library developers", xc.Key);
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
                            Target = ahd.TryGetValue(xac.TargetId.Value, out var webhook) ? webhook : new DiscordWebhook { Id = xac.TargetId.Value, Discord = this.Discord }
                        };

                        var entrywhk = entry as DiscordAuditLogWebhookEntry;
                        foreach (var xc in xac.Changes)
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
                                        Before = p1 ? this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null,
                                        After = p2 ? this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null
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

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in webhook update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.EmojiCreate:
                    case AuditLogActionType.EmojiDelete:
                    case AuditLogActionType.EmojiUpdate:
                        entry = new DiscordAuditLogEmojiEntry
                        {
                            Target = this._emojis.TryGetValue(xac.TargetId.Value, out var target) ? target : new DiscordEmoji { Id = xac.TargetId.Value, Discord = this.Discord }
                        };

                        var entryemo = entry as DiscordAuditLogEmojiEntry;
                        foreach (var xc in xac.Changes)
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
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in emote update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.MessageDelete:
                    case AuditLogActionType.MessageBulkDelete:
                        {
                            entry = new DiscordAuditLogMessageEntry();

                            var entrymsg = entry as DiscordAuditLogMessageEntry;

                            if (xac.Options != null)
                            {
                                entrymsg.Channel = this.GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = this.Discord, GuildId = this.Id };
                                entrymsg.MessageCount = xac.Options.Count;
                            }

                            if (entrymsg.Channel != null)
                            {
                                if (this.Discord is DiscordClient dc
                                    && dc.MessageCache != null
                                    && dc.MessageCache.TryGet(xm => xm.Id == xac.TargetId.Value && xm.ChannelId == entrymsg.Channel.Id, out var msg))
                                {
                                    entrymsg.Target = msg;
                                }
                                else
                                {
                                    entrymsg.Target = new DiscordMessage { Discord = this.Discord, Id = xac.TargetId.Value };
                                }
                            }
                            break;
                        }

                    case AuditLogActionType.MessagePin:
                    case AuditLogActionType.MessageUnpin:
                        {
                            entry = new DiscordAuditLogMessagePinEntry();

                            var entrypin = entry as DiscordAuditLogMessagePinEntry;

                            if (!(this.Discord is DiscordClient dc))
                            {
                                break;
                            }

                            if (xac.Options != null)
                            {
                                DiscordMessage message = default;
                                dc.MessageCache?.TryGet(x => x.Id == xac.Options.MessageId && x.ChannelId == xac.Options.ChannelId, out message);

                                entrypin.Channel = this.GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = this.Discord, GuildId = this.Id };
                                entrypin.Message = message ?? new DiscordMessage { Id = xac.Options.MessageId, Discord = this.Discord };
                            }

                            if (xac.TargetId.HasValue)
                            {
                                dc.UserCache.TryGetValue(xac.TargetId.Value, out var user);
                                entrypin.Target = user ?? new DiscordUser { Id = user.Id, Discord = this.Discord };
                            }

                            break;
                        }

                    case AuditLogActionType.BotAdd:
                        {
                            entry = new DiscordAuditLogBotAddEntry();

                            if (!(this.Discord is DiscordClient dc && xac.TargetId.HasValue))
                            {
                                break;
                            }

                            dc.UserCache.TryGetValue(xac.TargetId.Value, out var bot);
                            (entry as DiscordAuditLogBotAddEntry).TargetBot = bot ?? new DiscordUser { Id = xac.TargetId.Value, Discord = this.Discord };

                            break;
                        }

                    case AuditLogActionType.MemberMove:
                        entry = new DiscordAuditLogMemberMoveEntry();

                        if (xac.Options == null)
                        {
                            break;
                        }

                        var moveentry = entry as DiscordAuditLogMemberMoveEntry;

                        moveentry.UserCount = xac.Options.Count;
                        moveentry.Channel = this.GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = this.Discord, GuildId = this.Id };
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

                        var integentry = entry as DiscordAuditLogIntegrationEntry;
                        foreach (var xc in xac.Changes)
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
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in integration update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    default:
                        this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown audit log action type: {0} - this should be reported to library developers", (int)xac.ActionType);
                        break;
                }

                if (entry == null)
                    continue;

                switch (xac.ActionType)
                {
                    case AuditLogActionType.ChannelCreate:
                    case AuditLogActionType.EmojiCreate:
                    case AuditLogActionType.InviteCreate:
                    case AuditLogActionType.OverwriteCreate:
                    case AuditLogActionType.RoleCreate:
                    case AuditLogActionType.WebhookCreate:
                    case AuditLogActionType.IntegrationCreate:
                        entry.ActionCategory = AuditLogActionCategory.Create;
                        break;

                    case AuditLogActionType.ChannelDelete:
                    case AuditLogActionType.EmojiDelete:
                    case AuditLogActionType.InviteDelete:
                    case AuditLogActionType.MessageDelete:
                    case AuditLogActionType.MessageBulkDelete:
                    case AuditLogActionType.OverwriteDelete:
                    case AuditLogActionType.RoleDelete:
                    case AuditLogActionType.WebhookDelete:
                    case AuditLogActionType.IntegrationDelete:
                        entry.ActionCategory = AuditLogActionCategory.Delete;
                        break;

                    case AuditLogActionType.ChannelUpdate:
                    case AuditLogActionType.EmojiUpdate:
                    case AuditLogActionType.InviteUpdate:
                    case AuditLogActionType.MemberRoleUpdate:
                    case AuditLogActionType.MemberUpdate:
                    case AuditLogActionType.OverwriteUpdate:
                    case AuditLogActionType.RoleUpdate:
                    case AuditLogActionType.WebhookUpdate:
                    case AuditLogActionType.IntegrationUpdate:
                        entry.ActionCategory = AuditLogActionCategory.Update;
                        break;

                    default:
                        entry.ActionCategory = AuditLogActionCategory.Other;
                        break;
                }

                entry.Discord = this.Discord;
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
        public Task<IReadOnlyList<DiscordGuildEmoji>> GetEmojisAsync()
            => this.Discord.ApiClient.GetGuildEmojisAsync(this.Id);

        /// <summary>
        /// Gets this guild's specified custom emoji.
        /// </summary>
        /// <param name="id">ID of the emoji to get.</param>
        /// <returns>The requested custom emoji.</returns>
        public Task<DiscordGuildEmoji> GetEmojiAsync(ulong id)
            => this.Discord.ApiClient.GetGuildEmojiAsync(this.Id, id);

        /// <summary>
        /// Creates a new custom emoji for this guild.
        /// </summary>
        /// <param name="name">Name of the new emoji.</param>
        /// <param name="image">Image to use as the emoji.</param>
        /// <param name="roles">Roles for which the emoji will be available. This works only if your application is whitelisted as integration.</param>
        /// <param name="reason">Reason for audit log.</param>
        /// <returns>The newly-created emoji.</returns>
        public Task<DiscordGuildEmoji> CreateEmojiAsync(string name, Stream image, IEnumerable<DiscordRole> roles = null, string reason = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            name = name.Trim();
            if (name.Length < 2 || name.Length > 50)
                throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.");

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            string image64 = null;
            using (var imgtool = new ImageTool(image))
                image64 = imgtool.GetBase64();

            return this.Discord.ApiClient.CreateGuildEmojiAsync(this.Id, name, image64, roles?.Select(xr => xr.Id), reason);
        }

        /// <summary>
        /// Modifies a this guild's custom emoji.
        /// </summary>
        /// <param name="emoji">Emoji to modify.</param>
        /// <param name="name">New name for the emoji.</param>
        /// <param name="roles">Roles for which the emoji will be available. This works only if your application is whitelisted as integration.</param>
        /// <param name="reason">Reason for audit log.</param>
        /// <returns>The modified emoji.</returns>
        public Task<DiscordGuildEmoji> ModifyEmojiAsync(DiscordGuildEmoji emoji, string name, IEnumerable<DiscordRole> roles = null, string reason = null)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            if (emoji.Guild.Id != this.Id)
                throw new ArgumentException("This emoji does not belong to this guild.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            name = name.Trim();
            if (name.Length < 2 || name.Length > 50)
                throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.");

            return this.Discord.ApiClient.ModifyGuildEmojiAsync(this.Id, emoji.Id, name, roles?.Select(xr => xr.Id), reason);
        }

        /// <summary>
        /// Deletes this guild's custom emoji.
        /// </summary>
        /// <param name="emoji">Emoji to delete.</param>
        /// <param name="reason">Reason for audit log.</param>
        /// <returns></returns>
        public Task DeleteEmojiAsync(DiscordGuildEmoji emoji, string reason = null)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            if (emoji.Guild.Id != this.Id)
                throw new ArgumentException("This emoji does not belong to this guild.");

            return this.Discord.ApiClient.DeleteGuildEmojiAsync(this.Id, emoji.Id, reason);
        }

        /// <summary>
        /// <para>Gets the default channel for this guild.</para>
        /// <para>Default channel is the first channel current member can see.</para>
        /// </summary>
        /// <returns>This member's default guild.</returns>
        public DiscordChannel GetDefaultChannel()
        {
            return this._channels.Values.Where(xc => xc.Type == ChannelType.Text)
                .OrderBy(xc => xc.Position)
                .FirstOrDefault(xc => (xc.PermissionsFor(this.CurrentMember) & DSharpPlus.Permissions.AccessChannels) == DSharpPlus.Permissions.AccessChannels);
        }
        #endregion

        /// <summary>
        /// Returns a string representation of this guild.
        /// </summary>
        /// <returns>String representation of this guild.</returns>
        public override string ToString()
        {
            return $"Guild {this.Id}; {this.Name}";
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordGuild"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordGuild"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordGuild);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordGuild"/> is equal to another <see cref="DiscordGuild"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordGuild"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordGuild"/> is equal to this <see cref="DiscordGuild"/>.</returns>
        public bool Equals(DiscordGuild e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordGuild"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordGuild"/>.</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordGuild"/> objects are equal.
        /// </summary>
        /// <param name="e1">First member to compare.</param>
        /// <param name="e2">Second member to compare.</param>
        /// <returns>Whether the two members are equal.</returns>
        public static bool operator ==(DiscordGuild e1, DiscordGuild e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            if (o1 == null && o2 == null)
                return true;

            return e1.Id == e2.Id;
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
}