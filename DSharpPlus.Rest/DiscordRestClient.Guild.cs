// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;

namespace DSharpPlus;

public sealed partial class DiscordRestClient
{
    /// <summary>
    /// Gets a guild.
    /// </summary>
    /// <param name="guildId">The guild ID to search for.</param>
    /// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
    public Task<DiscordGuild> GetGuildAsync(ulong guildId, bool? withCounts = null)
        => ApiClient.GetGuildAsync(guildId, withCounts);

    /// <summary>
    /// Creates a new guild
    /// </summary>
    /// <param name="name">New guild's name</param>
    /// <param name="regionId">New guild's region ID</param>
    /// <param name="iconBase64">New guild's icon (base64)</param>
    /// <param name="verificationLevel">New guild's verification level</param>
    /// <param name="defaultMessageNotifications">New guild's default message notification level</param>
    /// <param name="systemChannelFlags">New guild's system channel flags</param>
    public Task<DiscordGuild> CreateGuildAsync(
        string name,
        string regionId,
        string iconBase64,
        VerificationLevel? verificationLevel,
        DefaultMessageNotifications? defaultMessageNotifications,
        SystemChannelFlags? systemChannelFlags
    ) => ApiClient.CreateGuildAsync(name, regionId, iconBase64, verificationLevel, defaultMessageNotifications, systemChannelFlags);

    /// <summary>
    /// Creates a guild from a template. This requires the bot to be in less than 10 guilds total.
    /// </summary>
    /// <param name="code">The template code.</param>
    /// <param name="name">Name of the guild.</param>
    /// <param name="icon">Stream containing the icon for the guild.</param>
    /// <returns>The created guild.</returns>
    public Task<DiscordGuild> CreateGuildFromTemplateAsync(string code, string name, string icon)
        => ApiClient.CreateGuildFromTemplateAsync(code, name, icon);

    /// <summary>
    /// Get a guild's voice region
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    public Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guildId)
        => ApiClient.GetGuildVoiceRegionsAsync(guildId);

    /// <summary>
    /// Deletes a guild
    /// </summary>
    /// <param name="id">Guild ID</param>
    public Task DeleteGuildAsync(ulong id)
        => ApiClient.DeleteGuildAsync(id);

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
    /// <param name="iconBase64">New guild icon (base64)</param>
    /// <param name="ownerId">New guild owner ID</param>
    /// <param name="splashBase64">New guild splash (base64)</param>
    /// <param name="systemChannelId">New guild system channel ID</param>
    /// <param name="banner">New guild banner</param>
    /// <param name="description">New guild description</param>
    /// <param name="discoverySplash">New guild Discovery splash</param>
    /// <param name="features">List of new <see href="https://discord.com/developers/docs/resources/guild#guild-object-guild-features">guild features</see></param>
    /// <param name="preferredLocale">New preferred locale</param>
    /// <param name="publicUpdatesChannelId">New updates channel ID</param>
    /// <param name="rulesChannelId">New rules channel ID</param>
    /// <param name="systemChannelFlags">New system channel flags</param>
    /// <param name="reason">Modify reason</param>
    public Task<DiscordGuild> ModifyGuildAsync
    (
        ulong guildId,
        Optional<string> name,
        Optional<string> region,
        Optional<VerificationLevel> verificationLevel,
        Optional<DefaultMessageNotifications> defaultMessageNotifications,
        Optional<MfaLevel> mfaLevel,
        Optional<ExplicitContentFilter> explicitContentFilter,
        Optional<ulong?> afkChannelId,
        Optional<int> afkTimeout,
        Optional<string> iconBase64,
        Optional<ulong> ownerId,
        Optional<string> splashBase64,
        Optional<ulong?> systemChannelId,
        Optional<string> banner,
        Optional<string> description,
        Optional<string> discoverySplash,
        Optional<IEnumerable<string>> features,
        Optional<string> preferredLocale,
        Optional<ulong?> publicUpdatesChannelId,
        Optional<ulong?> rulesChannelId,
        Optional<SystemChannelFlags> systemChannelFlags,
        string reason
    ) => ApiClient.ModifyGuildAsync(
        guildId,
        name,
        region,
        verificationLevel,
        defaultMessageNotifications,
        mfaLevel,
        explicitContentFilter,
        afkChannelId,
        afkTimeout,
        iconBase64,
        ownerId,
        splashBase64,
        systemChannelId,
        banner,
        description,
        discoverySplash,
        features,
        preferredLocale,
        publicUpdatesChannelId,
        rulesChannelId,
        systemChannelFlags,
        reason
    );

    /// <summary>
    /// Modifies a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="action">Guild modifications</param>
    /// <exception cref="ArgumentException">Thrown when the AFK channel is not a voice channel.</exception>
    /// <returns>The modified guild.</returns>
    public Task<DiscordGuild> ModifyGuildAsync(ulong guildId, Action<GuildEditModel> action)
    {
        GuildEditModel guildEditModel = new();
        action(guildEditModel);
        if (guildEditModel.AfkChannel.HasValue && guildEditModel.AfkChannel.Value.Type != ChannelType.Voice)
        {
            throw new ArgumentException("The AFK channel needs to be a voice channel!");
        }

        // Icon
        Optional<string?> iconBase64;
        if (guildEditModel.Icon.IsDefined(out Stream? icon))
        {
            using ImageTool imgtool = new(guildEditModel.Icon.Value);
            iconBase64 = Optional.FromValue(imgtool.GetBase64())!;
        }
        else
        {
            iconBase64 = guildEditModel.Icon.HasValue ? Optional.FromNoValue<string?>() : Optional.FromValue<string?>(null);
        }

        // Splash
        Optional<string?> splashBase64;
        if (guildEditModel.Splash.IsDefined(out Stream? splash))
        {
            using ImageTool imgtool = new(guildEditModel.Splash.Value);
            splashBase64 = Optional.FromValue(imgtool.GetBase64())!;
        }
        else
        {
            splashBase64 = guildEditModel.Splash.HasValue ? Optional.FromNoValue<string?>() : Optional.FromValue<string?>(null);
        }

        // Banner
        Optional<string?> bannerBase64;
        if (guildEditModel.Banner.IsDefined(out Stream? banner))
        {
            using ImageTool imgtool = new(guildEditModel.Banner.Value);
            bannerBase64 = Optional.FromValue(imgtool.GetBase64())!;
        }
        else
        {
            bannerBase64 = guildEditModel.Banner.HasValue ? Optional.FromNoValue<string?>() : Optional.FromValue<string?>(null);
        }

        // Modify
        // Can't help but notice that we send everything here instead of only the changed values.
        // TODO: (Micro-optimization) determine which values are actually changed and only send those.
        return ApiClient.ModifyGuildAsync(
            guildId,
            guildEditModel.Name,
            guildEditModel.Region.IfPresent(region => region.Id),
            guildEditModel.VerificationLevel,
            guildEditModel.DefaultMessageNotifications,
            guildEditModel.MfaLevel,
            guildEditModel.ExplicitContentFilter,
            guildEditModel.AfkChannel.IfPresent(afkChannel => afkChannel?.Id),
            guildEditModel.AfkTimeout,
            iconBase64,
            guildEditModel.Owner.IfPresent(owner => owner.Id),
            splashBase64,
            guildEditModel.SystemChannel.IfPresent(systemChannel => systemChannel?.Id),
            bannerBase64,
            guildEditModel.Description,
            guildEditModel.DiscoverySplash,
            guildEditModel.Features,
            guildEditModel.PreferredLocale,
            guildEditModel.PublicUpdatesChannel.IfPresent(publicUpdatesChannel => publicUpdatesChannel?.Id),
            guildEditModel.RulesChannel.IfPresent(rulesChannel => rulesChannel?.Id),
            guildEditModel.SystemChannelFlags,
            guildEditModel.AuditLogReason
        );
    }

    /// <summary>
    /// Gets guild bans.
    /// </summary>
    /// <param name="guildId">The ID of the guild to get the bans from.</param>
    /// <param name="limit">The number of users to return (up to maximum 1000, default 1000).</param>
    /// <param name="before">Consider only users before the given user ID.</param>
    /// <param name="after">Consider only users after the given user ID.</param>
    /// <returns>A collection of the guild's bans.</returns>
    public Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guildId, int? limit = null, ulong? before = null, ulong? after = null)
        => ApiClient.GetGuildBansAsync(guildId, limit, before, after);

    /// <summary>
    /// Gets the ban of the specified user. Requires Ban Members permission.
    /// </summary>
    /// <param name="guildId">The ID of the guild to get the ban from.</param>
    /// <param name="userId">The ID of the user to get the ban for.</param>
    /// <returns>A guild ban object.</returns>
    public Task<DiscordBan> GetGuildBanAsync(ulong guildId, ulong userId)
        => ApiClient.GetGuildBanAsync(guildId, userId);

    /// <summary>
    /// Creates guild ban
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="deleteMessageDays">Days to delete messages</param>
    /// <param name="reason">Reason why this member was banned</param>
    public Task CreateGuildBanAsync(ulong guildId, ulong userId, int deleteMessageDays, string reason)
        => ApiClient.CreateGuildBanAsync(guildId, userId, deleteMessageDays, reason);

    /// <summary>
    /// Removes a guild ban
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User to unban</param>
    /// <param name="reason">Reason why this member was unbanned</param>
    public Task RemoveGuildBanAsync(ulong guildId, ulong userId, string reason)
        => ApiClient.RemoveGuildBanAsync(guildId, userId, reason);

    /// <summary>
    /// Leaves a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    public Task LeaveGuildAsync(ulong guildId)
        => ApiClient.LeaveGuildAsync(guildId);

    /// <summary>
    /// Gets a guild's widget
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    public Task<DiscordWidget> GetGuildWidgetAsync(ulong guildId)
        => ApiClient.GetGuildWidgetAsync(guildId);

    /// <summary>
    /// Gets a guild's widget settings
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    public Task<DiscordWidgetSettings> GetGuildWidgetSettingsAsync(ulong guildId)
        => ApiClient.GetGuildWidgetSettingsAsync(guildId);

    /// <summary>
    /// Modifies a guild's widget settings
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="enabled">If the widget is enabled or not</param>
    /// <param name="channelId">Widget channel ID</param>
    /// <param name="reason">Reason the widget settings were modified</param>
    public Task<DiscordWidgetSettings> ModifyGuildWidgetSettingsAsync(ulong guildId, bool? enabled = null, ulong? channelId = null, string? reason = null)
        => ApiClient.ModifyGuildWidgetSettingsAsync(guildId, enabled, channelId, reason);

    /// <summary>
    /// Gets a guild's membership screening form.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns>The guild's membership screening form.</returns>
    public Task<DiscordGuildMembershipScreening> GetGuildMembershipScreeningFormAsync(ulong guildId)
        => ApiClient.GetGuildMembershipScreeningFormAsync(guildId);

    /// <summary>
    /// Modifies a guild's membership screening form.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="action">Action to perform</param>
    /// <returns>The modified screening form.</returns>
    public Task<DiscordGuildMembershipScreening> ModifyGuildMembershipScreeningFormAsync(ulong guildId, Action<MembershipScreeningEditModel> action)
    {
        MembershipScreeningEditModel membershipScreeningEditModel = new();
        action(membershipScreeningEditModel);
        return ApiClient.ModifyGuildMembershipScreeningFormAsync(
            guildId,
            membershipScreeningEditModel.Enabled,
            membershipScreeningEditModel.Fields,
            membershipScreeningEditModel.Description
        );
    }

    /// <summary>
    /// Gets a guild's vanity url
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <returns>The guild's vanity url.</returns>
    public Task<DiscordInvite> GetGuildVanityUrlAsync(ulong guildId)
        => ApiClient.GetGuildVanityUrlAsync(guildId);

    /// <summary>
    /// Gets a guild's templates.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns>All of the guild's templates.</returns>
    public Task<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync(ulong guildId)
        => ApiClient.GetGuildTemplatesAsync(guildId);

    /// <summary>
    /// Creates a guild template.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="name">Name of the template.</param>
    /// <param name="description">Description of the template.</param>
    /// <returns>The template created.</returns>
    public Task<DiscordGuildTemplate> CreateGuildTemplateAsync(ulong guildId, string name, string? description = null)
        => ApiClient.CreateGuildTemplateAsync(guildId, name, description);

    /// <summary>
    /// Syncs the template to the current guild's state.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="code">The code of the template to sync.</param>
    /// <returns>The template synced.</returns>
    public Task<DiscordGuildTemplate> SyncGuildTemplateAsync(ulong guildId, string code)
        => ApiClient.SyncGuildTemplateAsync(guildId, code);

    /// <summary>
    /// Modifies the template's metadata.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="code">The template's code.</param>
    /// <param name="name">Name of the template.</param>
    /// <param name="description">Description of the template.</param>
    /// <returns>The template modified.</returns>
    public Task<DiscordGuildTemplate> ModifyGuildTemplateAsync(ulong guildId, string code, string? name = null, string? description = null)
        => ApiClient.ModifyGuildTemplateAsync(guildId, code, name, description);

    /// <summary>
    /// Deletes the template.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="code">The code of the template to delete.</param>
    /// <returns>The deleted template.</returns>
    public Task<DiscordGuildTemplate> DeleteGuildTemplateAsync(ulong guildId, string code)
        => ApiClient.DeleteGuildTemplateAsync(guildId, code);

    /// <summary>
    /// Gets a guild's welcome screen.
    /// </summary>
    /// <returns>The guild's welcome screen object.</returns>
    public Task<DiscordGuildWelcomeScreen> GetGuildWelcomeScreenAsync(ulong guildId)
        => ApiClient.GetGuildWelcomeScreenAsync(guildId);

    /// <summary>
    /// Modifies a guild's welcome screen.
    /// </summary>
    /// <param name="guildId">The guild ID to modify.</param>
    /// <param name="action">Action to perform.</param>
    /// <param name="reason">The audit log reason for this action.</param>
    /// <returns>The modified welcome screen.</returns>
    public Task<DiscordGuildWelcomeScreen> ModifyGuildWelcomeScreenAsync(ulong guildId, Action<WelcomeScreenEditModel> action, string? reason = null)
    {
        WelcomeScreenEditModel welcomeScreenEditModel = new();
        action(welcomeScreenEditModel);
        return ApiClient.ModifyGuildWelcomeScreenAsync(
            guildId,
            welcomeScreenEditModel.Enabled,
            welcomeScreenEditModel.WelcomeChannels,
            welcomeScreenEditModel.Description,
            reason
        );
    }

    /// <summary>
    /// Gets a guild preview.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    public Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong guildId)
        => ApiClient.GetGuildPreviewAsync(guildId);

    /// <summary>
    /// Get a guild's invites
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    public Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guildId)
        => ApiClient.GetGuildInvitesAsync(guildId);

    /// <summary>
    /// Gets a guild template by the code.
    /// </summary>
    /// <param name="code">The code of the template.</param>
    /// <returns>The guild template for the code.</returns>\
    public Task<DiscordGuildTemplate> GetTemplateAsync(string code)
        => ApiClient.GetTemplateAsync(code);
}
