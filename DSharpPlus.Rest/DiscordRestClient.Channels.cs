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
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;

namespace DSharpPlus;

public sealed partial class DiscordRestClient
{
    /// <summary>
    /// Gets a channel object
    /// </summary>
    /// <param name="id">Channel ID</param>
    public Task<DiscordChannel> GetChannelAsync(ulong id)
        => ApiClient.GetChannelAsync(id);

    /// <summary>
    /// Gets channels from a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    public Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guildId)
        => ApiClient.GetGuildChannelsAsync(guildId);

    /// <summary>
    /// Deletes a channel
    /// </summary>
    /// <param name="id">Channel ID</param>
    /// <param name="reason">Reason why this channel was deleted</param>
    public Task DeleteChannelAsync(ulong id, string reason)
        => ApiClient.DeleteChannelAsync(id, reason);

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
    /// <exception cref="ArgumentException">The channel type must be a category, text, news, forum, voice or stage.</exception>
    /// <returns>The newly created channel.</returns>
    public Task<DiscordChannel> CreateGuildChannelAsync
    (
        ulong id,
        string name,
        ChannelType type,
        ulong? parent,
        Optional<string> topic,
        int? bitrate,
        int? userLimit,
        IEnumerable<DiscordOverwriteBuilder> overwrites,
        bool? nsfw,
        Optional<int?> perUserRateLimit,
        VideoQualityMode? qualityMode,
        int? position,
        string reason,
        AutoArchiveDuration? defaultAutoArchiveDuration = null,
        DefaultReaction? defaultReactionEmoji = null,
        IEnumerable<DiscordForumTagBuilder>? availableTags = null,
        DefaultSortOrder? defaultSortOrder = null
    ) => type switch
    {
        ChannelType.Category
        or ChannelType.Text
        or ChannelType.News
        or ChannelType.GuildForum
        or ChannelType.Voice
        or ChannelType.Stage => ApiClient.CreateGuildChannelAsync(
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
        ),
        _ => throw new ArgumentException("The channel type must be a category, text, news, forum, voice or stage.", nameof(type)),
    };

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
    public Task ModifyChannelAsync
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
        VideoQualityMode? qualityMode,
        Optional<ChannelType> type,
        IEnumerable<DiscordOverwriteBuilder> permissionOverwrites,
        string reason,
        Optional<ChannelFlags> flags,
        IEnumerable<DiscordForumTagBuilder>? availableTags,
        Optional<AutoArchiveDuration?> defaultAutoArchiveDuration,
        Optional<DefaultReaction?> defaultReactionEmoji,
        Optional<int> defaultPerUserRatelimit,
        Optional<DefaultSortOrder?> defaultSortOrder,
        Optional<DefaultForumLayout> defaultForumLayout
    ) => ApiClient.ModifyChannelAsync
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
        rtcRegion.IfPresent(rtcRegion => rtcRegion?.Id),
        qualityMode,
        type,
        permissionOverwrites,
        reason,
        flags,
        availableTags,
        defaultAutoArchiveDuration,
        defaultReactionEmoji,
        defaultPerUserRatelimit,
        defaultSortOrder,
        defaultForumLayout
    );

    /// <summary>
    /// Modifies a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="action">Channel modifications</param>
    public Task ModifyChannelAsync(ulong channelId, Action<ChannelEditModel> action)
    {
        ChannelEditModel channelEditModel = new();
        action(channelEditModel);
        return ApiClient.ModifyChannelAsync(
            channelId,
            channelEditModel.Name,
            channelEditModel.Position,
            channelEditModel.Topic,
            channelEditModel.Nsfw,
            channelEditModel.Parent.IfPresent(parentChannel => parentChannel?.Id),
            channelEditModel.Bitrate,
            channelEditModel.Userlimit,
            channelEditModel.PerUserRateLimit,
            channelEditModel.RtcRegion.IfPresent(rtcRegion => rtcRegion?.Id),
            channelEditModel.QualityMode,
            channelEditModel.Type,
            channelEditModel.PermissionOverwrites,
            channelEditModel.AuditLogReason,
            channelEditModel.Flags,
            channelEditModel.AvailableTags,
            channelEditModel.DefaultAutoArchiveDuration,
            channelEditModel.DefaultReaction,
            channelEditModel.DefaultThreadRateLimit,
            channelEditModel.DefaultSortOrder,
            channelEditModel.DefaultForumLayout
        );
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
    public Task UpdateChannelPositionAsync(ulong guildId, ulong channelId, int position, string reason, bool? lockPermissions = null, ulong? parentId = null)
    {
        List<RestGuildChannelReorderPayload> channelReorderPayload = new()
        {
            new RestGuildChannelReorderPayload
            {
                ChannelId = channelId,
                Position = position,
                LockPermissions = lockPermissions,
                ParentId = parentId
            }
        };

        return ApiClient.ModifyGuildChannelPositionAsync(guildId, channelReorderPayload, reason);
    }

    /// <summary>
    /// Gets a channel's invites
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    public Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channelId)
        => ApiClient.GetChannelInvitesAsync(channelId);

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
    public Task<DiscordInvite> CreateChannelInviteAsync
    (
        ulong channelId,
        int maxAge,
        int maxUses,
        bool temporary,
        bool unique,
        string reason,
        InviteTargetType? targetType = null,
        ulong? targetUserId = null,
        ulong? targetApplicationId = null
    ) => ApiClient.CreateChannelInviteAsync
    (
        channelId,
        maxAge,
        maxUses,
        temporary,
        unique,
        reason,
        targetType,
        targetUserId,
        targetApplicationId
    );

    /// <summary>
    /// Deletes channel overwrite
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="overwriteId">Overwrite ID</param>
    /// <param name="reason">Reason it was deleted</param>
    public Task DeleteChannelPermissionAsync(ulong channelId, ulong overwriteId, string reason)
        => ApiClient.DeleteChannelPermissionAsync(channelId, overwriteId, reason);

    /// <summary>
    /// Edits channel overwrite
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="overwriteId">Overwrite ID</param>
    /// <param name="allow">Permissions to allow</param>
    /// <param name="deny">Permissions to deny</param>
    /// <param name="type">Overwrite type</param>
    /// <param name="reason">Reason this overwrite was created</param>
    public Task EditChannelPermissionsAsync(ulong channelId, ulong overwriteId, Permissions allow, Permissions deny, string type, string reason)
        => ApiClient.EditChannelPermissionsAsync(channelId, overwriteId, allow, deny, type, reason);

    /// <summary>
    /// Creates a stage instance in a stage channel.
    /// </summary>
    /// <param name="channelId">The ID of the stage channel to create it in.</param>
    /// <param name="topic">The topic of the stage instance.</param>
    /// <param name="privacyLevel">The privacy level of the stage instance.</param>
    /// <param name="reason">The reason the stage instance was created.</param>
    /// <returns>The created stage instance.</returns>
    public Task<DiscordStageInstance> CreateStageInstanceAsync(ulong channelId, string topic, PrivacyLevel? privacyLevel = null, string? reason = null)
        => ApiClient.CreateStageInstanceAsync(channelId, topic, privacyLevel, reason);

    /// <summary>
    /// Gets a stage instance in a stage channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel.</param>
    /// <returns>The stage instance in the channel.</returns>
    public Task<DiscordStageInstance> GetStageInstanceAsync(ulong channelId)
        => ApiClient.GetStageInstanceAsync(channelId);

    /// <summary>
    /// Modifies a stage instance in a stage channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to modify the stage instance of.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The modified stage instance.</returns>
    public Task<DiscordStageInstance> ModifyStageInstanceAsync(ulong channelId, Action<StageInstanceEditModel> action)
    {
        StageInstanceEditModel stageInstanceEditModel = new();
        action(stageInstanceEditModel);
        return ApiClient.ModifyStageInstanceAsync(
            channelId,
            stageInstanceEditModel.Topic,
            stageInstanceEditModel.PrivacyLevel,
            stageInstanceEditModel.AuditLogReason
        );
    }

    /// <summary>
    /// Deletes a stage instance in a stage channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to delete the stage instance of.</param>
    /// <param name="reason">The reason the stage instance was deleted.</param>
    public Task DeleteStageInstanceAsync(ulong channelId, string? reason = null)
        => ApiClient.DeleteStageInstanceAsync(channelId, reason);

    /// <summary>
    /// Gets an invite.
    /// </summary>
    /// <param name="inviteCode">The invite code.</param>
    /// <param name="withCounts">Whether to include presence and total member counts in the returned invite.</param>
    /// <param name="withExpiration">Whether to include the expiration date in the returned invite.</param>
    public Task<DiscordInvite> GetInviteAsync(string inviteCode, bool? withCounts = null, bool? withExpiration = null)
        => ApiClient.GetInviteAsync(inviteCode, withCounts, withExpiration);

    /// <summary>
    /// Removes an invite
    /// </summary>
    /// <param name="inviteCode">Invite code</param>
    /// <param name="reason">Reason why this invite was removed</param>
    public Task<DiscordInvite> DeleteInvite(string inviteCode, string reason)
        => ApiClient.DeleteInviteAsync(inviteCode, reason);

    /// <summary>
    /// Creates a new webhook
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="name">Webhook name</param>
    /// <param name="avatarBase64">Webhook avatar (base64)</param>
    /// <param name="reason">Reason why this webhook was created</param>
    public Task<DiscordWebhook> CreateWebhookAsync(ulong channelId, string name, string avatarBase64, string reason)
        => ApiClient.CreateWebhookAsync(channelId, name, avatarBase64, reason);

    /// <summary>
    /// Creates a new webhook
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="name">Webhook name</param>
    /// <param name="avatar">Webhook avatar</param>
    /// <param name="reason">Reason why this webhook was created</param>
    public Task<DiscordWebhook> CreateWebhookAsync(ulong channelId, string name, Stream? avatar = null, string? reason = null)
    {
        string? avatarBase64;
        if (avatar != null)
        {
            using ImageTool imgtool = new(avatar);
            avatarBase64 = imgtool.GetBase64();
        }
        else
        {
            avatarBase64 = null;
        }

        return ApiClient.CreateWebhookAsync(channelId, name, avatarBase64, reason);
    }

    /// <summary>
    /// Gets all webhooks from a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    public Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channelId)
        => ApiClient.GetChannelWebhooksAsync(channelId);

    /// <summary>
    /// Gets all webhooks from a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    public Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guildId)
        => ApiClient.GetGuildWebhooksAsync(guildId);

    /// <summary>
    /// Gets a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    public Task<DiscordWebhook> GetWebhookAsync(ulong webhookId)
        => ApiClient.GetWebhookAsync(webhookId);

    /// <summary>
    /// Gets a webhook with its token (when user is not in said guild)
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="webhookToken">Webhook token</param>
    public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhookId, string webhookToken)
        => ApiClient.GetWebhookWithTokenAsync(webhookId, webhookToken);

    /// <summary>
    /// Modifies a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="channelId">The new channel ID the webhook should be moved to.</param>
    /// <param name="name">New webhook name</param>
    /// <param name="avatarBase64">New webhook avatar (base64)</param>
    /// <param name="reason">Reason why this webhook was modified</param>
    public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, ulong channelId, string name, string avatarBase64, string reason)
        => ApiClient.ModifyWebhookAsync(webhookId, channelId, name, avatarBase64, reason);

    /// <summary>
    /// Modifies a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="channelId">The new channel ID the webhook should be moved to.</param>
    /// <param name="name">New webhook name</param>
    /// <param name="avatar">New webhook avatar</param>
    /// <param name="reason">Reason why this webhook was modified</param>
    public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, ulong channelId, string name, Stream avatar, string reason)
    {
        string? avatarBase64;
        if (avatar != null)
        {
            using ImageTool imgtool = new(avatar);
            avatarBase64 = imgtool.GetBase64();
        }
        else
        {
            avatarBase64 = null;
        }

        return ApiClient.ModifyWebhookAsync(webhookId, channelId, name, avatarBase64, reason);
    }

    /// <summary>
    /// Modifies a webhook (when user is not in said guild)
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="name">New webhook name</param>
    /// <param name="avatarBase64">New webhook avatar (base64)</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <param name="reason">Reason why this webhook was modified</param>
    public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, string avatarBase64, string webhookToken, string reason)
        => ApiClient.ModifyWebhookAsync(webhookId, name, avatarBase64, webhookToken, reason);

    /// <summary>
    /// Modifies a webhook (when user is not in said guild)
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="name">New webhook name</param>
    /// <param name="avatar">New webhook avatar</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <param name="reason">Reason why this webhook was modified</param>
    public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, Stream avatar, string webhookToken, string reason)
    {
        string? avatarBase64;
        if (avatar != null)
        {
            using ImageTool imgtool = new(avatar);
            avatarBase64 = imgtool.GetBase64();
        }
        else
        {
            avatarBase64 = null;
        }

        return ApiClient.ModifyWebhookAsync(webhookId, name, avatarBase64, webhookToken, reason);
    }

    /// <summary>
    /// Deletes a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="reason">Reason this webhook was deleted</param>
    public Task DeleteWebhookAsync(ulong webhookId, string reason)
        => ApiClient.DeleteWebhookAsync(webhookId, reason);

    /// <summary>
    /// Deletes a webhook (when user is not in said guild)
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="reason">Reason this webhook was removed</param>
    /// <param name="webhookToken">Webhook token</param>
    public Task DeleteWebhookAsync(ulong webhookId, string reason, string webhookToken)
        => ApiClient.DeleteWebhookAsync(webhookId, webhookToken, reason);

    /// <summary>
    /// Sends a message to a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <param name="builder">Webhook builder filled with data to send.</param>
    public Task<DiscordMessage> ExecuteWebhookAsync(ulong webhookId, string webhookToken, DiscordWebhookBuilder builder)
        => ApiClient.ExecuteWebhookAsync(webhookId, webhookToken, builder);

    /// <summary>
    /// Edits a previously-sent webhook message.
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <param name="messageId">The ID of the message to edit.</param>
    /// <param name="builder">The builder of the message to edit.</param>
    /// <param name="attachments">Attached files to keep.</param>
    /// <returns>The modified <see cref="DiscordMessage"/></returns>
    public Task<DiscordMessage> EditWebhookMessageAsync
    (
        ulong webhookId,
        string webhookToken,
        ulong messageId,
        DiscordWebhookBuilder builder,
        IEnumerable<DiscordAttachment>? attachments = default
    )
    {
        builder.Validate(true);
        return ApiClient.EditWebhookMessageAsync(webhookId, webhookToken, messageId, builder, attachments);
    }

    /// <summary>
    /// Deletes a message that was created by a webhook.
    /// </summary>
    /// <param name="webhookId">Webhook ID</param>
    /// <param name="webhookToken">Webhook token</param>
    /// <param name="messageId">The ID of the message to delete</param>
    public Task DeleteWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId)
        => ApiClient.DeleteWebhookMessageAsync(webhookId, webhookToken, messageId);

    /// <summary>
    /// Follows a news channel
    /// </summary>
    /// <param name="channelId">ID of the channel to follow</param>
    /// <param name="webhookChannelId">ID of the channel to crosspost messages to</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> on the target channel</exception>
    public Task<DiscordFollowedChannel> FollowChannelAsync(ulong channelId, ulong webhookChannelId)
        => ApiClient.FollowChannelAsync(channelId, webhookChannelId);
}
