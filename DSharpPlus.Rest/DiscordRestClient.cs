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

namespace DSharpPlus;

public class DiscordRestClient : BaseDiscordClient
{
    /// <summary>
    /// Gets the dictionary of guilds cached by this client.
    /// </summary>
    public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds
        => this._guilds;

    internal Dictionary<ulong, DiscordGuild> _guilds = new();

    public DiscordRestClient(DiscordConfiguration config) : base(config) => this._disposed = false;

    /// <summary>
    /// Initializes cache
    /// </summary>
    /// <returns></returns>
    public async Task InitializeCacheAsync()
    {
        await base.InitializeAsync();
        IReadOnlyList<DiscordGuild> currentUserGuilds = await this.ApiClient.GetCurrentUserGuildsAsync();
        foreach (DiscordGuild guild in currentUserGuilds)
        {
            this._guilds[guild.Id] = guild;
        }
    }

    #region Scheduled Guild Events

    /// <summary>
    /// Creates a new scheduled guild event.
    /// </summary>
    /// <param name="guildId">The guild to create an event on.</param>
    /// <param name="name">The name of the event, up to 100 characters.</param>
    /// <param name="description">The description of the event, up to 1000 characters.</param>
    /// <param name="channelId">The channel the event will take place in, if applicable.</param>
    /// <param name="type">The type of event. If <see cref="DiscordScheduledGuildEventType.External"/>, a end time must be specified.</param>
    /// <param name="image">The image of event.</param>
    /// <param name="privacyLevel">The privacy level of the event.</param>
    /// <param name="start">When the event starts. Must be in the future and before the end date, if specified.</param>
    /// <param name="end">When the event ends. Required for <see cref="DiscordScheduledGuildEventType.External"/></param>
    /// <param name="location">Where this location takes place.</param>
    /// <returns>The created event.</returns>
    public async Task<DiscordScheduledGuildEvent> CreateScheduledGuildEventAsync(ulong guildId, string name, string description, ulong? channelId, DiscordScheduledGuildEventType type, DiscordScheduledGuildEventPrivacyLevel privacyLevel, DateTimeOffset start, DateTimeOffset? end, Stream? image = null, string location = null)
        => await this.ApiClient.CreateScheduledGuildEventAsync(guildId, name, description, start, type, privacyLevel, new DiscordScheduledGuildEventMetadata(location), end, channelId, image);

    /// <summary>
    /// Delete a scheduled guild event.
    /// </summary>
    /// <param name="guildId">The ID the guild the event resides on.</param>
    /// <param name="eventId">The ID of the event to delete.</param>
    public async Task DeleteScheduledGuildEventAsync(ulong guildId, ulong eventId)
        => await this.ApiClient.DeleteScheduledGuildEventAsync(guildId, eventId);

    /// <summary>
    /// Gets a specific scheduled guild event.
    /// </summary>
    /// <param name="guildId">The ID of the guild the event resides on.</param>
    /// <param name="eventId">The ID of the event to get</param>
    /// <returns>The requested event.</returns>
    public async Task<DiscordScheduledGuildEvent> GetScheduledGuildEventAsync(ulong guildId, ulong eventId)
        => await this.ApiClient.GetScheduledGuildEventAsync(guildId, eventId);

    /// <summary>
    /// Gets all available scheduled guild events.
    /// </summary>
    /// <param name="guildId">The ID of the guild to query.</param>
    /// <returns>All active and scheduled events.</returns>
    public async Task<IReadOnlyList<DiscordScheduledGuildEvent>> GetScheduledGuildEventsAsync(ulong guildId)
        => await this.ApiClient.GetScheduledGuildEventsAsync(guildId);

    /// <summary>
    /// Modify a scheduled guild event.
    /// </summary>
    /// <param name="guildId">The ID of the guild the event resides on.</param>
    /// <param name="eventId">The ID of the event to modify.</param>
    /// <param name="mdl">The action to apply to the event.</param>
    /// <returns>The modified event.</returns>
    public async Task<DiscordScheduledGuildEvent> ModifyScheduledGuildEventAsync(ulong guildId, ulong eventId, Action<ScheduledGuildEventEditModel> mdl)
    {
        ScheduledGuildEventEditModel model = new();
        mdl(model);

        if (model.Type.HasValue && model.Type.Value is DiscordScheduledGuildEventType.StageInstance or DiscordScheduledGuildEventType.VoiceChannel)
        {
            if (!model.Channel.HasValue)
            {
                throw new ArgumentException("Channel must be supplied if the event is a stage instance or voice channel event.");
            }
        }

        if (model.Type.HasValue && model.Type.Value is DiscordScheduledGuildEventType.External)
        {
            if (!model.EndTime.HasValue)
            {
                throw new ArgumentException("End must be supplied if the event is an external event.");
            }

            if (!model.Metadata.HasValue || string.IsNullOrEmpty(model.Metadata.Value.Location))
            {
                throw new ArgumentException("Location must be supplied if the event is an external event.");
            }

            if (model.Channel.HasValue && model.Channel.Value is not null)
            {
                throw new ArgumentException("Channel must not be supplied if the event is an external event.");
            }
        }

        // We only have an ID to work off of, so we have no validation as to the current state of the event.
        return model.Status.HasValue && model.Status.Value is DiscordScheduledGuildEventStatus.Scheduled
            ? throw new ArgumentException("Status cannot be set to scheduled.")
            : await this.ApiClient.ModifyScheduledGuildEventAsync(
            guildId, eventId,
            model.Name, model.Description,
            model.Channel.IfPresent(c => c?.Id),
            model.StartTime, model.EndTime,
            model.Type, model.PrivacyLevel,
            model.Metadata, model.Status);
    }

    /// <summary>
    /// Gets the users interested in the guild event.
    /// </summary>
    /// <param name="guildId">The ID of the guild the event resides on.</param>
    /// <param name="eventId">The ID of the event.</param>
    /// <param name="limit">How many users to query.</param>
    /// <param name="after">Fetch users after this ID.</param>
    /// <param name="before">Fetch users before this ID.</param>
    /// <returns>The users interested in the event.</returns>
    public async Task<IReadOnlyList<DiscordUser>> GetScheduledGuildEventUsersAsync(ulong guildId, ulong eventId, int limit = 100, ulong? after = null, ulong? before = null)
    {
        int remaining = limit;
        ulong? last = null;
        bool isAfter = after is not null;

        List<DiscordUser> users = new();

        int lastCount;
        do
        {
            int fetchSize = remaining > 100 ? 100 : remaining;
            IReadOnlyList<DiscordUser> fetch = await this.ApiClient.GetScheduledGuildEventUsersAsync(guildId, eventId, true, fetchSize, !isAfter ? last ?? before : null, isAfter ? last ?? after : null);

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

    #endregion

    #region Guild

    /// <summary>
    /// Searches the given guild for members who's display name start with the specified name.
    /// </summary>
    /// <param name="guildId">The ID of the guild to search.</param>
    /// <param name="name">The name to search for.</param>
    /// <param name="limit">The maximum amount of members to return. Max 1000. Defaults to 1.</param>
    /// <returns>The members found, if any.</returns>
    public async Task<IReadOnlyList<DiscordMember>> SearchMembersAsync(ulong guildId, string name, int? limit = 1)
        => await this.ApiClient.SearchMembersAsync(guildId, name, limit);

    /// <summary>
    /// Creates a new guild
    /// </summary>
    /// <param name="name">New guild's name</param>
    /// <param name="regionId">New guild's region ID</param>
    /// <param name="iconb64">New guild's icon (base64)</param>
    /// <param name="verificationLevel">New guild's verification level</param>
    /// <param name="defaultMessageNotifications">New guild's default message notification level</param>
    /// <param name="systemChannelFlags">New guild's system channel flags</param>
    /// <returns></returns>
    public async Task<DiscordGuild> CreateGuildAsync(string name, string regionId, string iconb64, DiscordVerificationLevel? verificationLevel, DiscordDefaultMessageNotifications? defaultMessageNotifications, DiscordSystemChannelFlags? systemChannelFlags)
        => await this.ApiClient.CreateGuildAsync(name, regionId, iconb64, verificationLevel, defaultMessageNotifications, systemChannelFlags);

    /// <summary>
    /// Creates a guild from a template. This requires the bot to be in less than 10 guilds total.
    /// </summary>
    /// <param name="code">The template code.</param>
    /// <param name="name">Name of the guild.</param>
    /// <param name="icon">Stream containing the icon for the guild.</param>
    /// <returns>The created guild.</returns>
    public async Task<DiscordGuild> CreateGuildFromTemplateAsync(string code, string name, string icon)
        => await this.ApiClient.CreateGuildFromTemplateAsync(code, name, icon);

    /// <summary>
    /// Deletes a guild
    /// </summary>
    /// <param name="id">Guild ID</param>
    /// <returns></returns>
    public async Task DeleteGuildAsync(ulong id)
        => await this.ApiClient.DeleteGuildAsync(id);

    /// <summary>
    /// Modifies a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="name">New guild Name</param>
    /// <param name="region">New guild voice region</param>
    /// <param name="verificationLevel">New guild verification level</param>
    /// <param name="defaultMessageNotifications">New guild default message notification level</param>
    /// <param name="mfaLevel">New guild MFA level</param>
    /// <param name="explicitContentFilter">New guild explicit content filter level</param>
    /// <param name="afkChannelId">New guild AFK channel ID</param>
    /// <param name="afkTimeout">New guild AFK timeout in seconds</param>
    /// <param name="iconb64">New guild icon (base64)</param>
    /// <param name="ownerId">New guild owner ID</param>
    /// <param name="splashb64">New guild splash (base64)</param>
    /// <param name="systemChannelId">New guild system channel ID</param>
    /// <param name="banner">New guild banner</param>
    /// <param name="description">New guild description</param>
    /// <param name="discorverySplash">New guild Discovery splash</param>
    /// <param name="features">List of new <see href="https://discord.com/developers/docs/resources/guild#guild-object-guild-features">guild features</see></param>
    /// <param name="preferredLocale">New preferred locale</param>
    /// <param name="publicUpdatesChannelId">New updates channel ID</param>
    /// <param name="rulesChannelId">New rules channel ID</param>
    /// <param name="systemChannelFlags">New system channel flags</param>
    /// <param name="reason">Modify reason</param>
    /// <returns></returns>
    public async Task<DiscordGuild> ModifyGuildAsync(ulong guildId, Optional<string> name,
        Optional<string> region, Optional<DiscordVerificationLevel> verificationLevel,
        Optional<DiscordDefaultMessageNotifications> defaultMessageNotifications, Optional<DiscordMfaLevel> mfaLevel,
        Optional<DiscordExplicitContentFilter> explicitContentFilter, Optional<ulong?> afkChannelId,
        Optional<int> afkTimeout, Optional<string> iconb64, Optional<ulong> ownerId, Optional<string> splashb64,
        Optional<ulong?> systemChannelId, Optional<string> banner, Optional<string> description,
        Optional<string> discorverySplash, Optional<IEnumerable<string>> features, Optional<string> preferredLocale,
        Optional<ulong?> publicUpdatesChannelId, Optional<ulong?> rulesChannelId, Optional<DiscordSystemChannelFlags> systemChannelFlags,
        string reason)
        => await this.ApiClient.ModifyGuildAsync(guildId, name, region, verificationLevel, defaultMessageNotifications, mfaLevel, explicitContentFilter, afkChannelId, afkTimeout, iconb64,
            ownerId, splashb64, systemChannelId, banner, description, discorverySplash, features, preferredLocale, publicUpdatesChannelId, rulesChannelId, systemChannelFlags, reason);

    /// <summary>
    /// Modifies a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="action">Guild modifications</param>
    /// <returns></returns>
    public async Task<DiscordGuild> ModifyGuildAsync(ulong guildId, Action<GuildEditModel> action)
    {
        GuildEditModel mdl = new();
        action(mdl);

        if (mdl.AfkChannel.HasValue)
        {
            if (mdl.AfkChannel.Value.Type != DiscordChannelType.Voice)
            {
                throw new ArgumentException("AFK channel needs to be a voice channel!");
            }
        }

        Optional<string> iconb64 = Optional.FromNoValue<string>();
        if (mdl.Icon.HasValue && mdl.Icon.Value is not null)
        {
            using ImageTool imgtool = new(mdl.Icon.Value);
            iconb64 = imgtool.GetBase64();
        }
        else if (mdl.Icon.HasValue)
        {
            iconb64 = null;
        }

        Optional<string> splashb64 = Optional.FromNoValue<string>();
        if (mdl.Splash.HasValue && mdl.Splash.Value is not null)
        {
            using ImageTool imgtool = new(mdl.Splash.Value);
            splashb64 = imgtool.GetBase64();
        }
        else if (mdl.Splash.HasValue)
        {
            splashb64 = null;
        }

        Optional<string> bannerb64 = Optional.FromNoValue<string>();

        if (mdl.Banner.HasValue && mdl.Banner.Value is not null)
        {
            using ImageTool imgtool = new(mdl.Banner.Value);
            bannerb64 = imgtool.GetBase64();
        }
        else if (mdl.Banner.HasValue)
        {
            bannerb64 = null;
        }

        return await this.ApiClient.ModifyGuildAsync(guildId, mdl.Name, mdl.Region.IfPresent(x => x.Id), mdl.VerificationLevel, mdl.DefaultMessageNotifications,
            mdl.MfaLevel, mdl.ExplicitContentFilter, mdl.AfkChannel.IfPresent(x => x?.Id), mdl.AfkTimeout, iconb64, mdl.Owner.IfPresent(x => x.Id),
            splashb64, mdl.SystemChannel.IfPresent(x => x?.Id), bannerb64, mdl.Description, mdl.DiscoverySplash, mdl.Features, mdl.PreferredLocale,
            mdl.PublicUpdatesChannel.IfPresent(e => e?.Id), mdl.RulesChannel.IfPresent(e => e?.Id), mdl.SystemChannelFlags, mdl.AuditLogReason);
    }

    /// <summary>
    /// Gets guild bans.
    /// </summary>
    /// <param name="guildId">The ID of the guild to get the bans from.</param>
    /// <param name="limit">The number of users to return (up to maximum 1000, default 1000).</param>
    /// <param name="before">Consider only users before the given user ID.</param>
    /// <param name="after">Consider only users after the given user ID.</param>
    /// <returns>A collection of the guild's bans.</returns>
    public async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guildId, int? limit = null, ulong? before = null, ulong? after = null)
        => await this.ApiClient.GetGuildBansAsync(guildId, limit, before, after);

    /// <summary>
    /// Gets the ban of the specified user. Requires Ban Members permission.
    /// </summary>
    /// <param name="guildId">The ID of the guild to get the ban from.</param>
    /// <param name="userId">The ID of the user to get the ban for.</param>
    /// <returns>A guild ban object.</returns>
    public async Task<DiscordBan> GetGuildBanAsync(ulong guildId, ulong userId)
        => await this.ApiClient.GetGuildBanAsync(guildId, userId);

    /// <summary>
    /// Creates guild ban
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="deleteMessageDays">Days to delete messages</param>
    /// <param name="reason">Reason why this member was banned</param>
    /// <returns></returns>
    public async Task CreateGuildBanAsync(ulong guildId, ulong userId, int deleteMessageDays, string reason)
        => await this.ApiClient.CreateGuildBanAsync(guildId, userId, deleteMessageDays, reason);
    
    /// <summary>
    /// Creates multiple guild bans
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userIds">Collection of user ids to ban</param>
    /// <param name="deleteMessageSeconds">Timespan in seconds to delete messages from the banned users</param>
    /// <param name="reason">Auditlog reason</param>
    /// <returns></returns>
    public async Task<DiscordBulkBan> CreateGuildBansAsync(ulong guildId, IEnumerable<ulong> userIds, int deleteMessageSeconds, string reason)
        => await this.ApiClient.CreateGuildBulkBanAsync(guildId, userIds, deleteMessageSeconds, reason);

    /// <summary>
    /// Removes a guild ban
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User to unban</param>
    /// <param name="reason">Reason why this member was unbanned</param>
    /// <returns></returns>
    public async Task RemoveGuildBanAsync(ulong guildId, ulong userId, string reason)
        => await this.ApiClient.RemoveGuildBanAsync(guildId, userId, reason);

    /// <summary>
    /// Leaves a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns></returns>
    public async Task LeaveGuildAsync(ulong guildId)
        => await this.ApiClient.LeaveGuildAsync(guildId);

    /// <summary>
    /// Adds a member to a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="accessToken">Access token</param>
    /// <param name="nick">User nickname</param>
    /// <param name="roles">User roles</param>
    /// <param name="muted">Whether this user should be muted on join</param>
    /// <param name="deafened">Whether this user should be deafened on join</param>
    /// <returns>Only returns the member if they were not already in the guild</returns>
    public async Task<DiscordMember?> AddGuildMemberAsync(ulong guildId, ulong userId, string accessToken, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
        => await this.ApiClient.AddGuildMemberAsync(guildId, userId, accessToken, muted, deafened, nick, roles);

    /// <summary>
    /// Gets all guild members
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="limit">Member download limit</param>
    /// <param name="after">Gets members after this ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordMember>> ListGuildMembersAsync(ulong guildId, int? limit, ulong? after)
    {
        List<DiscordMember> recmbr = new();

        int recd = limit ?? 1000;
        int lim = limit ?? 1000;
        ulong? last = after;
        while (recd == lim)
        {
            IReadOnlyList<TransportMember> tms = await this.ApiClient.ListGuildMembersAsync(guildId, lim, last == 0 ? null : last);
            recd = tms.Count;

            foreach (TransportMember xtm in tms)
            {
                last = xtm.User.Id;

                if (this.UserCache.ContainsKey(xtm.User.Id))
                {
                    continue;
                }

                DiscordUser usr = new(xtm.User)
                {
                    Discord = this
                };

                this.UpdateUserCache(usr);
            }

            recmbr.AddRange(tms.Select(xtm => new DiscordMember(xtm) { Discord = this, _guild_id = guildId }));
        }

        return new ReadOnlyCollection<DiscordMember>(recmbr);
    }

    /// <summary>
    /// Add role to guild member
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="reason">Reason this role gets added</param>
    /// <returns></returns>
    public async Task AddGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string reason)
        => await this.ApiClient.AddGuildMemberRoleAsync(guildId, userId, roleId, reason);

    /// <summary>
    /// Remove role from member
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="reason">Reason this role gets removed</param>
    /// <returns></returns>
    public async Task RemoveGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string reason)
        => await this.ApiClient.RemoveGuildMemberRoleAsync(guildId, userId, roleId, reason);

    /// <summary>
    /// Updates a role's position
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="position">Role position</param>
    /// <param name="reason">Reason this position was modified</param>
    /// <returns></returns>
    public async Task UpdateRolePositionAsync(ulong guildId, ulong roleId, int position, string reason = null)
    {
        List<RestGuildRoleReorderPayload> rgrrps = new()
        {
            new()
            {
                RoleId = roleId
            }
        };
        await this.ApiClient.ModifyGuildRolePositionsAsync(guildId, rgrrps, reason);
    }

    /// <summary>
    /// Updates a channel's position
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="channelId">Channel ID</param>
    /// <param name="position">Channel position</param>
    /// <param name="reason">Reason this position was modified</param>
    /// <param name="lockPermissions">Whether to sync channel permissions with the parent, if moving to a new category.</param>
    /// <param name="parentId">The new parent id if the channel is to be moved to a new category.</param>
    /// <returns></returns>
    public async Task UpdateChannelPositionAsync(ulong guildId, ulong channelId, int position, string reason, bool? lockPermissions = null, ulong? parentId = null)
    {
        List<RestGuildChannelReorderPayload> rgcrps = new()
        {
            new()
            {
                ChannelId = channelId,
                Position = position,
                LockPermissions = lockPermissions,
                ParentId = parentId
            }
        };
        await this.ApiClient.ModifyGuildChannelPositionAsync(guildId, rgcrps, reason);
    }

    /// <summary>
    /// Gets a guild's widget
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns></returns>
    public async Task<DiscordWidget> GetGuildWidgetAsync(ulong guildId)
        => await this.ApiClient.GetGuildWidgetAsync(guildId);

    /// <summary>
    /// Gets a guild's widget settings
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns></returns>
    public async Task<DiscordWidgetSettings> GetGuildWidgetSettingsAsync(ulong guildId)
        => await this.ApiClient.GetGuildWidgetSettingsAsync(guildId);

    /// <summary>
    /// Modifies a guild's widget settings
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="enabled">If the widget is enabled or not</param>
    /// <param name="channelId">Widget channel ID</param>
    /// <param name="reason">Reason the widget settings were modified</param>
    /// <returns></returns>
    public async Task<DiscordWidgetSettings> ModifyGuildWidgetSettingsAsync(ulong guildId, bool? enabled = null, ulong? channelId = null, string reason = null)
        => await this.ApiClient.ModifyGuildWidgetSettingsAsync(guildId, enabled, channelId, reason);

    /// <summary>
    /// Gets a guild's membership screening form.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns>The guild's membership screening form.</returns>
    public async Task<DiscordGuildMembershipScreening> GetGuildMembershipScreeningFormAsync(ulong guildId)
        => await this.ApiClient.GetGuildMembershipScreeningFormAsync(guildId);

    /// <summary>
    /// Modifies a guild's membership screening form.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="action">Action to perform</param>
    /// <returns>The modified screening form.</returns>
    public async Task<DiscordGuildMembershipScreening> ModifyGuildMembershipScreeningFormAsync(ulong guildId, Action<MembershipScreeningEditModel> action)
    {
        MembershipScreeningEditModel mdl = new();
        action(mdl);
        return await this.ApiClient.ModifyGuildMembershipScreeningFormAsync(guildId, mdl.Enabled, mdl.Fields, mdl.Description);
    }

    /// <summary>
    /// Gets a guild's vanity url
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <returns>The guild's vanity url.</returns>
    public async Task<DiscordInvite> GetGuildVanityUrlAsync(ulong guildId)
        => await this.ApiClient.GetGuildVanityUrlAsync(guildId);

    /// <summary>
    /// Updates the current user's suppress state in a stage channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="suppress">Toggles the suppress state.</param>
    /// <param name="requestToSpeakTimestamp">Sets the time the user requested to speak.</param>
    public async Task UpdateCurrentUserVoiceStateAsync(ulong guildId, ulong channelId, bool? suppress, DateTimeOffset? requestToSpeakTimestamp = null)
        => await this.ApiClient.UpdateCurrentUserVoiceStateAsync(guildId, channelId, suppress, requestToSpeakTimestamp);

    /// <summary>
    /// Updates a member's suppress state in a stage channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="userId">The ID of the member.</param>
    /// <param name="channelId">The ID of the stage channel.</param>
    /// <param name="suppress">Toggles the member's suppress state.</param>
    /// <returns></returns>
    public async Task UpdateUserVoiceStateAsync(ulong guildId, ulong userId, ulong channelId, bool? suppress)
        => await this.ApiClient.UpdateUserVoiceStateAsync(guildId, userId, channelId, suppress);
    #endregion

    #region Channel
    /// <summary>
    /// Creates a guild channel
    /// </summary>
    /// <param name="id">Channel ID</param>
    /// <param name="name">Channel name</param>
    /// <param name="type">Channel type</param>
    /// <param name="parent">Channel parent ID</param>
    /// <param name="topic">Channel topic</param>
    /// <param name="bitrate">Voice channel bitrate</param>
    /// <param name="userLimit">Voice channel user limit</param>
    /// <param name="overwrites">Channel overwrites</param>
    /// <param name="nsfw">Whether this channel should be marked as NSFW</param>
    /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
    /// <param name="qualityMode">Voice channel video quality mode.</param>
    /// <param name="position">Sorting position of the channel.</param>
    /// <param name="reason">Reason this channel was created</param>
    /// <param name="defaultAutoArchiveDuration">Default duration for newly created forum posts in the channel.</param>
    /// <param name="defaultReactionEmoji">Default emoji used for reacting to forum posts.</param>
    /// <param name="availableTags">Tags available for use by forum posts in the channel.</param>
    /// <param name="defaultSortOrder">Default sorting order for forum posts in the channel.</param>
    /// <returns></returns>
    public async Task<DiscordChannel> CreateGuildChannelAsync
    (
        ulong id,
        string name,
        DiscordChannelType type,
        ulong? parent,
        Optional<string> topic,
        int? bitrate,
        int? userLimit,
        IEnumerable<DiscordOverwriteBuilder> overwrites,
        bool? nsfw,
        Optional<int?> perUserRateLimit,
        DiscordVideoQualityMode? qualityMode,
        int? position,
        string reason,
        DiscordAutoArchiveDuration? defaultAutoArchiveDuration = null,
        DefaultReaction? defaultReactionEmoji = null,
        IEnumerable<DiscordForumTagBuilder> availableTags = null,
        DiscordDefaultSortOrder? defaultSortOrder = null
    )
    {
        return type is not (DiscordChannelType.Text or DiscordChannelType.Voice or DiscordChannelType.Category or DiscordChannelType.News or DiscordChannelType.Stage or DiscordChannelType.GuildForum)
            ? throw new ArgumentException("Channel type must be text, voice, stage, category, or a forum.", nameof(type))
            : await this.ApiClient.CreateGuildChannelAsync
        (
            id,
            name,
            type,
            parent,
            topic,
            bitrate,
            userLimit,
            overwrites,
            nsfw,
            perUserRateLimit,
            qualityMode,
            position,
            reason,
            defaultAutoArchiveDuration,
            defaultReactionEmoji,
            availableTags,
            defaultSortOrder
        );
    }

    /// <summary>
    /// Modifies a channel
    /// </summary>
    /// <param name="id">Channel ID</param>
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
    /// <param name="flags">Channel flags.</param>
    /// <param name="defaultAutoArchiveDuration">Default duration for newly created forum posts in the channel.</param>
    /// <param name="defaultReactionEmoji">Default emoji used for reacting to forum posts.</param>
    /// <param name="availableTags">Tags available for use by forum posts in the channel.</param>
    /// <param name="defaultPerUserRatelimit">Default per-user ratelimit for forum posts in the channel.</param>
    /// <param name="defaultSortOrder">Default sorting order for forum posts in the channel.</param>
    /// <param name="defaultForumLayout">Default layout for forum posts in the channel.</param>
    /// <returns></returns>
    public async Task ModifyChannelAsync
    (
        ulong id,
        string name,
        int? position,
        Optional<string> topic,
        bool? nsfw,
        Optional<ulong?> parent,
        int? bitrate,
        int? userLimit,
        Optional<int?> perUserRateLimit,
        Optional<DiscordVoiceRegion> rtcRegion,
        DiscordVideoQualityMode? qualityMode,
        Optional<DiscordChannelType> type,
        IEnumerable<DiscordOverwriteBuilder> permissionOverwrites,
        string reason,
        Optional<DiscordChannelFlags> flags,
        IEnumerable<DiscordForumTagBuilder>? availableTags,
        Optional<DiscordAutoArchiveDuration?> defaultAutoArchiveDuration,
        Optional<DefaultReaction?> defaultReactionEmoji,
        Optional<int> defaultPerUserRatelimit,
        Optional<DiscordDefaultSortOrder?> defaultSortOrder,
        Optional<DiscordDefaultForumLayout> defaultForumLayout
    )
        => await this.ApiClient.ModifyChannelAsync
        (
            id,
            name,
            position,
            topic,
            nsfw,
            parent,
            bitrate,
            userLimit,
            perUserRateLimit,
            rtcRegion.IfPresent(e => e?.Id),
            qualityMode,
            type,
            permissionOverwrites,
            flags,
            availableTags,
            defaultAutoArchiveDuration,
            defaultReactionEmoji,
            defaultPerUserRatelimit,
            defaultSortOrder,
            defaultForumLayout,
            reason
        );

    /// <summary>
    /// Modifies a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="action">Channel modifications</param>
    /// <returns></returns>
    public async Task ModifyChannelAsync(ulong channelId, Action<ChannelEditModel> action)
    {
        ChannelEditModel mdl = new();
        action(mdl);

        await this.ApiClient.ModifyChannelAsync
        (
            channelId, mdl.Name,
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
    /// Gets a channel object
    /// </summary>
    /// <param name="id">Channel ID</param>
    /// <returns></returns>
    public async Task<DiscordChannel> GetChannelAsync(ulong id)
        => await this.ApiClient.GetChannelAsync(id);

    /// <summary>
    /// Deletes a channel
    /// </summary>
    /// <param name="id">Channel ID</param>
    /// <param name="reason">Reason why this channel was deleted</param>
    /// <returns></returns>
    public async Task DeleteChannelAsync(ulong id, string reason)
        => await this.ApiClient.DeleteChannelAsync(id, reason);

    /// <summary>
    /// Gets message in a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <returns></returns>
    public async Task<DiscordMessage> GetMessageAsync(ulong channelId, ulong messageId)
        => await this.ApiClient.GetMessageAsync(channelId, messageId);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="content">Message (text) content</param>
    /// <returns></returns>
    public async Task<DiscordMessage> CreateMessageAsync(ulong channelId, string content)
        => await this.ApiClient.CreateMessageAsync(channelId, content, null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="embed">Embed to attach</param>
    /// <returns></returns>
    public async Task<DiscordMessage> CreateMessageAsync(ulong channelId, DiscordEmbed embed)
        => await this.ApiClient.CreateMessageAsync(channelId, null, embed is not null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="content">Message (text) content</param>
    /// <param name="embed">Embed to attach</param>
    /// <returns></returns>
    public async Task<DiscordMessage> CreateMessageAsync(ulong channelId, string content, DiscordEmbed embed)
        => await this.ApiClient.CreateMessageAsync(channelId, content, embed is not null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="builder">The Discord Message builder.</param>
    /// <returns></returns>
    public async Task<DiscordMessage> CreateMessageAsync(ulong channelId, DiscordMessageBuilder builder)
        => await this.ApiClient.CreateMessageAsync(channelId, builder);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="action">The Discord Message builder.</param>
    /// <returns></returns>
    public async Task<DiscordMessage> CreateMessageAsync(ulong channelId, Action<DiscordMessageBuilder> action)
    {
        DiscordMessageBuilder builder = new();
        action(builder);
        return await this.ApiClient.CreateMessageAsync(channelId, builder);
    }

    /// <summary>
    /// Gets channels from a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guildId)
        => await this.ApiClient.GetGuildChannelsAsync(guildId);

    /// <summary>
    /// Gets messages from a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="limit">Limit of messages to get</param>
    /// <param name="before">Gets messages before this ID</param>
    /// <param name="after">Gets messages after this ID</param>
    /// <param name="around">Gets messages around this ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channelId, int limit, ulong? before, ulong? after, ulong? around)
        => await this.ApiClient.GetChannelMessagesAsync(channelId, limit, before, after, around);

    /// <summary>
    /// Gets a message from a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <returns></returns>
    public async Task<DiscordMessage> GetChannelMessageAsync(ulong channelId, ulong messageId)
        => await this.ApiClient.GetChannelMessageAsync(channelId, messageId);

    /// <summary>
    /// Edits a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="content">New message content</param>
    /// <returns></returns>
    public async Task<DiscordMessage> EditMessageAsync(ulong channelId, ulong messageId, Optional<string> content)
        => await this.ApiClient.EditMessageAsync(channelId, messageId, content, default, default, default, Array.Empty<DiscordMessageFile>());

    /// <summary>
    /// Edits a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="embed">New message embed</param>
    /// <returns></returns>
    public async Task<DiscordMessage> EditMessageAsync(ulong channelId, ulong messageId, Optional<DiscordEmbed> embed)
        => await this.ApiClient.EditMessageAsync(channelId, messageId, default, embed.HasValue ? new[] { embed.Value } : Array.Empty<DiscordEmbed>(), default, default, Array.Empty<DiscordMessageFile>());

    /// <summary>
    /// Edits a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="builder">The builder of the message to edit.</param>
    /// <param name="suppressEmbeds">Whether to suppress embeds on the message.</param>
    /// <param name="attachments">Attached files to keep.</param>
    /// <returns></returns>
    public async Task<DiscordMessage> EditMessageAsync(ulong channelId, ulong messageId, DiscordMessageBuilder builder, bool suppressEmbeds = false, IEnumerable<DiscordAttachment> attachments = default)
    {
        builder.Validate();

        return await this.ApiClient.EditMessageAsync(channelId, messageId, builder.Content, new Optional<IEnumerable<DiscordEmbed>>(builder.Embeds), builder._mentions, builder.Components, builder.Files, suppressEmbeds ? DiscordMessageFlags.SuppressedEmbeds : null, attachments);
    }

    /// <summary>
    /// Modifies the visibility of embeds in a message.
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="hideEmbeds">Whether to hide all embeds.</param>
    public async Task ModifyEmbedSuppressionAsync(ulong channelId, ulong messageId, bool hideEmbeds)
        => await this.ApiClient.EditMessageAsync(channelId, messageId, default, default, default, default, Array.Empty<DiscordMessageFile>(), hideEmbeds ? DiscordMessageFlags.SuppressedEmbeds : null);

    /// <summary>
    /// Deletes a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="reason">Why this message was deleted</param>
    /// <returns></returns>
    public async Task DeleteMessageAsync(ulong channelId, ulong messageId, string reason)
        => await this.ApiClient.DeleteMessageAsync(channelId, messageId, reason);

    /// <summary>
    /// Deletes multiple messages
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageIds">Message IDs</param>
    /// <param name="reason">Reason these messages were deleted</param>
    /// <returns></returns>
    public async Task DeleteMessagesAsync(ulong channelId, IEnumerable<ulong> messageIds, string reason)
        => await this.ApiClient.DeleteMessagesAsync(channelId, messageIds, reason);

    /// <summary>
    /// Gets a channel's invites
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channelId)
        => await this.ApiClient.GetChannelInvitesAsync(channelId);

    /// <summary>
    /// Creates a channel invite
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="maxAge">For how long the invite should exist</param>
    /// <param name="maxUses">How often the invite may be used</param>
    /// <param name="temporary">Whether this invite should be temporary</param>
    /// <param name="unique">Whether this invite should be unique (false might return an existing invite)</param>
    /// <param name="reason">Why you made an invite</param>
    /// <param name="targetType">The target type of the invite, for stream and embedded application invites.</param>
    /// <param name="targetUserId">The ID of the target user.</param>
    /// <param name="targetApplicationId">The ID of the target application.</param>
    /// <returns></returns>
    public async Task<DiscordInvite> CreateChannelInviteAsync(ulong channelId, int maxAge, int maxUses, bool temporary, bool unique, string reason, DiscordInviteTargetType? targetType = null, ulong? targetUserId = null, ulong? targetApplicationId = null)
        => await this.ApiClient.CreateChannelInviteAsync(channelId, maxAge, maxUses, temporary, unique, reason, targetType, targetUserId, targetApplicationId);

    /// <summary>
    /// Deletes channel overwrite
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="overwriteId">Overwrite ID</param>
    /// <param name="reason">Reason it was deleted</param>
    /// <returns></returns>
    public async Task DeleteChannelPermissionAsync(ulong channelId, ulong overwriteId, string reason)
        => await this.ApiClient.DeleteChannelPermissionAsync(channelId, overwriteId, reason);

    /// <summary>
    /// Edits channel overwrite
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="overwriteId">Overwrite ID</param>
    /// <param name="allow">Permissions to allow</param>
    /// <param name="deny">Permissions to deny</param>
    /// <param name="type">Overwrite type</param>
    /// <param name="reason">Reason this overwrite was created</param>
    /// <returns></returns>
    public async Task EditChannelPermissionsAsync(ulong channelId, ulong overwriteId, DiscordPermissions allow, DiscordPermissions deny, string type, string reason)
        => await this.ApiClient.EditChannelPermissionsAsync(channelId, overwriteId, allow, deny, type, reason);

    /// <summary>
    /// Send a typing indicator to a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <returns></returns>
    public async Task TriggerTypingAsync(ulong channelId)
        => await this.ApiClient.TriggerTypingAsync(channelId);

    /// <summary>
    /// Gets pinned messages
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channelId)
        => await this.ApiClient.GetPinnedMessagesAsync(channelId);

    /// <summary>
    /// Unpins a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <returns></returns>
    public async Task UnpinMessageAsync(ulong channelId, ulong messageId)
        => await this.ApiClient.UnpinMessageAsync(channelId, messageId);

    /// <summary>
    /// Joins a group DM
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="nickname">DM nickname</param>
    /// <returns></returns>
    public async Task JoinGroupDmAsync(ulong channelId, string nickname)
        => await this.ApiClient.AddGroupDmRecipientAsync(channelId, this.CurrentUser.Id, this.Configuration.Token, nickname);

    /// <summary>
    /// Adds a member to a group DM
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="accessToken">User's access token</param>
    /// <param name="nickname">Nickname for user</param>
    /// <returns></returns>
    public async Task GroupDmAddRecipientAsync(ulong channelId, ulong userId, string accessToken, string nickname)
        => await this.ApiClient.AddGroupDmRecipientAsync(channelId, userId, accessToken, nickname);

    /// <summary>
    /// Leaves a group DM
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <returns></returns>
    public async Task LeaveGroupDmAsync(ulong channelId)
        => await this.ApiClient.RemoveGroupDmRecipientAsync(channelId, this.CurrentUser.Id);

    /// <summary>
    /// Removes a member from a group DM
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="userId">User ID</param>
    /// <returns></returns>
    public async Task GroupDmRemoveRecipientAsync(ulong channelId, ulong userId)
        => await this.ApiClient.RemoveGroupDmRecipientAsync(channelId, userId);

    /// <summary>
    /// Creates a group DM
    /// </summary>
    /// <param name="accessTokens">Access tokens</param>
    /// <param name="nicks">Nicknames per user</param>
    /// <returns></returns>
    public async Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> accessTokens, IDictionary<ulong, string> nicks)
        => await this.ApiClient.CreateGroupDmAsync(accessTokens, nicks);

    /// <summary>
    /// Creates a group DM with current user
    /// </summary>
    /// <param name="accessTokens">Access tokens</param>
    /// <param name="nicks">Nicknames</param>
    /// <returns></returns>
    public async Task<DiscordDmChannel> CreateGroupDmWithCurrentUserAsync(IEnumerable<string> accessTokens, IDictionary<ulong, string> nicks)
    {
        List<string> a = accessTokens.ToList();
        a.Add(this.Configuration.Token);
        return await this.ApiClient.CreateGroupDmAsync(a, nicks);
    }

    /// <summary>
    /// Creates a DM
    /// </summary>
    /// <param name="recipientId">Recipient user ID</param>
    /// <returns></returns>
    public async Task<DiscordDmChannel> CreateDmAsync(ulong recipientId)
        => await this.ApiClient.CreateDmAsync(recipientId);

    /// <summary>
    /// Follows a news channel
    /// </summary>
    /// <param name="channelId">ID of the channel to follow</param>
    /// <param name="webhookChannelId">ID of the channel to crosspost messages to</param>
    /// <exception cref="UnauthorizedException">Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> on the target channel</exception>
    public async Task<DiscordFollowedChannel> FollowChannelAsync(ulong channelId, ulong webhookChannelId)
        => await this.ApiClient.FollowChannelAsync(channelId, webhookChannelId);

    /// <summary>
    /// Publishes a message in a news channel to following channels
    /// </summary>
    /// <param name="channelId">ID of the news channel the message to crosspost belongs to</param>
    /// <param name="messageId">ID of the message to crosspost</param>
    /// <exception cref="UnauthorizedException">
    ///     Thrown when the current user doesn't have <see cref="DiscordPermissions.ManageWebhooks"/> and/or <see cref="DiscordPermissions.SendMessages"/>
    /// </exception>
    public async Task<DiscordMessage> CrosspostMessageAsync(ulong channelId, ulong messageId)
        => await this.ApiClient.CrosspostMessageAsync(channelId, messageId);

    /// <summary>
    /// Creates a stage instance in a stage channel.
    /// </summary>
    /// <param name="channelId">The ID of the stage channel to create it in.</param>
    /// <param name="topic">The topic of the stage instance.</param>
    /// <param name="privacyLevel">The privacy level of the stage instance.</param>
    /// <param name="reason">The reason the stage instance was created.</param>
    /// <returns>The created stage instance.</returns>
    public async Task<DiscordStageInstance> CreateStageInstanceAsync(ulong channelId, string topic, DiscordStagePrivacyLevel? privacyLevel = null, string reason = null)
        => await this.ApiClient.CreateStageInstanceAsync(channelId, topic, privacyLevel, reason);

    /// <summary>
    /// Gets a stage instance in a stage channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel.</param>
    /// <returns>The stage instance in the channel.</returns>
    public async Task<DiscordStageInstance> GetStageInstanceAsync(ulong channelId)
        => await this.ApiClient.GetStageInstanceAsync(channelId);

    /// <summary>
    /// Modifies a stage instance in a stage channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to modify the stage instance of.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The modified stage instance.</returns>
    public async Task<DiscordStageInstance> ModifyStageInstanceAsync(ulong channelId, Action<StageInstanceEditModel> action)
    {
        StageInstanceEditModel mdl = new();
        action(mdl);
        return await this.ApiClient.ModifyStageInstanceAsync(channelId, mdl.Topic, mdl.PrivacyLevel, mdl.AuditLogReason);
    }

    /// <summary>
    /// Deletes a stage instance in a stage channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to delete the stage instance of.</param>
    /// <param name="reason">The reason the stage instance was deleted.</param>
    public async Task DeleteStageInstanceAsync(ulong channelId, string reason = null)
        => await this.ApiClient.DeleteStageInstanceAsync(channelId, reason);

    /// <summary>
    /// Pins a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel the message is in.</param>
    /// <param name="messageId">The ID of the message.</param>
    public async Task PinMessageAsync(ulong channelId, ulong messageId)
        => await this.ApiClient.PinMessageAsync(channelId, messageId);

    #endregion

    #region Member
    /// <summary>
    /// Gets current user object
    /// </summary>
    /// <returns></returns>
    public async Task<DiscordUser> GetCurrentUserAsync()
        => await this.ApiClient.GetCurrentUserAsync();

    /// <summary>
    /// Gets user object
    /// </summary>
    /// <param name="user">User ID</param>
    /// <returns></returns>
    public async Task<DiscordUser> GetUserAsync(ulong user)
        => await this.ApiClient.GetUserAsync(user);

    /// <summary>
    /// Gets guild member
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="memberId">Member ID</param>
    /// <returns></returns>
    public async Task<DiscordMember> GetGuildMemberAsync(ulong guildId, ulong memberId)
        => await this.ApiClient.GetGuildMemberAsync(guildId, memberId);

    /// <summary>
    /// Removes guild member
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="reason">Why this user was removed</param>
    /// <returns></returns>
    public async Task RemoveGuildMemberAsync(ulong guildId, ulong userId, string reason)
        => await this.ApiClient.RemoveGuildMemberAsync(guildId, userId, reason);

    /// <summary>
    /// Modifies current user
    /// </summary>
    /// <param name="username">New username</param>
    /// <param name="base64Avatar">New avatar (base64)</param>
    /// <returns></returns>
    public async Task<DiscordUser> ModifyCurrentUserAsync(string username, string base64Avatar)
        => new DiscordUser(await this.ApiClient.ModifyCurrentUserAsync(username, base64Avatar)) { Discord = this };

    /// <summary>
    /// Modifies current user
    /// </summary>
    /// <param name="username">username</param>
    /// <param name="avatar">avatar</param>
    /// <returns></returns>
    public async Task<DiscordUser> ModifyCurrentUserAsync(string username = null, Stream avatar = null)
    {
        string av64 = null;
        if (avatar is not null)
        {
            using ImageTool imgtool = new(avatar);
            av64 = imgtool.GetBase64();
        }

        return new DiscordUser(await this.ApiClient.ModifyCurrentUserAsync(username, av64)) { Discord = this };
    }

    /// <summary>
    /// Gets current user's guilds
    /// </summary>
    /// <param name="limit">Limit of guilds to get</param>
    /// <param name="before">Gets guild before ID</param>
    /// <param name="after">Gets guilds after ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit = 100, ulong? before = null, ulong? after = null)
        => await this.ApiClient.GetCurrentUserGuildsAsync(limit, before, after);

    /// <summary>
    /// Gets the guild member for the current user in the specified guild. Only works with bearer tokens with the guilds.members.read scope.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns></returns>
    public async Task<DiscordMember> GetCurrentUserGuildMemberAsync(ulong guildId)
        => await this.ApiClient.GetCurrentUserGuildMemberAsync(guildId);

    /// <summary>
    /// Modifies guild member.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="nick">New nickname</param>
    /// <param name="roleIds">New roles</param>
    /// <param name="mute">Whether this user should be muted</param>
    /// <param name="deaf">Whether this user should be deafened</param>
    /// <param name="voiceChannelId">Voice channel to move this user to</param>
    /// <param name="communicationDisabledUntil">How long this member should be timed out for. Requires MODERATE_MEMBERS permission.</param>
    /// <param name="reason">Reason this user was modified</param>
    /// <returns></returns>
    public async Task ModifyGuildMemberAsync(ulong guildId, ulong userId, Optional<string> nick,
        Optional<IEnumerable<ulong>> roleIds, Optional<bool> mute, Optional<bool> deaf,
        Optional<ulong?> voiceChannelId, Optional<DateTimeOffset?> communicationDisabledUntil, string reason)
        => await this.ApiClient.ModifyGuildMemberAsync(guildId, userId, nick, roleIds, mute, deaf, voiceChannelId, communicationDisabledUntil, reason);

    /// <summary>
    /// Modifies a member
    /// </summary>
    /// <param name="memberId">Member ID</param>
    /// <param name="guildId">Guild ID</param>
    /// <param name="action">Modifications</param>
    /// <returns></returns>
    public async Task ModifyAsync(ulong memberId, ulong guildId, Action<MemberEditModel> action)
    {
        MemberEditModel mdl = new();
        action(mdl);

        if (mdl.VoiceChannel.HasValue && mdl.VoiceChannel.Value is not null && mdl.VoiceChannel.Value.Type != DiscordChannelType.Voice && mdl.VoiceChannel.Value.Type != DiscordChannelType.Stage)
        {
            throw new ArgumentException("Given channel is not a voice or stage channel.", nameof(mdl.VoiceChannel));
        }

        if (mdl.Nickname.HasValue && this.CurrentUser.Id == memberId)
        {
            await this.ApiClient.ModifyCurrentMemberAsync(guildId, mdl.Nickname.Value,
                mdl.AuditLogReason);
            await this.ApiClient.ModifyGuildMemberAsync(guildId, memberId, Optional.FromNoValue<string>(),
                mdl.Roles.IfPresent(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
                mdl.VoiceChannel.IfPresent(e => e?.Id), default, mdl.AuditLogReason);
        }
        else
        {
            await this.ApiClient.ModifyGuildMemberAsync(guildId, memberId, mdl.Nickname,
                mdl.Roles.IfPresent(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
                mdl.VoiceChannel.IfPresent(e => e?.Id), mdl.CommunicationDisabledUntil, mdl.AuditLogReason);
        }
    }

    /// <summary>
    /// Changes the current user in a guild.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="nickname">Nickname to set</param>
    /// <param name="reason">Audit log reason</param>
    /// <returns></returns>
    public async Task ModifyCurrentMemberAsync(ulong guildId, string nickname, string reason)
        => await this.ApiClient.ModifyCurrentMemberAsync(guildId, nickname, reason);

    #endregion

    #region Roles
    /// <summary>
    /// Gets roles
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guildId)
        => await this.ApiClient.GetGuildRolesAsync(guildId);

    /// <summary>
    /// Gets a guild.
    /// </summary>
    /// <param name="guildId">The guild ID to search for.</param>
    /// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
    /// <returns></returns>
    public async Task<DiscordGuild> GetGuildAsync(ulong guildId, bool? withCounts = null)
        => await this.ApiClient.GetGuildAsync(guildId, withCounts);

    /// <summary>
    /// Modifies a role
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="name">New role name</param>
    /// <param name="permissions">New role permissions</param>
    /// <param name="color">New role color</param>
    /// <param name="hoist">Whether this role should be hoisted</param>
    /// <param name="mentionable">Whether this role should be mentionable</param>
    /// <param name="reason">Why this role was modified</param>
    /// <param name="icon">The icon to add to this role</param>
    /// <param name="emoji">The emoji to add to this role. Must be unicode.</param>
    /// <returns></returns>
    public async Task<DiscordRole> ModifyGuildRoleAsync(ulong guildId, ulong roleId, string name, DiscordPermissions? permissions, DiscordColor? color, bool? hoist, bool? mentionable, string reason, Stream icon, DiscordEmoji emoji)
        => await this.ApiClient.ModifyGuildRoleAsync(guildId, roleId, name, permissions, color.HasValue ? color.Value.Value : null, hoist, mentionable, icon, emoji?.ToString(), reason);

    /// <summary>
    /// Modifies a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="guildId">Guild ID</param>
    /// <param name="action">Modifications</param>
    /// <returns></returns>
    public async Task ModifyGuildRoleAsync(ulong roleId, ulong guildId, Action<RoleEditModel> action)
    {
        RoleEditModel mdl = new();
        action(mdl);

        await this.ModifyGuildRoleAsync(guildId, roleId, mdl.Name, mdl.Permissions, mdl.Color, mdl.Hoist, mdl.Mentionable, mdl.AuditLogReason, mdl.Icon, mdl.Emoji);
    }

    /// <summary>
    /// Deletes a role
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="reason">Reason why this role was deleted</param>
    /// <returns></returns>
    public async Task DeleteGuildRoleAsync(ulong guildId, ulong roleId, string reason)
        => await this.ApiClient.DeleteRoleAsync(guildId, roleId, reason);

    /// <summary>
    /// Creates a new role
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="name">Role name</param>
    /// <param name="permissions">Role permissions</param>
    /// <param name="color">Role color</param>
    /// <param name="hoist">Whether this role should be hoisted</param>
    /// <param name="mentionable">Whether this role should be mentionable</param>
    /// <param name="reason">Reason why this role was created</param>
    /// <param name="icon">The icon to add to this role</param>
    /// <param name="emoji">The emoji to add to this role. Must be unicode.</param>
    /// <returns></returns>
    public async Task<DiscordRole> CreateGuildRoleAsync(ulong guildId, string name, DiscordPermissions? permissions, int? color, bool? hoist, bool? mentionable, string reason, Stream icon = null, DiscordEmoji emoji = null)
        => await this.ApiClient.CreateGuildRoleAsync(guildId, name, permissions, color, hoist, mentionable, icon, emoji?.ToString(), reason);
    #endregion

    #region Prune
    /// <summary>
    /// Get a guild's prune count.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="days">Days to check for</param>
    /// <param name="includeRoles">The roles to be included in the prune.</param>
    /// <returns></returns>
    public async Task<int> GetGuildPruneCountAsync(ulong guildId, int days, IEnumerable<ulong> includeRoles)
        => await this.ApiClient.GetGuildPruneCountAsync(guildId, days, includeRoles);

    /// <summary>
    /// Begins a guild prune.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="days">Days to prune for</param>
    /// <param name="computePruneCount">Whether to return the prune count after this method completes. This is discouraged for larger guilds.</param>
    /// <param name="includeRoles">The roles to be included in the prune.</param>
    /// <param name="reason">Reason why this guild was pruned</param>
    /// <returns></returns>
    public async Task<int?> BeginGuildPruneAsync(ulong guildId, int days, bool computePruneCount, IEnumerable<ulong> includeRoles, string reason)
        => await this.ApiClient.BeginGuildPruneAsync(guildId, days, computePruneCount, includeRoles, reason);
    #endregion

    #region GuildVarious
    /// <summary>
    /// Gets guild integrations
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guildId)
        => await this.ApiClient.GetGuildIntegrationsAsync(guildId);

    /// <summary>
    /// Creates guild integration
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="type">Integration type</param>
    /// <param name="id">Integration id</param>
    /// <returns></returns>
    public async Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guildId, string type, ulong id)
        => await this.ApiClient.CreateGuildIntegrationAsync(guildId, type, id);

    /// <summary>
    /// Modifies a guild integration
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="integrationId">Integration ID</param>
    /// <param name="expireBehaviour">Expiration behaviour</param>
    /// <param name="expireGracePeriod">Expiration grace period</param>
    /// <param name="enableEmoticons">Whether to enable emojis for this integration</param>
    /// <returns></returns>
    public async Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guildId, ulong integrationId, int expireBehaviour, int expireGracePeriod, bool enableEmoticons)
        => await this.ApiClient.ModifyGuildIntegrationAsync(guildId, integrationId, expireBehaviour, expireGracePeriod, enableEmoticons);

    /// <summary>
    /// Removes a guild integration
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="integration">Integration to remove</param>
    /// <param name="reason">Reason why this integration was removed</param>
    /// <returns></returns>
    public async Task DeleteGuildIntegrationAsync(ulong guildId, DiscordIntegration integration, string reason = null)
        => await this.ApiClient.DeleteGuildIntegrationAsync(guildId, integration.Id, reason);

    /// <summary>
    /// Syncs guild integration
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="integrationId">Integration ID</param>
    /// <returns></returns>
    public async Task SyncGuildIntegrationAsync(ulong guildId, ulong integrationId)
        => await this.ApiClient.SyncGuildIntegrationAsync(guildId, integrationId);

    /// <summary>
    /// Get a guild's voice region
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guildId)
        => await this.ApiClient.GetGuildVoiceRegionsAsync(guildId);

    /// <summary>
    /// Get a guild's invites
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guildId)
        => await this.ApiClient.GetGuildInvitesAsync(guildId);

    /// <summary>
    /// Gets a guild's templates.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns>All of the guild's templates.</returns>
    public async Task<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync(ulong guildId)
        => await this.ApiClient.GetGuildTemplatesAsync(guildId);

    /// <summary>
    /// Creates a guild template.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="name">Name of the template.</param>
    /// <param name="description">Description of the template.</param>
    /// <returns>The template created.</returns>
    public async Task<DiscordGuildTemplate> CreateGuildTemplateAsync(ulong guildId, string name, string description = null)
        => await this.ApiClient.CreateGuildTemplateAsync(guildId, name, description);

    /// <summary>
    /// Syncs the template to the current guild's state.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="code">The code of the template to sync.</param>
    /// <returns>The template synced.</returns>
    public async Task<DiscordGuildTemplate> SyncGuildTemplateAsync(ulong guildId, string code)
        => await this.ApiClient.SyncGuildTemplateAsync(guildId, code);

    /// <summary>
    /// Modifies the template's metadata.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="code">The template's code.</param>
    /// <param name="name">Name of the template.</param>
    /// <param name="description">Description of the template.</param>
    /// <returns>The template modified.</returns>
    public async Task<DiscordGuildTemplate> ModifyGuildTemplateAsync(ulong guildId, string code, string name = null, string description = null)
        => await this.ApiClient.ModifyGuildTemplateAsync(guildId, code, name, description);

    /// <summary>
    /// Deletes the template.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="code">The code of the template to delete.</param>
    /// <returns>The deleted template.</returns>
    public async Task<DiscordGuildTemplate> DeleteGuildTemplateAsync(ulong guildId, string code)
        => await this.ApiClient.DeleteGuildTemplateAsync(guildId, code);

    /// <summary>
    /// Gets a guild's welcome screen.
    /// </summary>
    /// <returns>The guild's welcome screen object.</returns>
    public async Task<DiscordGuildWelcomeScreen> GetGuildWelcomeScreenAsync(ulong guildId) =>
        await this.ApiClient.GetGuildWelcomeScreenAsync(guildId);

    /// <summary>
    /// Modifies a guild's welcome screen.
    /// </summary>
    /// <param name="guildId">The guild ID to modify.</param>
    /// <param name="action">Action to perform.</param>
    /// <param name="reason">The audit log reason for this action.</param>
    /// <returns>The modified welcome screen.</returns>
    public async Task<DiscordGuildWelcomeScreen> ModifyGuildWelcomeScreenAsync(ulong guildId, Action<WelcomeScreenEditModel> action, string reason = null)
    {
        WelcomeScreenEditModel mdl = new();
        action(mdl);
        return await this.ApiClient.ModifyGuildWelcomeScreenAsync(guildId, mdl.Enabled, mdl.WelcomeChannels, mdl.Description, reason);
    }

    /// <summary>
    /// Gets a guild preview.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    public async Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong guildId)
        => await this.ApiClient.GetGuildPreviewAsync(guildId);

    #endregion

    #region Invites
    /// <summary>
    /// Gets an invite.
    /// </summary>
    /// <param name="inviteCode">The invite code.</param>
    /// <param name="withCounts">Whether to include presence and total member counts in the returned invite.</param>
    /// <param name="withExpiration">Whether to include the expiration date in the returned invite.</param>
    /// <returns></returns>
    public async Task<DiscordInvite> GetInviteAsync(string inviteCode, bool? withCounts = null, bool? withExpiration = null)
        => await this.ApiClient.GetInviteAsync(inviteCode, withCounts, withExpiration);

    /// <summary>
    /// Removes an invite
    /// </summary>
    /// <param name="inviteCode">Invite code</param>
    /// <param name="reason">Reason why this invite was removed</param>
    /// <returns></returns>
    public async Task<DiscordInvite> DeleteInviteAsync(string inviteCode, string reason)
        => await this.ApiClient.DeleteInviteAsync(inviteCode, reason);
    #endregion

    #region Connections
    /// <summary>
    /// Gets current user's connections
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordConnection>> GetUsersConnectionsAsync()
        => await this.ApiClient.GetUsersConnectionsAsync();
    #endregion

    #region Webhooks
    /// <summary>
    /// Creates a new webhook
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="name">Webhook name</param>
    /// <param name="base64Avatar">Webhook avatar (base64)</param>
    /// <param name="reason">Reason why this webhook was created</param>
    /// <returns></returns>
    public async Task<DiscordWebhook> CreateWebhookAsync(ulong channelId, string name, string base64Avatar, string reason)
        => await this.ApiClient.CreateWebhookAsync(channelId, name, base64Avatar, reason);

    /// <summary>
    /// Creates a new webhook
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="name">Webhook name</param>
    /// <param name="avatar">Webhook avatar</param>
    /// <param name="reason">Reason why this webhook was created</param>
    /// <returns></returns>
    public async Task<DiscordWebhook> CreateWebhookAsync(ulong channelId, string name, Stream avatar = null, string reason = null)
    {
        string av64 = null;
        if (avatar is not null)
        {
            using ImageTool imgtool = new(avatar);
            av64 = imgtool.GetBase64();
        }

        return await this.ApiClient.CreateWebhookAsync(channelId, name, av64, reason);
    }

    /// <summary>
    /// Gets all webhooks from a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channelId)
        => await this.ApiClient.GetChannelWebhooksAsync(channelId);

    /// <summary>
    /// Gets all webhooks from a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guildId)
        => await this.ApiClient.GetGuildWebhooksAsync(guildId);

    /// <summary>
    /// Gets a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <returns></returns>
    public async Task<DiscordWebhook> GetWebhookAsync(ulong webhookId)
        => await this.ApiClient.GetWebhookAsync(webhookId);

    /// <summary>
    /// Gets a webhook with its token (when user is not in said guild)
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <returns></returns>
    public async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhookId, string webhookToken)
        => await this.ApiClient.GetWebhookWithTokenAsync(webhookId, webhookToken);

    /// <summary>
    /// Modifies a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="channelId">The new channel ID the webhook should be moved to.</param>
    /// <param name="name">New webhook name</param>
    /// <param name="base64Avatar">New webhook avatar (base64)</param>
    /// <param name="reason">Reason why this webhook was modified</param>
    /// <returns></returns>
    public async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, ulong channelId, string name, string base64Avatar, string reason)
        => await this.ApiClient.ModifyWebhookAsync(webhookId, channelId, name, base64Avatar, reason);

    /// <summary>
    /// Modifies a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="channelId">The new channel ID the webhook should be moved to.</param>
    /// <param name="name">New webhook name</param>
    /// <param name="avatar">New webhook avatar</param>
    /// <param name="reason">Reason why this webhook was modified</param>
    /// <returns></returns>
    public async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, ulong channelId, string name, Stream avatar, string reason)
    {
        string av64 = null;
        if (avatar is not null)
        {
            using ImageTool imgtool = new(avatar);
            av64 = imgtool.GetBase64();
        }

        return await this.ApiClient.ModifyWebhookAsync(webhookId, channelId, name, av64, reason);
    }

    /// <summary>
    /// Modifies a webhook (when user is not in said guild)
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="name">New webhook name</param>
    /// <param name="base64Avatar">New webhook avatar (base64)</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <param name="reason">Reason why this webhook was modified</param>
    /// <returns></returns>
    public async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, string base64Avatar, string webhookToken, string reason)
        => await this.ApiClient.ModifyWebhookAsync(webhookId, name, base64Avatar, webhookToken, reason);

    /// <summary>
    /// Modifies a webhook (when user is not in said guild)
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="name">New webhook name</param>
    /// <param name="avatar">New webhook avatar</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <param name="reason">Reason why this webhook was modified</param>
    /// <returns></returns>
    public async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, Stream avatar, string webhookToken, string reason)
    {
        string av64 = null;
        if (avatar is not null)
        {
            using ImageTool imgtool = new(avatar);
            av64 = imgtool.GetBase64();
        }

        return await this.ApiClient.ModifyWebhookAsync(webhookId, name, av64, webhookToken, reason);
    }

    /// <summary>
    /// Deletes a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="reason">Reason this webhook was deleted</param>
    /// <returns></returns>
    public async Task DeleteWebhookAsync(ulong webhookId, string reason)
        => await this.ApiClient.DeleteWebhookAsync(webhookId, reason);

    /// <summary>
    /// Deletes a webhook (when user is not in said guild)
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="reason">Reason this webhook was removed</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <returns></returns>
    public async Task DeleteWebhookAsync(ulong webhookId, string reason, string webhookToken)
        => await this.ApiClient.DeleteWebhookAsync(webhookId, webhookToken, reason);

    /// <summary>
    /// Sends a message to a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <param name="builder">Webhook builder filled with data to send.</param>
    /// <returns></returns>
    public async Task<DiscordMessage> ExecuteWebhookAsync(ulong webhookId, string webhookToken, DiscordWebhookBuilder builder)
        => await this.ApiClient.ExecuteWebhookAsync(webhookId, webhookToken, builder);

    /// <summary>
    /// Edits a previously-sent webhook message.
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <param name="messageId">The ID of the message to edit.</param>
    /// <param name="builder">The builder of the message to edit.</param>
    /// <param name="attachments">Attached files to keep.</param>
    /// <returns>The modified <see cref="DiscordMessage"/></returns>
    public async Task<DiscordMessage> EditWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments = default)
    {
        builder.Validate(true);

        return await this.ApiClient.EditWebhookMessageAsync(webhookId, webhookToken, messageId, builder, attachments);
    }

    /// <summary>
    /// Deletes a message that was created by a webhook.
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <param name="messageId">The ID of the message to delete</param>
    /// <returns></returns>
    public async Task DeleteWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId)
        => await this.ApiClient.DeleteWebhookMessageAsync(webhookId, webhookToken, messageId);
    #endregion

    #region Reactions
    /// <summary>
    /// Creates a new reaction
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="emoji">Emoji to react</param>
    /// <returns></returns>
    public async Task CreateReactionAsync(ulong channelId, ulong messageId, string emoji)
        => await this.ApiClient.CreateReactionAsync(channelId, messageId, emoji);

    /// <summary>
    /// Deletes own reaction
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="emoji">Emoji to remove from reaction</param>
    /// <returns></returns>
    public async Task DeleteOwnReactionAsync(ulong channelId, ulong messageId, string emoji)
        => await this.ApiClient.DeleteOwnReactionAsync(channelId, messageId, emoji);

    /// <summary>
    /// Deletes someone elses reaction
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="emoji">Emoji to remove</param>
    /// <param name="reason">Reason why this reaction was removed</param>
    /// <returns></returns>
    public async Task DeleteUserReactionAsync(ulong channelId, ulong messageId, ulong userId, string emoji, string reason)
        => await this.ApiClient.DeleteUserReactionAsync(channelId, messageId, userId, emoji, reason);

    /// <summary>
    /// Gets all users that reacted with a specific emoji to a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="emoji">Emoji to check for</param>
    /// <param name="afterId">Whether to search for reactions after this message id.</param>
    /// <param name="limit">The maximum amount of reactions to fetch.</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channelId, ulong messageId, string emoji, ulong? afterId = null, int limit = 25)
        => await this.ApiClient.GetReactionsAsync(channelId, messageId, emoji, afterId, limit);

    /// <summary>
    /// Gets all users that reacted with a specific emoji to a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="emoji">Emoji to check for</param>
    /// <param name="afterId">Whether to search for reactions after this message id.</param>
    /// <param name="limit">The maximum amount of reactions to fetch.</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channelId, ulong messageId, DiscordEmoji emoji, ulong? afterId = null, int limit = 25)
        => await this.ApiClient.GetReactionsAsync(channelId, messageId, emoji.ToReactionString(), afterId, limit);

    /// <summary>
    /// Deletes all reactions from a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="reason">Reason why all reactions were removed</param>
    /// <returns></returns>
    public async Task DeleteAllReactionsAsync(ulong channelId, ulong messageId, string reason)
        => await this.ApiClient.DeleteAllReactionsAsync(channelId, messageId, reason);

    /// <summary>
    /// Deletes all reactions of a specific reaction for a message.
    /// </summary>
    /// <param name="channelid">The ID of the channel.</param>
    /// <param name="messageId">The ID of the message.</param>
    /// <param name="emoji">The emoji to clear.</param>
    /// <returns></returns>
    public async Task DeleteReactionsEmojiAsync(ulong channelid, ulong messageId, string emoji)
        => await this.ApiClient.DeleteReactionsEmojiAsync(channelid, messageId, emoji);

    #endregion

    #region Application Commands
    /// <summary>
    /// Gets all the global application commands for this application.
    /// </summary>
    /// <returns>A list of global application commands.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync() =>
        await this.ApiClient.GetGlobalApplicationCommandsAsync(this.CurrentApplication.Id);

    /// <summary>
    /// Overwrites the existing global application commands. New commands are automatically created and missing commands are automatically deleted.
    /// </summary>
    /// <param name="commands">The list of commands to overwrite with.</param>
    /// <returns>The list of global commands.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(IEnumerable<DiscordApplicationCommand> commands) =>
        await this.ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(this.CurrentApplication.Id, commands);

    /// <summary>
    /// Creates or overwrites a global application command.
    /// </summary>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command.</returns>
    public async Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(DiscordApplicationCommand command) =>
        await this.ApiClient.CreateGlobalApplicationCommandAsync(this.CurrentApplication.Id, command);

    /// <summary>
    /// Gets a global application command by its ID.
    /// </summary>
    /// <param name="commandId">The ID of the command to get.</param>
    /// <returns>The command with the ID.</returns>
    public async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong commandId) =>
        await this.ApiClient.GetGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

    /// <summary>
    /// Edits a global application command.
    /// </summary>
    /// <param name="commandId">The ID of the command to edit.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The edited command.</returns>
    public async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong commandId, Action<ApplicationCommandEditModel> action)
    {
        ApplicationCommandEditModel mdl = new();
        action(mdl);
        ulong applicationId = this.CurrentApplication?.Id ?? (await this.GetCurrentApplicationAsync()).Id;
        return await this.ApiClient.EditGlobalApplicationCommandAsync(applicationId, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.DefaultPermission, mdl.NSFW, default, default, mdl.AllowDMUsage, mdl.DefaultMemberPermissions);
    }

    /// <summary>
    /// Deletes a global application command.
    /// </summary>
    /// <param name="commandId">The ID of the command to delete.</param>
    public async Task DeleteGlobalApplicationCommandAsync(ulong commandId) =>
        await this.ApiClient.DeleteGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

    /// <summary>
    /// Gets all the application commands for a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to get application commands for.</param>
    /// <returns>A list of application commands in the guild.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong guildId) =>
        await this.ApiClient.GetGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId);

    /// <summary>
    /// Overwrites the existing application commands in a guild. New commands are automatically created and missing commands are automatically deleted.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="commands">The list of commands to overwrite with.</param>
    /// <returns>The list of guild commands.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong guildId, IEnumerable<DiscordApplicationCommand> commands) =>
        await this.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId, commands);

    /// <summary>
    /// Creates or overwrites a guild application command.
    /// </summary>
    /// <param name="guildId">The ID of the guild to create the application command in.</param>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command.</returns>
    public async Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong guildId, DiscordApplicationCommand command) =>
        await this.ApiClient.CreateGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, command);

    /// <summary>
    /// Gets a application command in a guild by its ID.
    /// </summary>
    /// <param name="guildId">The ID of the guild the application command is in.</param>
    /// <param name="commandId">The ID of the command to get.</param>
    /// <returns>The command with the ID.</returns>
    public async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
        await this.ApiClient.GetGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

    /// <summary>
    /// Edits a application command in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild the application command is in.</param>
    /// <param name="commandId">The ID of the command to edit.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The edited command.</returns>
    public async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong guildId, ulong commandId, Action<ApplicationCommandEditModel> action)
    {
        ApplicationCommandEditModel mdl = new();
        action(mdl);
        ulong applicationId = this.CurrentApplication?.Id ?? (await this.GetCurrentApplicationAsync()).Id;
        return await this.ApiClient.EditGuildApplicationCommandAsync(applicationId, guildId, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.DefaultPermission, mdl.NSFW, default, default, mdl.AllowDMUsage, mdl.DefaultMemberPermissions);
    }

    /// <summary>
    /// Deletes a application command in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to delete the application command in.</param>
    /// <param name="commandId">The ID of the command.</param>
    public async Task DeleteGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
        await this.ApiClient.DeleteGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

    /// <summary>
    /// Creates a response to an interaction.
    /// </summary>
    /// <param name="interactionId">The ID of the interaction.</param>
    /// <param name="interactionToken">The token of the interaction</param>
    /// <param name="type">The type of the response.</param>
    /// <param name="builder">The data, if any, to send.</param>
    public async Task CreateInteractionResponseAsync(ulong interactionId, string interactionToken, DiscordInteractionResponseType type, DiscordInteractionResponseBuilder builder = null) =>
        await this.ApiClient.CreateInteractionResponseAsync(interactionId, interactionToken, type, builder);

    /// <summary>
    /// Gets the original interaction response.
    /// </summary>
    /// <returns>The original message that was sent. This <b>does not work on ephemeral messages.</b></returns>
    public async Task<DiscordMessage> GetOriginalInteractionResponseAsync(string interactionToken) =>
        await this.ApiClient.GetOriginalInteractionResponseAsync(this.CurrentApplication.Id, interactionToken);

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

        return await this.ApiClient.EditOriginalInteractionResponseAsync(this.CurrentApplication.Id, interactionToken, builder, attachments);
    }

    /// <summary>
    /// Deletes the original interaction response.
    /// <param name="interactionToken">The token of the interaction.</param>
    /// </summary>>
    public async Task DeleteOriginalInteractionResponseAsync(string interactionToken) =>
        await this.ApiClient.DeleteOriginalInteractionResponseAsync(this.CurrentApplication.Id, interactionToken);

    /// <summary>
    /// Creates a follow up message to an interaction.
    /// </summary>
    /// <param name="interactionToken">The token of the interaction.</param>
    /// <param name="builder">The webhook builder.</param>
    /// <returns>The <see cref="DiscordMessage"/> created.</returns>
    public async Task<DiscordMessage> CreateFollowupMessageAsync(string interactionToken, DiscordFollowupMessageBuilder builder)
    {
        builder.Validate();

        return await this.ApiClient.CreateFollowupMessageAsync(this.CurrentApplication.Id, interactionToken, builder);
    }

    /// <summary>
    /// Edits a follow up message.
    /// </summary>
    /// <param name="interactionToken">The token of the interaction.</param>
    /// <param name="messageId">The ID of the follow up message.</param>
    /// <param name="builder">The webhook builder.</param>
    /// <param name="attachments">Attached files to keep.</param>
    /// <returns>The <see cref="DiscordMessage"/> edited.</returns>
    public async Task<DiscordMessage> EditFollowupMessageAsync(string interactionToken, ulong messageId, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments = default)
    {
        builder.Validate(isFollowup: true);

        return await this.ApiClient.EditFollowupMessageAsync(this.CurrentApplication.Id, interactionToken, messageId, builder, attachments);
    }

    /// <summary>
    /// Deletes a follow up message.
    /// </summary>
    /// <param name="interactionToken">The token of the interaction.</param>
    /// <param name="messageId">The ID of the follow up message.</param>
    public async Task DeleteFollowupMessageAsync(string interactionToken, ulong messageId) =>
        await this.ApiClient.DeleteFollowupMessageAsync(this.CurrentApplication.Id, interactionToken, messageId);

    /// <summary>
    /// Gets all application command permissions in a guild.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <returns>A list of permissions.</returns>
    public async Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> GetGuildApplicationCommandsPermissionsAsync(ulong guildId)
        => await this.ApiClient.GetGuildApplicationCommandPermissionsAsync(this.CurrentApplication.Id, guildId);

    /// <summary>
    /// Gets permissions for a application command in a guild.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="commandId">The ID of the command to get them for.</param>
    /// <returns>The permissions.</returns>
    public async Task<DiscordGuildApplicationCommandPermissions> GetGuildApplicationCommandPermissionsAsync(ulong guildId, ulong commandId)
        => await this.ApiClient.GetApplicationCommandPermissionsAsync(this.CurrentApplication.Id, guildId, commandId);

    /// <summary>
    /// Edits permissions for a application command in a guild.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="commandId">The ID of the command to edit permissions for.</param>
    /// <param name="permissions">The list of permissions to use.</param>
    /// <returns>The edited permissions.</returns>
    public async Task<DiscordGuildApplicationCommandPermissions> EditApplicationCommandPermissionsAsync(ulong guildId, ulong commandId, IEnumerable<DiscordApplicationCommandPermission> permissions)
        => await this.ApiClient.EditApplicationCommandPermissionsAsync(this.CurrentApplication.Id, guildId, commandId, permissions);

    /// <summary>
    /// Batch edits permissions for a application command in a guild.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="permissions">The list of permissions to use.</param>
    /// <returns>A list of edited permissions.</returns>
    public async Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> BatchEditApplicationCommandPermissionsAsync(ulong guildId, IEnumerable<DiscordGuildApplicationCommandPermissions> permissions)
        => await this.ApiClient.BatchEditApplicationCommandPermissionsAsync(this.CurrentApplication.Id, guildId, permissions);

    public async Task<DiscordMessage> GetFollowupMessageAsync(string interactionToken, ulong messageId)
        => await this.ApiClient.GetFollowupMessageAsync(this.CurrentApplication.Id, interactionToken, messageId);

    #endregion

    #region Stickers

    /// <summary>
    /// Gets a sticker from a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="stickerId">The ID of the sticker.</param>
    public async Task<DiscordMessageSticker> GetGuildStickerAsync(ulong guildId, ulong stickerId)
        => await this.ApiClient.GetGuildStickerAsync(guildId, stickerId);

    /// <summary>
    /// Gets a sticker by its ID.
    /// </summary>
    /// <param name="stickerId">The ID of the sticker.</param>
    public async Task<DiscordMessageSticker> GetStickerAsync(ulong stickerId)
        => await this.ApiClient.GetStickerAsync(stickerId);

    /// <summary>
    /// Gets a collection of sticker packs that may be used by nitro users.
    /// </summary>
    public async Task<IReadOnlyList<DiscordMessageStickerPack>> GetStickerPacksAsync()
        => await this.ApiClient.GetStickerPacksAsync();

    /// <summary>
    /// Gets a list of stickers from a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    public async Task<IReadOnlyList<DiscordMessageSticker>> GetGuildStickersAsync(ulong guildId)
        => await this.ApiClient.GetGuildStickersAsync(guildId);

    /// <summary>
    /// Creates a sticker in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="name">The name of the sticker.</param>
    /// <param name="description">The description of the sticker.</param>
    /// <param name="tags">The tags of the sticker.</param>
    /// <param name="imageContents">The image content of the sticker.</param>
    /// <param name="format">The image format of the sticker.</param>
    /// <param name="reason">The reason this sticker is being created.</param>

    public async Task<DiscordMessageSticker> CreateGuildStickerAsync(ulong guildId, string name, string description, string tags, Stream imageContents, DiscordStickerFormat format, string reason = null)
    {
        string contentType, extension;

        if (format == DiscordStickerFormat.PNG || format == DiscordStickerFormat.APNG)
        {
            contentType = "image/png";
            extension = "png";
        }
        else
        {
            contentType = "application/json";
            extension = "json";
        }

        return await this.ApiClient.CreateGuildStickerAsync(guildId, name, description ?? string.Empty, tags, new DiscordMessageFile(null, imageContents, null, extension, contentType), reason);
    }

    /// <summary>
    /// Modifies a sticker in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="stickerId">The ID of the sticker.</param>
    /// <param name="action">Action to perform.</param>
    /// <param name="reason">Reason for audit log.</param>
    public async Task<DiscordMessageSticker> ModifyGuildStickerAsync(ulong guildId, ulong stickerId, Action<StickerEditModel> action, string reason = null)
    {
        StickerEditModel mdl = new();
        action(mdl);
        return await this.ApiClient.ModifyStickerAsync(guildId, stickerId, mdl.Name, mdl.Description, mdl.Tags, reason);
    }

    /// <summary>
    /// Deletes a sticker in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="stickerId">The ID of the sticker.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns></returns>
    public async Task DeleteGuildStickerAsync(ulong guildId, ulong stickerId, string reason = null)
        => await this.ApiClient.DeleteStickerAsync(guildId, stickerId, reason);

    #endregion

    #region Threads

    /// <summary>
    /// Creates a thread from a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="messageId">The ID of the message </param>
    /// <param name="name">The name of the thread.</param>
    /// <param name="archiveAfter">The auto archive duration.</param>
    /// <param name="reason">Reason for audit logs.</param>
    public async Task<DiscordThreadChannel> CreateThreadFromMessageAsync(ulong channelId, ulong messageId, string name, DiscordAutoArchiveDuration archiveAfter, string reason = null)
       => await this.ApiClient.CreateThreadFromMessageAsync(channelId, messageId, name, archiveAfter, reason);

    /// <summary>
    /// Creates a thread.
    /// </summary>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="name">The name of the thread.</param>
    /// <param name="archiveAfter">The auto archive duration.</param>
    /// <param name="threadType">The type of the thread.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    public async Task<DiscordThreadChannel> CreateThreadAsync(ulong channelId, string name, DiscordAutoArchiveDuration archiveAfter, DiscordChannelType threadType, string reason = null)
       => await this.ApiClient.CreateThreadAsync(channelId, name, archiveAfter, threadType, reason);

    /// <summary>
    /// Joins a thread.
    /// </summary>
    /// <param name="threadId">The ID of the thread.</param>
    public async Task JoinThreadAsync(ulong threadId)
        => await this.ApiClient.JoinThreadAsync(threadId);

    /// <summary>
    /// Leaves a thread.
    /// </summary>
    /// <param name="threadId">The ID of the thread.</param>
    public async Task LeaveThreadAsync(ulong threadId)
        => await this.ApiClient.LeaveThreadAsync(threadId);

    /// <summary>
    /// Adds a member to a thread.
    /// </summary>
    /// <param name="threadId">The ID of the thread.</param>
    /// <param name="userId">The ID of the member.</param>
    public async Task AddThreadMemberAsync(ulong threadId, ulong userId)
        => await this.ApiClient.AddThreadMemberAsync(threadId, userId);

    /// <summary>
    /// Removes a member from a thread.
    /// </summary>
    /// <param name="threadId">The ID of the thread.</param>
    /// <param name="userId">The ID of the member.</param>
    public async Task RemoveThreadMemberAsync(ulong threadId, ulong userId)
        => await this.ApiClient.RemoveThreadMemberAsync(threadId, userId);

    /// <summary>
    /// Lists the members of a thread.
    /// </summary>
    /// <param name="threadId">The ID of the thread.</param>
    public async Task<IReadOnlyList<DiscordThreadChannelMember>> ListThreadMembersAsync(ulong threadId)
        => await this.ApiClient.ListThreadMembersAsync(threadId);

    /// <summary>
    /// Lists the active threads of a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    public async Task<ThreadQueryResult> ListActiveThreadAsync(ulong guildId)
        => await this.ApiClient.ListActiveThreadsAsync(guildId);

    /// <summary>
    /// Gets the threads that are public and archived for a channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="before">Date to filter by.</param>
    /// <param name="limit">Limit.</param>
    public async Task<ThreadQueryResult> ListPublicArchivedThreadsAsync(ulong guildId, ulong channelId, DateTimeOffset? before = null, int limit = 0)
       => await this.ApiClient.ListPublicArchivedThreadsAsync(guildId, channelId, before?.ToString("o"), limit);

    /// <summary>
    /// Gets the threads that are public and archived for a channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="before">Date to filter by.</param>
    /// <param name="limit">Limit.</param>
    public async Task<ThreadQueryResult> ListPrivateArchivedThreadAsync(ulong guildId, ulong channelId, DateTimeOffset? before = null, int limit = 0)
       => await this.ApiClient.ListPrivateArchivedThreadsAsync(guildId, channelId, limit, before?.ToString("o"));

    /// <summary>
    /// Gets the private archived threads the user has joined for a channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="before">Date to filter by.</param>
    /// <param name="limit">Limit.</param>
    public async Task<ThreadQueryResult> ListJoinedPrivateArchivedThreadsAsync(ulong guildId, ulong channelId, DateTimeOffset? before = null, int limit = 0)
       => await this.ApiClient.ListJoinedPrivateArchivedThreadsAsync(guildId, channelId, limit, (ulong?)before?.ToUnixTimeSeconds());

    #endregion

    #region Emoji

    /// <summary>
    /// Gets a guild's emojis.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    public async Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong guildId)
        => await this.ApiClient.GetGuildEmojisAsync(guildId);

    /// <summary>
    /// Gets a guild emoji.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="emojiId">The ID of the emoji.</param>
    public async Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong guildId, ulong emojiId)
        => await this.ApiClient.GetGuildEmojiAsync(guildId, emojiId);

    /// <summary>
    /// Creates an emoji in a guild.
    /// </summary>
    /// <param name="name">Name of the emoji.</param>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="image">Image to use as the emoji.</param>
    /// <param name="roles">Roles for which the emoji will be available.</param>
    /// <param name="reason">Reason for audit logs.</param>
    public async Task<DiscordGuildEmoji> CreateEmojiAsync(ulong guildId, string name, Stream image, IEnumerable<ulong> roles = null, string reason = null)
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

        string image64;

        using ImageTool imgtool = new(image);
        image64 = imgtool.GetBase64();

        return await this.ApiClient.CreateGuildEmojiAsync(guildId, name, image64, roles, reason);
    }

    /// <summary>
    /// Modifies a guild's emoji.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="emojiId">The ID of the emoji.</param>
    /// <param name="name">New name of the emoji.</param>
    /// <param name="roles">Roles for which the emoji will be available.</param>
    /// <param name="reason">Reason for audit logs.</param>
    public async Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong guildId, ulong emojiId, string name, IEnumerable<ulong> roles = null, string reason = null)
        => await this.ApiClient.ModifyGuildEmojiAsync(guildId, emojiId, name, roles, reason);

    /// <summary>
    /// Deletes a guild's emoji.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="emojiId">The ID of the emoji.</param>
    /// <param name="reason">Reason for audit logs.</param>
    public async Task DeleteGuildEmojiAsync(ulong guildId, ulong emojiId, string reason = null)
        => await this.ApiClient.DeleteGuildEmojiAsync(guildId, emojiId, reason);

    #endregion

    #region Misc
    /// <summary>
    /// Gets assets from an application
    /// </summary>
    /// <param name="application">Application to get assets from</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication application)
        => await this.ApiClient.GetApplicationAssetsAsync(application);

    /// <summary>
    /// Gets a guild template by the code.
    /// </summary>
    /// <param name="code">The code of the template.</param>
    /// <returns>The guild template for the code.</returns>\
    public async Task<DiscordGuildTemplate> GetTemplateAsync(string code)
        => await this.ApiClient.GetTemplateAsync(code);
    #endregion

    private bool _disposed;
    /// <summary>
    /// Disposes of this DiscordRestClient
    /// </summary>
    public override void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        this._disposed = true;
        this._guilds = null;
        this.ApiClient?._rest?.Dispose();
    }
}
