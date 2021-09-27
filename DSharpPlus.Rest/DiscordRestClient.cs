// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;

namespace DSharpPlus
{
    public class DiscordRestClient : BaseDiscordClient
    {
        /// <summary>
        /// Gets the dictionary of guilds cached by this client.
        /// </summary>
        public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds
            => this._guilds_lazy.Value;

        internal Dictionary<ulong, DiscordGuild> _guilds = new();
        private Lazy<IReadOnlyDictionary<ulong, DiscordGuild>> _guilds_lazy;

        public DiscordRestClient(DiscordConfiguration config) : base(config)
        {
            this._disposed = false;
        }

        /// <summary>
        /// Initializes cache
        /// </summary>
        /// <returns></returns>
        public async Task InitializeCacheAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);
            this._guilds_lazy = new Lazy<IReadOnlyDictionary<ulong, DiscordGuild>>(() => new ReadOnlyDictionary<ulong, DiscordGuild>(this._guilds));
            var gs = await this.ApiClient.GetCurrentUserGuildsAsync(100, null, null).ConfigureAwait(false);
            foreach (var g in gs)
            {
                this._guilds[g.Id] = g;
            }
        }

        #region Guild

        /// <summary>
        /// Searches the given guild for members who's display name start with the specified name.
        /// </summary>
        /// <param name="guild_id">The Id of the guild to search.</param>
        /// <param name="name">The name to search for.</param>
        /// <param name="limit">The maximum amount of members to return. Max 1000. Defaults to 1.</param>
        /// <returns>The members found, if any.</returns>
        public Task<IReadOnlyList<DiscordMember>> SearchMembersAsync(ulong guild_id, string name, int? limit = 1)
            => this.ApiClient.SearchMembersAsync(guild_id, name, limit);

        /// <summary>
        /// Creates a new guild
        /// </summary>
        /// <param name="name">New guild's name</param>
        /// <param name="region_id">New guild's region ID</param>
        /// <param name="iconb64">New guild's icon (base64)</param>
        /// <param name="verification_level">New guild's verification level</param>
        /// <param name="default_message_notifications">New guild's default message notification level</param>
        /// <param name="system_channel_flags">New guild's system channel flags</param>
        /// <returns></returns>
        public Task<DiscordGuild> CreateGuildAsync(string name, string region_id, string iconb64, VerificationLevel? verification_level, DefaultMessageNotifications? default_message_notifications, SystemChannelFlags? system_channel_flags)
            => this.ApiClient.CreateGuildAsync(name, region_id, iconb64, verification_level, default_message_notifications, system_channel_flags);

        /// <summary>
        /// Creates a guild from a template. This requires the bot to be in less than 10 guilds total.
        /// </summary>
        /// <param name="code">The template code.</param>
        /// <param name="name">Name of the guild.</param>
        /// <param name="icon">Stream containing the icon for the guild.</param>
        /// <returns>The created guild.</returns>
        public Task<DiscordGuild> CreateGuildFromTemplateAsync(string code, string name, string icon)
            => this.ApiClient.CreateGuildFromTemplateAsync(code, name, icon);

        /// <summary>
        /// Deletes a guild
        /// </summary>
        /// <param name="id">guild id</param>
        /// <returns></returns>
        public Task DeleteGuildAsync(ulong id)
            => this.ApiClient.DeleteGuildAsync(id);

        /// <summary>
        /// Modifies a guild
        /// </summary>
        /// <param name="guild_id">Guild ID</param>
        /// <param name="name">New guild Name</param>
        /// <param name="region">New guild voice region</param>
        /// <param name="verification_level">New guild verification level</param>
        /// <param name="default_message_notifications">New guild default message notification level</param>
        /// <param name="mfa_level">New guild MFA level</param>
        /// <param name="explicit_content_filter">New guild explicit content filter level</param>
        /// <param name="afk_channel_id">New guild AFK channel id</param>
        /// <param name="afk_timeout">New guild AFK timeout in seconds</param>
        /// <param name="iconb64">New guild icon (base64)</param>
        /// <param name="owner_id">New guild owner id</param>
        /// <param name="splashb64">New guild spalsh (base64)</param>
        /// <param name="systemChannelId">New guild system channel id</param>
        /// <param name="banner">New guild banner</param>
        /// <param name="description">New guild description</param>
        /// <param name="discorverySplash">New guild Discovery splash</param>
        /// <param name="features">List of new <see href="https://discord.com/developers/docs/resources/guild#guild-object-guild-features">guild features</see></param>
        /// <param name="preferredLocale">New preferred locale</param>
        /// <param name="publicUpdatesChannelId">New updates channel id</param>
        /// <param name="rulesChannelId">New rules channel id</param>
        /// <param name="systemChannelFlags">New system channel flags</param>
        /// <param name="reason">Modify reason</param>
        /// <returns></returns>
        public Task<DiscordGuild> ModifyGuildAsync(ulong guild_id, Optional<string> name,
            Optional<string> region, Optional<VerificationLevel> verification_level,
            Optional<DefaultMessageNotifications> default_message_notifications, Optional<MfaLevel> mfa_level,
            Optional<ExplicitContentFilter> explicit_content_filter, Optional<ulong?> afk_channel_id,
            Optional<int> afk_timeout, Optional<string> iconb64, Optional<ulong> owner_id, Optional<string> splashb64,
            Optional<ulong?> systemChannelId, Optional<string> banner, Optional<string> description,
            Optional<string> discorverySplash, Optional<IEnumerable<string>> features, Optional<string> preferredLocale,
            Optional<ulong?> publicUpdatesChannelId, Optional<ulong?> rulesChannelId, Optional<SystemChannelFlags> systemChannelFlags,
            string reason)
            => this.ApiClient.ModifyGuildAsync(guild_id, name, region, verification_level, default_message_notifications, mfa_level, explicit_content_filter, afk_channel_id, afk_timeout, iconb64,
                owner_id, splashb64, systemChannelId, banner, description, discorverySplash, features, preferredLocale, publicUpdatesChannelId, rulesChannelId, systemChannelFlags, reason);

        /// <summary>
        /// Modifies a guild
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="action">Guild modifications</param>
        /// <returns></returns>
        public async Task<DiscordGuild> ModifyGuildAsync(ulong guild_id, Action<GuildEditModel> action)
        {
            var mdl = new GuildEditModel();
            action(mdl);

            if (mdl.AfkChannel.HasValue)
                if (mdl.AfkChannel.Value.Type != ChannelType.Voice)
                    throw new ArgumentException("AFK channel needs to be a voice channel!");

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

            return await this.ApiClient.ModifyGuildAsync(guild_id, mdl.Name, mdl.Region.IfPresent(x => x.Id), mdl.VerificationLevel, mdl.DefaultMessageNotifications,
                mdl.MfaLevel, mdl.ExplicitContentFilter, mdl.AfkChannel.IfPresent(x => x?.Id), mdl.AfkTimeout, iconb64, mdl.Owner.IfPresent(x => x.Id),
                splashb64, mdl.SystemChannel.IfPresent(x => x?.Id), mdl.Banner, mdl.Description, mdl.DiscoverySplash, mdl.Features, mdl.PreferredLocale,
                mdl.PublicUpdatesChannel.IfPresent(e => e?.Id), mdl.RulesChannel.IfPresent(e => e?.Id), mdl.SystemChannelFlags, mdl.AuditLogReason).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets guild bans.
        /// </summary>
        /// <param name="guild_id">The Id of the guild to get the bans from.</param>
        /// <returns>A collection of the guild's bans.</returns>
        public Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guild_id)
            => this.ApiClient.GetGuildBansAsync(guild_id);

        /// <summary>
        /// Gets the ban of the specified user. Requires Ban Members permission.
        /// </summary>
        /// <param name="guild_id">The Id of the guild to get the ban from.</param>
        /// <param name="user_id">The Id of the user to get the ban for.</param>
        /// <returns>A guild ban object.</returns>
        public Task<DiscordBan> GetGuildBanAsync(ulong guild_id, ulong user_id)
            => this.ApiClient.GetGuildBanAsync(guild_id, user_id);

        /// <summary>
        /// Creates guild ban
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="delete_message_days">Days to delete messages</param>
        /// <param name="reason">Reason why this member was banned</param>
        /// <returns></returns>
        public Task CreateGuildBanAsync(ulong guild_id, ulong user_id, int delete_message_days, string reason)
            => this.ApiClient.CreateGuildBanAsync(guild_id, user_id, delete_message_days, reason);

        /// <summary>
        /// Removes a guild ban
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User to unban</param>
        /// <param name="reason">Reason why this member was unbanned</param>
        /// <returns></returns>
        public Task RemoveGuildBanAsync(ulong guild_id, ulong user_id, string reason)
            => this.ApiClient.RemoveGuildBanAsync(guild_id, user_id, reason);

        /// <summary>
        /// Leaves a guild
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public Task LeaveGuildAsync(ulong guild_id)
            => this.ApiClient.LeaveGuildAsync(guild_id);

        /// <summary>
        /// Adds a member to a guild
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="access_token">Access token</param>
        /// <param name="nick">User nickname</param>
        /// <param name="roles">User roles</param>
        /// <param name="muted">Whether this user should be muted on join</param>
        /// <param name="deafened">Whether this user should be deafened on join</param>
        /// <returns></returns>
        public Task<DiscordMember> AddGuildMemberAsync(ulong guild_id, ulong user_id, string access_token, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
            => this.ApiClient.AddGuildMemberAsync(guild_id, user_id, access_token, nick, roles, muted, deafened);

        /// <summary>
        /// Gets all guild members
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="limit">Member download limit</param>
        /// <param name="after">Gets members after this ID</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DiscordMember>> ListGuildMembersAsync(ulong guild_id, int? limit, ulong? after)
        {
            var recmbr = new List<DiscordMember>();

            var recd = limit ?? 1000;
            var lim = limit ?? 1000;
            var last = after;
            while (recd == lim)
            {
                var tms = await this.ApiClient.ListGuildMembersAsync(guild_id, lim, last == 0 ? null : (ulong?)last).ConfigureAwait(false);
                recd = tms.Count;

                foreach (var xtm in tms)
                {
                    last = xtm.User.Id;

                    if (this.UserCache.ContainsKey(xtm.User.Id))
                        continue;

                    var usr = new DiscordUser(xtm.User) { Discord = this };
                    this.UpdateUserCache(usr);
                }

                recmbr.AddRange(tms.Select(xtm => new DiscordMember(xtm) { Discord = this, _guild_id = guild_id }));
            }

            return new ReadOnlyCollection<DiscordMember>(recmbr);
        }

        /// <summary>
        /// Add role to guild member
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="role_id">Role id</param>
        /// <param name="reason">Reason this role gets added</param>
        /// <returns></returns>
        public Task AddGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
            => this.ApiClient.AddGuildMemberRoleAsync(guild_id, user_id, role_id, reason);

        /// <summary>
        /// Remove role from member
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="role_id">Role id</param>
        /// <param name="reason">Reason this role gets removed</param>
        /// <returns></returns>
        public Task RemoveGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
            => this.ApiClient.RemoveGuildMemberRoleAsync(guild_id, user_id, role_id, reason);

        /// <summary>
        /// Updates a role's position
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="role_id">Role id</param>
        /// <param name="position">Role position</param>
        /// <param name="reason">Reason this position was modified</param>
        /// <returns></returns>
        public Task UpdateRolePositionAsync(ulong guild_id, ulong role_id, int position, string reason = null)
        {
            var rgrrps = new List<RestGuildRoleReorderPayload>()
            {
                new RestGuildRoleReorderPayload { RoleId = role_id }
            };
            return this.ApiClient.ModifyGuildRolePositionAsync(guild_id, rgrrps, reason);
        }

        /// <summary>
        /// Updates a channel's position
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="channel_id">Channel id</param>
        /// <param name="position">Channel position</param>
        /// <param name="reason">Reason this position was modified</param>
        /// <param name="lockPermissions">Whether to sync channel permissions with the parent, if moving to a new category.</param>
        /// <param name="parentId">The new parent id if the channel is to be moved to a new category.</param>
        /// <returns></returns>
        public Task UpdateChannelPositionAsync(ulong guild_id, ulong channel_id, int position, string reason, bool? lockPermissions = null, ulong? parentId = null)
        {
            var rgcrps = new List<RestGuildChannelReorderPayload>()
            {
                new RestGuildChannelReorderPayload { ChannelId = channel_id, Position = position, LockPermissions = lockPermissions, ParentId = parentId }
            };
            return this.ApiClient.ModifyGuildChannelPositionAsync(guild_id, rgcrps, reason);
        }

        /// <summary>
        /// Gets a guild's widget
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public Task<DiscordWidget> GetGuildWidgetAsync(ulong guild_id)
            => this.ApiClient.GetGuildWidgetAsync(guild_id);

        /// <summary>
        /// Gets a guild's widget settings
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public Task<DiscordWidgetSettings> GetGuildWidgetSettingsAsync(ulong guild_id)
            => this.ApiClient.GetGuildWidgetSettingsAsync(guild_id);

        /// <summary>
        /// Modifies a guild's widget settings
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="enabled">If the widget is enabled or not</param>
        /// <param name="channel_id">Widget channel id</param>
        /// <param name="reason">Reason the widget settings were modified</param>
        /// <returns></returns>
        public Task<DiscordWidgetSettings> ModifyGuildWidgetSettingsAsync(ulong guild_id, bool? enabled = null, ulong? channel_id = null, string reason = null)
            => this.ApiClient.ModifyGuildWidgetSettingsAsync(guild_id, enabled, channel_id, reason);

        /// <summary>
        /// Gets a guild's membership screening form.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns>The guild's membership screening form.</returns>
        public Task<DiscordGuildMembershipScreening> GetGuildMembershipScreeningFormAsync(ulong guild_id)
            => this.ApiClient.GetGuildMembershipScreeningFormAsync(guild_id);

        /// <summary>
        /// Modifies a guild's membership screening form.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="action">Action to perform</param>
        /// <returns>The modified screening form.</returns>
        public async Task<DiscordGuildMembershipScreening> ModifyGuildMembershipScreeningFormAsync(ulong guild_id, Action<MembershipScreeningEditModel> action)
        {
            var mdl = new MembershipScreeningEditModel();
            action(mdl);
            return await this.ApiClient.ModifyGuildMembershipScreeningFormAsync(guild_id, mdl.Enabled, mdl.Fields, mdl.Description).ConfigureAwait(false);
        }
        #endregion

        #region Channel
        /// <summary>
        /// Creates a guild channel
        /// </summary>
        /// <param name="id">Channel id</param>
        /// <param name="name">Channel name</param>
        /// <param name="type">Channel type</param>
        /// <param name="parent">Channel parent id</param>
        /// <param name="topic">Channel topic</param>
        /// <param name="bitrate">Voice channel bitrate</param>
        /// <param name="userLimit">Voice channel user limit</param>
        /// <param name="overwrites">Channel overwrites</param>
        /// <param name="nsfw">Whether this channel should be marked as NSFW</param>
        /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
        /// <param name="qualityMode">Voice channel video quality mode.</param>
        /// <param name="reason">Reason this channel was created</param>
        /// <returns></returns>
        public Task<DiscordChannel> CreateGuildChannelAsync(ulong id, string name, ChannelType type, ulong? parent, Optional<string> topic, int? bitrate, int? userLimit, IEnumerable<DiscordOverwriteBuilder> overwrites, bool? nsfw, Optional<int?> perUserRateLimit, VideoQualityMode? qualityMode, string reason)
        {
            return type != ChannelType.Category && type != ChannelType.Text && type != ChannelType.Voice && type != ChannelType.News && type != ChannelType.Store && type != ChannelType.Stage
                ? throw new ArgumentException("Channel type must be text, voice, stage, or category.", nameof(type))
                : this.ApiClient.CreateGuildChannelAsync(id, name, type, parent, topic, bitrate, userLimit, overwrites, nsfw, perUserRateLimit, qualityMode, reason);
        }

        /// <summary>
        /// Modifies a channel
        /// </summary>
        /// <param name="id">Channel id</param>
        /// <param name="name">New channel name</param>
        /// <param name="position">New channel position</param>
        /// <param name="topic">New channel topic</param>
        /// <param name="nsfw">Whether this channel should be marked as NSFW</param>
        /// <param name="parent">New channel parent</param>
        /// <param name="bitrate">New voice channel bitrate</param>
        /// <param name="userLimit">New voice channel user limit</param>
        /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
        /// <param name="rtcRegion">New region override.</param>
        /// <param name="qualityMode">New video quality mode.</param>
        /// <param name="type">New channel type.</param>
        /// <param name="permissionOverwrites">New channel permission overwrites.</param>
        /// <param name="reason">Reason why this channel was modified</param>
        /// <returns></returns>
        public Task ModifyChannelAsync(ulong id, string name, int? position, Optional<string> topic, bool? nsfw, Optional<ulong?> parent, int? bitrate, int? userLimit, Optional<int?> perUserRateLimit, Optional<DiscordVoiceRegion> rtcRegion, VideoQualityMode? qualityMode, Optional<ChannelType> type, IEnumerable<DiscordOverwriteBuilder> permissionOverwrites, string reason)
            => this.ApiClient.ModifyChannelAsync(id, name, position, topic, nsfw, parent, bitrate, userLimit, perUserRateLimit, rtcRegion.IfPresent(e => e?.Id), qualityMode, type, permissionOverwrites, reason);

        /// <summary>
        /// Modifies a channel
        /// </summary>
        /// <param name="channelId">Channel id</param>
        /// <param name="action">Channel modifications</param>
        /// <returns></returns>
        public Task ModifyChannelAsync(ulong channelId, Action<ChannelEditModel> action)
        {
            var mdl = new ChannelEditModel();
            action(mdl);

            return this.ApiClient.ModifyChannelAsync(channelId, mdl.Name, mdl.Position, mdl.Topic, mdl.Nsfw,
                mdl.Parent.HasValue ? mdl.Parent.Value?.Id : default(Optional<ulong?>), mdl.Bitrate, mdl.Userlimit, mdl.PerUserRateLimit, mdl.RtcRegion.IfPresent(e => e?.Id),
                mdl.QualityMode, mdl.Type, mdl.PermissionOverwrites, mdl.AuditLogReason);
        }

        /// <summary>
        /// Gets a channel object
        /// </summary>
        /// <param name="id">Channel id</param>
        /// <returns></returns>
        public Task<DiscordChannel> GetChannelAsync(ulong id)
            => this.ApiClient.GetChannelAsync(id);

        /// <summary>
        /// Deletes a channel
        /// </summary>
        /// <param name="id">Channel id</param>
        /// <param name="reason">Reason why this channel was deleted</param>
        /// <returns></returns>
        public Task DeleteChannelAsync(ulong id, string reason)
            => this.ApiClient.DeleteChannelAsync(id, reason);

        /// <summary>
        /// Gets message in a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <returns></returns>
        public Task<DiscordMessage> GetMessageAsync(ulong channel_id, ulong message_id)
            => this.ApiClient.GetMessageAsync(channel_id, message_id);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="content">Message (text) content</param>
        /// <returns></returns>
        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string content)
            => this.ApiClient.CreateMessageAsync(channel_id, content, null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="embed">Embed to attach</param>
        /// <returns></returns>
        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, DiscordEmbed embed)
            => this.ApiClient.CreateMessageAsync(channel_id, null, embed != null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="content">Message (text) content</param>
        /// <param name="embed">Embed to attach</param>
        /// <returns></returns>
        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string content, DiscordEmbed embed)
            => this.ApiClient.CreateMessageAsync(channel_id, content, embed != null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="builder">The Discord Mesage builder.</param>
        /// <returns></returns>
        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, DiscordMessageBuilder builder)
            => this.ApiClient.CreateMessageAsync(channel_id, builder);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="action">The Discord Mesage builder.</param>
        /// <returns></returns>
        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, Action<DiscordMessageBuilder> action)
        {
            var builder = new DiscordMessageBuilder();
            action(builder);
            return this.ApiClient.CreateMessageAsync(channel_id, builder);
        }

        /// <summary>
        /// Gets channels from a guild
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guild_id)
            => this.ApiClient.GetGuildChannelsAsync(guild_id);

        /// <summary>
        /// Gets messages from a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="limit">Limit of messages to get</param>
        /// <param name="before">Gets messages before this id</param>
        /// <param name="after">Gets messages after this id</param>
        /// <param name="around">Gets messages around this id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channel_id, int limit, ulong? before, ulong? after, ulong? around)
            => this.ApiClient.GetChannelMessagesAsync(channel_id, limit, before, after, around);

        /// <summary>
        /// Gets a message from a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <returns></returns>
        public Task<DiscordMessage> GetChannelMessageAsync(ulong channel_id, ulong message_id)
            => this.ApiClient.GetChannelMessageAsync(channel_id, message_id);

        /// <summary>
        /// Edits a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="content">New message content</param>
        /// <returns></returns>
        public Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, Optional<string> content)
            => this.ApiClient.EditMessageAsync(channel_id, message_id, content, default, default, default, Array.Empty<DiscordMessageFile>(), null, default);

        /// <summary>
        /// Edits a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="embed">New message embed</param>
        /// <returns></returns>
        public Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, Optional<DiscordEmbed> embed)
            => this.ApiClient.EditMessageAsync(channel_id, message_id, default, embed.HasValue ? new[] {embed.Value} : Array.Empty<DiscordEmbed>(), default, default, Array.Empty<DiscordMessageFile>(), null, default);

        /// <summary>
        /// Edits a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="builder">The builder of the message to edit.</param>
        /// <param name="suppressEmbeds">Whether to suppress embeds on the message.</param>
        /// <param name="attachments">Attached files to keep.</param>
        /// <returns></returns>
        public async Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, DiscordMessageBuilder builder, bool suppressEmbeds = false, IEnumerable<DiscordAttachment> attachments = default)
        {
            builder.Validate(true);

            return await this.ApiClient.EditMessageAsync(channel_id, message_id, builder.Content, new Optional<IEnumerable<DiscordEmbed>>(builder.Embeds), builder.Mentions, builder.Components, builder.Files, suppressEmbeds ? MessageFlags.SuppressedEmbeds : null, attachments).ConfigureAwait(false);
        }

        /// <summary>
        /// Modifes the visibility of embeds in a message.
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="hideEmbeds">Whether to hide all embeds.</param>
        public Task ModifyEmbedSuppressionAsync(ulong channel_id, ulong message_id, bool hideEmbeds)
            => this.ApiClient.EditMessageAsync(channel_id, message_id, default, default, default, default, Array.Empty<DiscordMessageFile>(), hideEmbeds ? MessageFlags.SuppressedEmbeds : null, default);

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="reason">Why this message was deleted</param>
        /// <returns></returns>
        public Task DeleteMessageAsync(ulong channel_id, ulong message_id, string reason)
            => this.ApiClient.DeleteMessageAsync(channel_id, message_id, reason);

        /// <summary>
        /// Deletes multiple messages
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_ids">Message ids</param>
        /// <param name="reason">Reason these messages were deleted</param>
        /// <returns></returns>
        public Task DeleteMessagesAsync(ulong channel_id, IEnumerable<ulong> message_ids, string reason)
            => this.ApiClient.DeleteMessagesAsync(channel_id, message_ids, reason);

        /// <summary>
        /// Gets a channel's invites
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channel_id)
            => this.ApiClient.GetChannelInvitesAsync(channel_id);

        /// <summary>
        /// Creates a channel invite
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="max_age">For how long the invite should exist</param>
        /// <param name="max_uses">How often the invite may be used</param>
        /// <param name="temporary">Whether this invite should be temporary</param>
        /// <param name="unique">Whether this invite should be unique (false might return an existing invite)</param>
        /// <param name="reason">Why you made an invite</param>
        /// <returns></returns>
        public Task<DiscordInvite> CreateChannelInviteAsync(ulong channel_id, int max_age, int max_uses, bool temporary, bool unique, string reason)
            => this.ApiClient.CreateChannelInviteAsync(channel_id, max_age, max_uses, temporary, unique, reason);

        /// <summary>
        /// Deletes channel overwrite
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="overwrite_id">Overwrite id</param>
        /// <param name="reason">Reason it was deleted</param>
        /// <returns></returns>
        public Task DeleteChannelPermissionAsync(ulong channel_id, ulong overwrite_id, string reason)
            => this.ApiClient.DeleteChannelPermissionAsync(channel_id, overwrite_id, reason);

        /// <summary>
        /// Edits channel overwrite
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="overwrite_id">Overwrite id</param>
        /// <param name="allow">Permissions to allow</param>
        /// <param name="deny">Permissions to deny</param>
        /// <param name="type">Overwrite type</param>
        /// <param name="reason">Reason this overwrite was created</param>
        /// <returns></returns>
        public Task EditChannelPermissionsAsync(ulong channel_id, ulong overwrite_id, Permissions allow, Permissions deny, string type, string reason)
            => this.ApiClient.EditChannelPermissionsAsync(channel_id, overwrite_id, allow, deny, type, reason);

        /// <summary>
        /// Send a typing indicator to a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <returns></returns>
        public Task TriggerTypingAsync(ulong channel_id)
            => this.ApiClient.TriggerTypingAsync(channel_id);

        /// <summary>
        /// Gets pinned messages
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channel_id)
            => this.ApiClient.GetPinnedMessagesAsync(channel_id);

        /// <summary>
        /// Unpuns a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <returns></returns>
        public Task UnpinMessageAsync(ulong channel_id, ulong message_id)
            => this.ApiClient.UnpinMessageAsync(channel_id, message_id);

        /// <summary>
        /// Joins a group DM
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="nickname">Dm nickname</param>
        /// <returns></returns>
        public Task JoinGroupDmAsync(ulong channel_id, string nickname)
            => this.ApiClient.AddGroupDmRecipientAsync(channel_id, this.CurrentUser.Id, this.Configuration.Token, nickname);

        /// <summary>
        /// Adds a member to a group DM
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="user_id">User id</param>
        /// <param name="access_token">User's access token</param>
        /// <param name="nickname">Nickname for user</param>
        /// <returns></returns>
        public Task GroupDmAddRecipientAsync(ulong channel_id, ulong user_id, string access_token, string nickname)
            => this.ApiClient.AddGroupDmRecipientAsync(channel_id, user_id, access_token, nickname);

        /// <summary>
        /// Leaves a group DM
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <returns></returns>
        public Task LeaveGroupDmAsync(ulong channel_id)
            => this.ApiClient.RemoveGroupDmRecipientAsync(channel_id, this.CurrentUser.Id);

        /// <summary>
        /// Removes a member from a group DM
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="user_id">User id</param>
        /// <returns></returns>
        public Task GroupDmRemoveRecipientAsync(ulong channel_id, ulong user_id)
            => this.ApiClient.RemoveGroupDmRecipientAsync(channel_id, user_id);

        /// <summary>
        /// Creates a group DM
        /// </summary>
        /// <param name="access_tokens">Access tokens</param>
        /// <param name="nicks">Nicknames per user</param>
        /// <returns></returns>
        public Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
            => this.ApiClient.CreateGroupDmAsync(access_tokens, nicks);

        /// <summary>
        /// Creates a group DM with current user
        /// </summary>
        /// <param name="access_tokens">Access tokens</param>
        /// <param name="nicks">Nicknames</param>
        /// <returns></returns>
        public Task<DiscordDmChannel> CreateGroupDmWithCurrentUserAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
        {
            var a = access_tokens.ToList();
            a.Add(this.Configuration.Token);
            return this.ApiClient.CreateGroupDmAsync(a, nicks);
        }

        /// <summary>
        /// Creates a DM
        /// </summary>
        /// <param name="recipient_id">Recipient user id</param>
        /// <returns></returns>
        public Task<DiscordDmChannel> CreateDmAsync(ulong recipient_id)
            => this.ApiClient.CreateDmAsync(recipient_id);

        /// <summary>
        /// Follows a news channel
        /// </summary>
        /// <param name="channel_id">Id of the channel to follow</param>
        /// <param name="webhook_channel_id">Id of the channel to crosspost messages to</param>
        /// <exception cref="UnauthorizedException">Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> on the target channel</exception>
        public Task<DiscordFollowedChannel> FollowChannelAsync(ulong channel_id, ulong webhook_channel_id)
            => this.ApiClient.FollowChannelAsync(channel_id, webhook_channel_id);

        /// <summary>
        /// Publishes a message in a news channel to following channels
        /// </summary>
        /// <param name="channel_id">Id of the news channel the message to crosspost belongs to</param>
        /// <param name="message_id">Id of the message to crosspost</param>
        /// <exception cref="UnauthorizedException">
        ///     Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> and/or <see cref="Permissions.SendMessages"/>
        /// </exception>
        public Task<DiscordMessage> CrosspostMessageAsync(ulong channel_id, ulong message_id)
            => this.ApiClient.CrosspostMessageAsync(channel_id, message_id);

        /// <summary>
        /// Creates a stage instance in a stage channel.
        /// </summary>
        /// <param name="channelId">The id of the stage channel to create it in.</param>
        /// <param name="topic">The topic of the stage instance.</param>
        /// <param name="privacyLevel">The privacy level of the stage instance.</param>
        /// <param name="reason">The reason the stage instance was created.</param>
        /// <returns>The created stage instance.</returns>
        public Task<DiscordStageInstance> CreateStageInstanceAsync(ulong channelId, string topic, PrivacyLevel? privacyLevel = null, string reason = null)
            => this.ApiClient.CreateStageInstanceAsync(channelId, topic, privacyLevel, reason);

        /// <summary>
        /// Gets a stage instance in a stage channel.
        /// </summary>
        /// <param name="channelId">The id of the channel.</param>
        /// <returns>The stage instance in the channel.</returns>
        public Task<DiscordStageInstance> GetStageInstanceAsync(ulong channelId)
            => this.ApiClient.GetStageInstanceAsync(channelId);

        /// <summary>
        /// Modifies a stage instance in a stage channel.
        /// </summary>
        /// <param name="channelId">The id of the channel to modify the stage instance of.</param>
        /// <param name="action">Action to perform.</param>
        /// <returns>The modified stage instance.</returns>
        public async Task<DiscordStageInstance> ModifyStageInstanceAsync(ulong channelId, Action<StageInstanceEditModel> action)
        {
            var mdl = new StageInstanceEditModel();
            action(mdl);
            return await this.ApiClient.ModifyStageInstanceAsync(channelId, mdl.Topic, mdl.PrivacyLevel, mdl.AuditLogReason).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a stage instance in a stage channel.
        /// </summary>
        /// <param name="channelId">The id of the channel to delete the stage instance of.</param>
        /// <param name="reason">The reason the stage instance was deleted.</param>
        public Task DeleteStageInstanceAsync(ulong channelId, string reason = null)
            => this.ApiClient.DeleteStageInstanceAsync(channelId, reason);

        #endregion

        #region Member
        /// <summary>
        /// Gets current user object
        /// </summary>
        /// <returns></returns>
        public Task<DiscordUser> GetCurrentUserAsync()
            => this.ApiClient.GetCurrentUserAsync();

        /// <summary>
        /// Gets user object
        /// </summary>
        /// <param name="user">User id</param>
        /// <returns></returns>
        public Task<DiscordUser> GetUserAsync(ulong user)
            => this.ApiClient.GetUserAsync(user);

        /// <summary>
        /// Gets guild member
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="member_id">Member id</param>
        /// <returns></returns>
        public Task<DiscordMember> GetGuildMemberAsync(ulong guild_id, ulong member_id)
            => this.ApiClient.GetGuildMemberAsync(guild_id, member_id);

        /// <summary>
        /// Removes guild member
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="reason">Why this user was removed</param>
        /// <returns></returns>
        public Task RemoveGuildMemberAsync(ulong guild_id, ulong user_id, string reason)
            => this.ApiClient.RemoveGuildMemberAsync(guild_id, user_id, reason);

        /// <summary>
        /// Modifies current user
        /// </summary>
        /// <param name="username">New username</param>
        /// <param name="base64_avatar">New avatar (base64)</param>
        /// <returns></returns>
        public async Task<DiscordUser> ModifyCurrentUserAsync(string username, string base64_avatar)
            => new DiscordUser(await this.ApiClient.ModifyCurrentUserAsync(username, base64_avatar).ConfigureAwait(false)) { Discord = this };

        /// <summary>
        /// Modifies current user
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="avatar">avatar</param>
        /// <returns></returns>
        public async Task<DiscordUser> ModifyCurrentUserAsync(string username = null, Stream avatar = null)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return new DiscordUser(await this.ApiClient.ModifyCurrentUserAsync(username, av64).ConfigureAwait(false)) { Discord = this };
        }

        /// <summary>
        /// Gets current user's guilds
        /// </summary>
        /// <param name="limit">Limit of guilds to get</param>
        /// <param name="before">Gets guild before id</param>
        /// <param name="after">Gets guilds after id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit = 100, ulong? before = null, ulong? after = null)
            => this.ApiClient.GetCurrentUserGuildsAsync(limit, before, after);

        /// <summary>
        /// Modifies guild member
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="nick">New nickname</param>
        /// <param name="role_ids">New roles</param>
        /// <param name="mute">Whether this user should be muted</param>
        /// <param name="deaf">Whether this user should be deafened</param>
        /// <param name="voice_channel_id">Voice channel to move this user to</param>
        /// <param name="reason">Reason this user was modified</param>
        /// <returns></returns>
        public Task ModifyGuildMemberAsync(ulong guild_id, ulong user_id, Optional<string> nick,
            Optional<IEnumerable<ulong>> role_ids, Optional<bool> mute, Optional<bool> deaf,
            Optional<ulong?> voice_channel_id, string reason)
            => this.ApiClient.ModifyGuildMemberAsync(guild_id, user_id, nick, role_ids, mute, deaf, voice_channel_id, reason);

        /// <summary>
        /// Modifies a member
        /// </summary>
        /// <param name="member_id">Member id</param>
        /// <param name="guild_id">Guild id</param>
        /// <param name="action">Modifications</param>
        /// <returns></returns>
        public async Task ModifyAsync(ulong member_id, ulong guild_id, Action<MemberEditModel> action)
        {
            var mdl = new MemberEditModel();
            action(mdl);

            if (mdl.VoiceChannel.HasValue && mdl.VoiceChannel.Value != null && mdl.VoiceChannel.Value.Type != ChannelType.Voice && mdl.VoiceChannel.Value.Type != ChannelType.Stage)
                throw new ArgumentException("Given channel is not a voice or stage channel.", nameof(mdl.VoiceChannel));

            if (mdl.Nickname.HasValue && this.CurrentUser.Id == member_id)
            {
                await this.ApiClient.ModifyCurrentMemberNicknameAsync(guild_id, mdl.Nickname.Value,
                    mdl.AuditLogReason).ConfigureAwait(false);
                await this.ApiClient.ModifyGuildMemberAsync(guild_id, member_id, Optional.FromNoValue<string>(),
                    mdl.Roles.IfPresent(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
                    mdl.VoiceChannel.IfPresent(e => e?.Id), mdl.AuditLogReason).ConfigureAwait(false);
            }
            else
            {
                await this.ApiClient.ModifyGuildMemberAsync(guild_id, member_id, mdl.Nickname,
                    mdl.Roles.IfPresent(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
                    mdl.VoiceChannel.IfPresent(e => e?.Id), mdl.AuditLogReason).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Changes current user's nickname
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="nick">Nickname</param>
        /// <param name="reason">Reason why you set it to this</param>
        /// <returns></returns>
        public Task ModifyCurrentMemberNicknameAsync(ulong guild_id, string nick, string reason)
            => this.ApiClient.ModifyCurrentMemberNicknameAsync(guild_id, nick, reason);
        #endregion

        #region Roles
        /// <summary>
        /// Gets roles
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guild_id)
            => this.ApiClient.GetGuildRolesAsync(guild_id);

        /// <summary>
        /// Gets a guild.
        /// </summary>
        /// <param name="guild_id">The guild ID to search for.</param>
        /// <param name="with_counts">Whether to include approximate presence and member counts in the returned guild.</param>
        /// <returns></returns>
        public Task<DiscordGuild> GetGuildAsync(ulong guild_id, bool? with_counts = null)
            => this.ApiClient.GetGuildAsync(guild_id, with_counts);

        /// <summary>
        /// Modifies a role
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="role_id">Role id</param>
        /// <param name="name">New role name</param>
        /// <param name="permissions">New role permissions</param>
        /// <param name="color">New role color</param>
        /// <param name="hoist">Whether this role should be hoisted</param>
        /// <param name="mentionable">Whether this role should be mentionable</param>
        /// <param name="reason">Why this role was modified</param>
        /// <param name="icon">The icon to add to this role</param>
        /// <param name="emoji">The emoji to add to this role. Must be unicode.</param>
        /// <returns></returns>
        public Task<DiscordRole> ModifyGuildRoleAsync(ulong guild_id, ulong role_id, string name, Permissions? permissions, DiscordColor? color, bool? hoist, bool? mentionable, string reason, Stream icon, DiscordEmoji emoji)
            => this.ApiClient.ModifyGuildRoleAsync(guild_id, role_id, name, permissions, color.HasValue ? (int?)color.Value.Value : null, hoist, mentionable, reason, icon, emoji?.ToString());

        /// <summary>
        /// Modifies a role
        /// </summary>
        /// <param name="role_id">Role id</param>
        /// <param name="guild_id">Guild id</param>
        /// <param name="action">Modifications</param>
        /// <returns></returns>
        public Task ModifyGuildRoleAsync(ulong role_id, ulong guild_id, Action<RoleEditModel> action)
        {
            var mdl = new RoleEditModel();
            action(mdl);

            return this.ModifyGuildRoleAsync(guild_id, role_id, mdl.Name, mdl.Permissions, mdl.Color, mdl.Hoist, mdl.Mentionable, mdl.AuditLogReason, mdl.Icon, mdl.Emoji);
        }

        /// <summary>
        /// Deletes a role
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="role_id">Role id</param>
        /// <param name="reason">Reason why this role was deleted</param>
        /// <returns></returns>
        public Task DeleteGuildRoleAsync(ulong guild_id, ulong role_id, string reason)
            => this.ApiClient.DeleteRoleAsync(guild_id, role_id, reason);

        /// <summary>
        /// Creates a new role
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="name">Role name</param>
        /// <param name="permissions">Role permissions</param>
        /// <param name="color">Role color</param>
        /// <param name="hoist">Whether this role should be hoisted</param>
        /// <param name="mentionable">Whether this role should be mentionable</param>
        /// <param name="reason">Reason why this role was created</param>
        /// <param name="icon">The icon to add to this role</param>
        /// <param name="emoji">The emoji to add to this role. Must be unicode.</param>
        /// <returns></returns>
        public Task<DiscordRole> CreateGuildRoleAsync(ulong guild_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason, Stream icon = null, DiscordEmoji emoji = null)
            => this.ApiClient.CreateGuildRoleAsync(guild_id, name, permissions, color, hoist, mentionable, reason, icon , emoji?.ToString());
        #endregion

        #region Prune
        /// <summary>
        /// Get a guild's prune count.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="days">Days to check for</param>
        /// <param name="include_roles">The roles to be included in the prune.</param>
        /// <returns></returns>
        public Task<int> GetGuildPruneCountAsync(ulong guild_id, int days, IEnumerable<ulong> include_roles)
            => this.ApiClient.GetGuildPruneCountAsync(guild_id, days, include_roles);

        /// <summary>
        /// Begins a guild prune.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="days">Days to prune for</param>
        /// <param name="compute_prune_count">Whether to return the prune count after this method completes. This is discouraged for larger guilds.</param>
        /// <param name="include_roles">The roles to be included in the prune.</param>
        /// <param name="reason">Reason why this guild was pruned</param>
        /// <returns></returns>
        public Task<int?> BeginGuildPruneAsync(ulong guild_id, int days, bool compute_prune_count, IEnumerable<ulong> include_roles, string reason)
            => this.ApiClient.BeginGuildPruneAsync(guild_id, days, compute_prune_count, include_roles, reason);
        #endregion

        #region GuildVarious
        /// <summary>
        /// Gets guild integrations
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guild_id)
            => this.ApiClient.GetGuildIntegrationsAsync(guild_id);

        /// <summary>
        /// Creates guild integration
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="type">Integration type</param>
        /// <param name="id">Integration id</param>
        /// <returns></returns>
        public Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guild_id, string type, ulong id)
            => this.ApiClient.CreateGuildIntegrationAsync(guild_id, type, id);

        /// <summary>
        /// Modifies a guild integration
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="integration_id">Integration id</param>
        /// <param name="expire_behaviour">Expiration behaviour</param>
        /// <param name="expire_grace_period">Expiration grace period</param>
        /// <param name="enable_emoticons">Whether to enable emojis for this integration</param>
        /// <returns></returns>
        public Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guild_id, ulong integration_id, int expire_behaviour, int expire_grace_period, bool enable_emoticons)
            => this.ApiClient.ModifyGuildIntegrationAsync(guild_id, integration_id, expire_behaviour, expire_grace_period, enable_emoticons);

        /// <summary>
        /// Removes a guild integration
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="integration">Integration to remove</param>
        /// <returns></returns>
        public Task DeleteGuildIntegrationAsync(ulong guild_id, DiscordIntegration integration)
            => this.ApiClient.DeleteGuildIntegrationAsync(guild_id, integration);

        /// <summary>
        /// Syncs guild integration
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="integration_id">Integration id</param>
        /// <returns></returns>
        public Task SyncGuildIntegrationAsync(ulong guild_id, ulong integration_id)
            => this.ApiClient.SyncGuildIntegrationAsync(guild_id, integration_id);

        /// <summary>
        /// Get a guild's voice region
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guild_id)
            => this.ApiClient.GetGuildVoiceRegionsAsync(guild_id);

        /// <summary>
        /// Get a guild's invites
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guild_id)
            => this.ApiClient.GetGuildInvitesAsync(guild_id);

        /// <summary>
        /// Gets a guild's templates.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns>All of the guild's templates.</returns>
        public Task<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync(ulong guild_id)
            => this.ApiClient.GetGuildTemplatesAsync(guild_id);

        /// <summary>
        /// Creates a guild template.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="name">Name of the template.</param>
        /// <param name="description">Description of the template.</param>
        /// <returns>The template created.</returns>
        public Task<DiscordGuildTemplate> CreateGuildTemplateAsync(ulong guild_id, string name, string description = null)
            => this.ApiClient.CreateGuildTemplateAsync(guild_id, name, description);

        /// <summary>
        /// Syncs the template to the current guild's state.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="code">The code of the template to sync.</param>
        /// <returns>The template synced.</returns>
        public Task<DiscordGuildTemplate> SyncGuildTemplateAsync(ulong guild_id, string code)
            => this.ApiClient.SyncGuildTemplateAsync(guild_id, code);

        /// <summary>
        /// Modifies the template's metadata.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="code">The template's code.</param>
        /// <param name="name">Name of the template.</param>
        /// <param name="description">Description of the template.</param>
        /// <returns>The template modified.</returns>
        public Task<DiscordGuildTemplate> ModifyGuildTemplateAsync(ulong guild_id, string code, string name = null, string description = null)
            => this.ApiClient.ModifyGuildTemplateAsync(guild_id, code, name, description);

        /// <summary>
        /// Deletes the template.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="code">The code of the template to delete.</param>
        /// <returns>The deleted template.</returns>
        public Task<DiscordGuildTemplate> DeleteGuildTemplateAsync(ulong guild_id, string code)
            => this.ApiClient.DeleteGuildTemplateAsync(guild_id, code);

        /// <summary>
        /// Gets a guild's welcome screen.
        /// </summary>
        /// <returns>The guild's welcome screen object.</returns>
        public Task<DiscordGuildWelcomeScreen> GetGuildWelcomeScreenAsync(ulong guildId) =>
            this.ApiClient.GetGuildWelcomeScreenAsync(guildId);

        /// <summary>
        /// Modifies a guild's welcome screen.
        /// </summary>
        /// <param name="guildId">The guild ID to modify.</param>
        /// <param name="action">Action to perform.</param>
        /// <returns>The modified welcome screen.</returns>
        public async Task<DiscordGuildWelcomeScreen> ModifyGuildWelcomeScreenAsync(ulong guildId, Action<WelcomeScreenEditModel> action)
        {
            var mdl = new WelcomeScreenEditModel();
            action(mdl);
            return await this.ApiClient.ModifyGuildWelcomeScreenAsync(guildId, mdl.Enabled, mdl.WelcomeChannels, mdl.Description).ConfigureAwait(false);
        }
        #endregion

        #region Invites
        /// <summary>
        /// Gets an invite.
        /// </summary>
        /// <param name="invite_code">The invite code.</param>
        /// <param name="withCounts">Whether to include presence and total member counts in the returned invite.</param>
        /// <param name="withExpiration">Whether to include the expiration date in the returned invite.</param>
        /// <returns></returns>
        public Task<DiscordInvite> GetInviteAsync(string invite_code, bool? withCounts = null, bool? withExpiration = null)
            => this.ApiClient.GetInviteAsync(invite_code, withCounts, withExpiration);

        /// <summary>
        /// Removes an invite
        /// </summary>
        /// <param name="invite_code">Invite code</param>
        /// <param name="reason">Reason why this invite was removed</param>
        /// <returns></returns>
        public Task<DiscordInvite> DeleteInvite(string invite_code, string reason)
            => this.ApiClient.DeleteInviteAsync(invite_code, reason);
        #endregion

        #region Connections
        /// <summary>
        /// Gets current user's connections
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordConnection>> GetUsersConnectionsAsync()
            => this.ApiClient.GetUsersConnectionsAsync();
        #endregion

        #region Webhooks
        /// <summary>
        /// Creates a new webhook
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="name">Webhook name</param>
        /// <param name="base64_avatar">Webhook avatar (base64)</param>
        /// <param name="reason">Reason why this webhook was created</param>
        /// <returns></returns>
        public Task<DiscordWebhook> CreateWebhookAsync(ulong channel_id, string name, string base64_avatar, string reason)
            => this.ApiClient.CreateWebhookAsync(channel_id, name, base64_avatar, reason);

        /// <summary>
        /// Creates a new webhook
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="name">Webhook name</param>
        /// <param name="avatar">Webhook avatar</param>
        /// <param name="reason">Reason why this webhook was created</param>
        /// <returns></returns>
        public Task<DiscordWebhook> CreateWebhookAsync(ulong channel_id, string name, Stream avatar = null, string reason = null)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return this.ApiClient.CreateWebhookAsync(channel_id, name, av64, reason);
        }

        /// <summary>
        /// Gets all webhooks from a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channel_id)
            => this.ApiClient.GetChannelWebhooksAsync(channel_id);

        /// <summary>
        /// Gets all webhooks from a guild
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guild_id)
            => this.ApiClient.GetGuildWebhooksAsync(guild_id);

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookAsync(ulong webhook_id)
            => this.ApiClient.GetWebhookAsync(webhook_id);

        /// <summary>
        /// Gets a webhook with its token (when user is not in said guild)
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <param name="webhook_token">Webhook token</param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhook_id, string webhook_token)
            => this.ApiClient.GetWebhookWithTokenAsync(webhook_id, webhook_token);

        /// <summary>
        /// Modifies a webhook
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <param name="channelId">The new channel id the webhook should be moved to.</param>
        /// <param name="name">New webhook name</param>
        /// <param name="base64_avatar">New webhook avatar (base64)</param>
        /// <param name="reason">Reason why this webhook was modified</param>
        /// <returns></returns>
        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, ulong channelId, string name, string base64_avatar, string reason)
            => this.ApiClient.ModifyWebhookAsync(webhook_id, channelId, name, base64_avatar, reason);

        /// <summary>
        /// Modifies a webhook
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <param name="channelId">The new channel id the webhook should be moved to.</param>
        /// <param name="name">New webhook name</param>
        /// <param name="avatar">New webhook avatar</param>
        /// <param name="reason">Reason why this webhook was modified</param>
        /// <returns></returns>
        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, ulong channelId, string name, Stream avatar, string reason)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return this.ApiClient.ModifyWebhookAsync(webhook_id, channelId, name, av64, reason);
        }

        /// <summary>
        /// Modifies a webhook (when user is not in said guild)
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <param name="name">New webhook name</param>
        /// <param name="base64_avatar">New webhook avatar (base64)</param>
        /// <param name="webhook_token">Webhook token</param>
        /// <param name="reason">Reason why this webhook was modified</param>
        /// <returns></returns>
        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string webhook_token, string reason)
            => this.ApiClient.ModifyWebhookAsync(webhook_id, name, base64_avatar, webhook_token, reason);

        /// <summary>
        /// Modifies a webhook (when user is not in said guild)
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <param name="name">New webhook name</param>
        /// <param name="avatar">New webhook avatar</param>
        /// <param name="webhook_token">Webhook token</param>
        /// <param name="reason">Reason why this webhook was modified</param>
        /// <returns></returns>
        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, Stream avatar, string webhook_token, string reason)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return this.ApiClient.ModifyWebhookAsync(webhook_id, name, av64, webhook_token, reason);
        }

        /// <summary>
        /// Deletes a webhook
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <param name="reason">Reason this webhook was deleted</param>
        /// <returns></returns>
        public Task DeleteWebhookAsync(ulong webhook_id, string reason)
            => this.ApiClient.DeleteWebhookAsync(webhook_id, reason);

        /// <summary>
        /// Deletes a webhook (when user is not in said guild)
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <param name="reason">Reason this webhook was removed</param>
        /// <param name="webhook_token">Webhook token</param>
        /// <returns></returns>
        public Task DeleteWebhookAsync(ulong webhook_id, string reason, string webhook_token)
            => this.ApiClient.DeleteWebhookAsync(webhook_id, webhook_token, reason);

        /// <summary>
        /// Sends a message to a webhook
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <param name="webhook_token">Webhook token</param>
        /// <param name="builder">Webhook builder filled with data to send.</param>
        /// <returns></returns>
        public Task<DiscordMessage> ExecuteWebhookAsync(ulong webhook_id, string webhook_token, DiscordWebhookBuilder builder)
            => this.ApiClient.ExecuteWebhookAsync(webhook_id, webhook_token, builder);

        /// <summary>
        /// Edits a previously-sent webhook message.
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <param name="webhook_token">Webhook token</param>
        /// <param name="messageId">The id of the message to edit.</param>
        /// <param name="builder">The builder of the message to edit.</param>
        /// <param name="attachments">Attached files to keep.</param>
        /// <returns>The modified <see cref="DiscordMessage"/></returns>
        public async Task<DiscordMessage> EditWebhookMessageAsync(ulong webhook_id, string webhook_token, ulong messageId, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments = default)
        {
            builder.Validate(true);

            return await this.ApiClient.EditWebhookMessageAsync(webhook_id, webhook_token, messageId, builder, attachments).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a message that was created by a webhook.
        /// </summary>
        /// <param name="webhook_id">Webhook id</param>
        /// <param name="webhook_token">Webhook token</param>
        /// <param name="messageId">The id of the message to delete</param>
        /// <returns></returns>
        public Task DeleteWebhookMessageAsync(ulong webhook_id, string webhook_token, ulong messageId)
            => this.ApiClient.DeleteWebhookMessageAsync(webhook_id, webhook_token, messageId);
        #endregion

        #region Reactions
        /// <summary>
        /// Creates a new reaction
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="emoji">Emoji to react</param>
        /// <returns></returns>
        public Task CreateReactionAsync(ulong channel_id, ulong message_id, string emoji)
            => this.ApiClient.CreateReactionAsync(channel_id, message_id, emoji);

        /// <summary>
        /// Deletes own reaction
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="emoji">Emoji to remove from reaction</param>
        /// <returns></returns>
        public Task DeleteOwnReactionAsync(ulong channel_id, ulong message_id, string emoji)
            => this.ApiClient.DeleteOwnReactionAsync(channel_id, message_id, emoji);

        /// <summary>
        /// Deletes someone elses reaction
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="user_id">User id</param>
        /// <param name="emoji">Emoji to remove</param>
        /// <param name="reason">Reason why this reaction was removed</param>
        /// <returns></returns>
        public Task DeleteUserReactionAsync(ulong channel_id, ulong message_id, ulong user_id, string emoji, string reason)
            => this.ApiClient.DeleteUserReactionAsync(channel_id, message_id, user_id, emoji, reason);

        /// <summary>
        /// Gets all users that reacted with a specific emoji to a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="emoji">Emoji to check for</param>
        /// <param name="after_id">Whether to search for reactions after this message id.</param>
        /// <param name="limit">The maximum amount of reactions to fetch.</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channel_id, ulong message_id, string emoji, ulong? after_id = null, int limit = 25)
            => this.ApiClient.GetReactionsAsync(channel_id, message_id, emoji, after_id, limit);

        /// <summary>
        /// Gets all users that reacted with a specific emoji to a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="emoji">Emoji to check for</param>
        /// <param name="after_id">Whether to search for reactions after this message id.</param>
        /// <param name="limit">The maximum amount of reactions to fetch.</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channel_id, ulong message_id, DiscordEmoji emoji, ulong? after_id = null, int limit = 25)
            => this.ApiClient.GetReactionsAsync(channel_id, message_id, emoji.ToReactionString(), after_id, limit);

        /// <summary>
        /// Deletes all reactions from a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="reason">Reason why all reactions were removed</param>
        /// <returns></returns>
        public Task DeleteAllReactionsAsync(ulong channel_id, ulong message_id, string reason)
            => this.ApiClient.DeleteAllReactionsAsync(channel_id, message_id, reason);
        #endregion

        #region Application Commands
        /// <summary>
        /// Gets all the global application commands for this application.
        /// </summary>
        /// <returns>A list of global application commands.</returns>
        public Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync() =>
            this.ApiClient.GetGlobalApplicationCommandsAsync(this.CurrentApplication.Id);

        /// <summary>
        /// Overwrites the existing global application commands. New commands are automatically created and missing commands are automatically deleted.
        /// </summary>
        /// <param name="commands">The list of commands to overwrite with.</param>
        /// <returns>The list of global commands.</returns>
        public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(IEnumerable<DiscordApplicationCommand> commands) =>
            this.ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(this.CurrentApplication.Id, commands);

        /// <summary>
        /// Creates or overwrites a global application command.
        /// </summary>
        /// <param name="command">The command to create.</param>
        /// <returns>The created command.</returns>
        public Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(DiscordApplicationCommand command) =>
            this.ApiClient.CreateGlobalApplicationCommandAsync(this.CurrentApplication.Id, command);

        /// <summary>
        /// Gets a global application command by its id.
        /// </summary>
        /// <param name="commandId">The id of the command to get.</param>
        /// <returns>The command with the id.</returns>
        public Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong commandId) =>
            this.ApiClient.GetGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

        /// <summary>
        /// Edits a global application command.
        /// </summary>
        /// <param name="commandId">The id of the command to edit.</param>
        /// <param name="action">Action to perform.</param>
        /// <returns>The edited command.</returns>
        public async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong commandId, Action<ApplicationCommandEditModel> action)
        {
            var mdl = new ApplicationCommandEditModel();
            action(mdl);
            var applicationId = this.CurrentApplication?.Id ?? (await this.GetCurrentApplicationAsync().ConfigureAwait(false)).Id;
            return await this.ApiClient.EditGlobalApplicationCommandAsync(applicationId, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.DefaultPermission).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a global application command.
        /// </summary>
        /// <param name="commandId">The id of the command to delete.</param>
        public Task DeleteGlobalApplicationCommandAsync(ulong commandId) =>
            this.ApiClient.DeleteGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

        /// <summary>
        /// Gets all the application commands for a guild.
        /// </summary>
        /// <param name="guildId">The id of the guild to get application commands for.</param>
        /// <returns>A list of application commands in the guild.</returns>
        public Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong guildId) =>
            this.ApiClient.GetGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId);

        /// <summary>
        /// Overwrites the existing application commands in a guild. New commands are automatically created and missing commands are automatically deleted.
        /// </summary>
        /// <param name="guildId">The id of the guild.</param>
        /// <param name="commands">The list of commands to overwrite with.</param>
        /// <returns>The list of guild commands.</returns>
        public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong guildId, IEnumerable<DiscordApplicationCommand> commands) =>
            this.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId, commands);

        /// <summary>
        /// Creates or overwrites a guild application command.
        /// </summary>
        /// <param name="guildId">The id of the guild to create the application command in.</param>
        /// <param name="command">The command to create.</param>
        /// <returns>The created command.</returns>
        public Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong guildId, DiscordApplicationCommand command) =>
            this.ApiClient.CreateGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, command);

        /// <summary>
        /// Gets a application command in a guild by its id.
        /// </summary>
        /// <param name="guildId">The id of the guild the application command is in.</param>
        /// <param name="commandId">The id of the command to get.</param>
        /// <returns>The command with the id.</returns>
        public Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
             this.ApiClient.GetGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

        /// <summary>
        /// Edits a application command in a guild.
        /// </summary>
        /// <param name="guildId">The id of the guild the application command is in.</param>
        /// <param name="commandId">The id of the command to edit.</param>
        /// <param name="action">Action to perform.</param>
        /// <returns>The edited command.</returns>
        public async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong guildId, ulong commandId, Action<ApplicationCommandEditModel> action)
        {
            var mdl = new ApplicationCommandEditModel();
            action(mdl);
            var applicationId = this.CurrentApplication?.Id ?? (await this.GetCurrentApplicationAsync().ConfigureAwait(false)).Id;
            return await this.ApiClient.EditGuildApplicationCommandAsync(applicationId, guildId, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.DefaultPermission).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a application command in a guild.
        /// </summary>
        /// <param name="guildId">The id of the guild to delete the application command in.</param>
        /// <param name="commandId">The id of the command.</param>
        public Task DeleteGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
            this.ApiClient.DeleteGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

        /// <summary>
        /// Creates a response to an interaction.
        /// </summary>
        /// <param name="interactionId">The id of the interaction.</param>
        /// <param name="interactionToken">The token of the interaction</param>
        /// <param name="type">The type of the response.</param>
        /// <param name="builder">The data, if any, to send.</param>
        public Task CreateInteractionResponseAsync(ulong interactionId, string interactionToken, InteractionResponseType type, DiscordInteractionResponseBuilder builder = null) =>
            this.ApiClient.CreateInteractionResponseAsync(interactionId, interactionToken, type, builder);

        /// <summary>
        /// Gets the original interaction response.
        /// </summary>
        /// <returns>The original message that was sent. This <b>does not work on ephemeral messages.</b></returns>
        public Task<DiscordMessage> GetOriginalInteractionResponseAsync(string interactionToken) =>
            this.ApiClient.GetOriginalInteractionResponseAsync(this.CurrentApplication.Id, interactionToken);

        /// <summary>
        /// Edits the original interaction response.
        /// </summary>
        /// <param name="interactionToken">The token of the interaction.</param>
        /// <param name="builder">The webhook builder.</param>
        /// <param name="attachments">Attached files to keep.</param>
        /// <returns>The <see cref="DiscordMessage"/> edited.</returns>
        public async Task<DiscordMessage> EditOriginalInteractionResponseAsync(string interactionToken, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments = default)
        {
            builder.Validate(isInteractionResponse: true);

            return await this.ApiClient.EditOriginalInteractionResponseAsync(this.CurrentApplication.Id, interactionToken, builder, attachments).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the original interaction response.
        /// <param name="interactionToken">The token of the interaction.</param>
        /// </summary>>
        public Task DeleteOriginalInteractionResponseAsync(string interactionToken) =>
            this.ApiClient.DeleteOriginalInteractionResponseAsync(this.CurrentApplication.Id, interactionToken);

        /// <summary>
        /// Creates a follow up message to an interaction.
        /// </summary>
        /// <param name="interactionToken">The token of the interaction.</param>
        /// <param name="builder">The webhook builder.</param>
        /// <returns>The <see cref="DiscordMessage"/> created.</returns>
        public async Task<DiscordMessage> CreateFollowupMessageAsync(string interactionToken, DiscordFollowupMessageBuilder builder)
        {
            builder.Validate();

            return await this.ApiClient.CreateFollowupMessageAsync(this.CurrentApplication.Id, interactionToken, builder).ConfigureAwait(false);
        }

        /// <summary>
        /// Edits a follow up message.
        /// </summary>
        /// <param name="interactionToken">The token of the interaction.</param>
        /// <param name="messageId">The id of the follow up message.</param>
        /// <param name="builder">The webhook builder.</param>
        /// <param name="attachments">Attached files to keep.</param>
        /// <returns>The <see cref="DiscordMessage"/> edited.</returns>
        public async Task<DiscordMessage> EditFollowupMessageAsync(string interactionToken, ulong messageId, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments = default)
        {
            builder.Validate(isFollowup: true);

            return await this.ApiClient.EditFollowupMessageAsync(this.CurrentApplication.Id, interactionToken, messageId, builder, attachments).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a follow up message.
        /// </summary>
        /// <param name="interactionToken">The token of the interaction.</param>
        /// <param name="messageId">The id of the follow up message.</param>
        public Task DeleteFollowupMessageAsync(string interactionToken, ulong messageId) =>
            this.ApiClient.DeleteFollowupMessageAsync(this.CurrentApplication.Id, interactionToken, messageId);

        /// <summary>
        /// Gets all application command permissions in a guild.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <returns>A list of permissions.</returns>
        public Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> GetGuildApplicationCommandsPermissionsAsync(ulong guildId)
            => this.ApiClient.GetGuildApplicationCommandPermissionsAsync(this.CurrentApplication.Id, guildId);

        /// <summary>
        /// Gets permissions for a application command in a guild.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="commandId">The id of the command to get them for.</param>
        /// <returns>The permissions.</returns>
        public Task<DiscordGuildApplicationCommandPermissions> GetGuildApplicationCommandPermissionsAsync(ulong guildId, ulong commandId)
            => this.ApiClient.GetApplicationCommandPermissionsAsync(this.CurrentApplication.Id, guildId, commandId);

        /// <summary>
        /// Edits permissions for a application command in a guild.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="commandId">The id of the command to edit permissions for.</param>
        /// <param name="permissions">The list of permissions to use.</param>
        /// <returns>The edited permissions.</returns>
        public Task<DiscordGuildApplicationCommandPermissions> EditApplicationCommandPermissionsAsync(ulong guildId, ulong commandId, IEnumerable<DiscordApplicationCommandPermission> permissions)
            => this.ApiClient.EditApplicationCommandPermissionsAsync(this.CurrentApplication.Id, guildId, commandId, permissions);

        /// <summary>
        /// Batch edits permissions for a application command in a guild.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="permissions">The list of permissions to use.</param>
        /// <returns>A list of edited permissions.</returns>
        public Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> BatchEditApplicationCommandPermissionsAsync(ulong guildId, IEnumerable<DiscordGuildApplicationCommandPermissions> permissions)
            => this.ApiClient.BatchEditApplicationCommandPermissionsAsync(this.CurrentApplication.Id, guildId, permissions);
        #endregion

        #region Misc
        /// <summary>
        /// Gets assets from an application
        /// </summary>
        /// <param name="application">Application to get assets from</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication application)
            => this.ApiClient.GetApplicationAssetsAsync(application);

        /// <summary>
        /// Gets a guild template by the code.
        /// </summary>
        /// <param name="code">The code of the template.</param>
        /// <returns>The guild template for the code.</returns>\
        public Task<DiscordGuildTemplate> GetTemplateAsync(string code)
            => this.ApiClient.GetTemplateAsync(code);
        #endregion

        private bool _disposed;
        /// <summary>
        /// Disposes of this DiscordRestClient
        /// </summary>
        public override void Dispose()
        {
            if (this._disposed)
                return;
            this._disposed = true;
            this._guilds = null;
            this.ApiClient.Rest.Dispose();
        }
    }
}
