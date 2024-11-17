// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates the visible types of audit log events; and lists the metadata provided to each event.
/// </summary>
public enum DiscordAuditLogEvent
{
    /// <summary>
    /// The server settings were updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a guild object.
    /// </remarks>
    GuildUpdated = 1,

    /// <summary>
    /// A channel was created.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a channel object.
    /// </remarks>
    ChannelCreated = 10,

    /// <summary>
    /// A channel was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a channel object.
    /// </remarks>
    ChannelUpdated = 11,

    /// <summary>
    /// A channel was deleted.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a channel object.
    /// </remarks>
    ChannelDeleted = 12,

    /// <summary>
    /// A permission overwrite was added to a channel.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a channel overwrite object.
    /// </remarks>
    ChannelOverwriteCreated = 13,

    /// <summary>
    /// A permission overwrite in a channel was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a channel overwrite object.
    /// </remarks>
    ChannelOverwriteUpdated = 14,

    /// <summary>
    /// A permission overwrite in a channel was deleted.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a channel overwrite object.
    /// </remarks>
    ChannelOverwriteDeleted = 15,

    /// <summary>
    /// A member was kicked from the server.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    MemberKicked = 20,

    /// <summary>
    /// Members were pruned from the server.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    MemberPruned = 21,

    /// <summary>
    /// A member was banned from the server.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    MemberBanned = 22,

    /// <summary>
    /// A member was unbanned from the server.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    MemberUnbanned = 23,

    /// <summary>
    /// A server member was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a member object.
    /// </remarks>
    MemberUpdated = 24,

    /// <summary>
    /// A role was granted to or removed from a server member.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a role object.
    /// </remarks>
    MemberRoleUpdated = 25,

    /// <summary>
    /// A server member was moved to a different voice channel.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    MemberMoved = 26,

    /// <summary>
    /// A server member was disconnected from a voice channel.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    MemberDisconnected = 27,

    /// <summary>
    /// A bot user was added to the server.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    BotAdded = 28,

    /// <summary>
    /// A role was created.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a role object.
    /// </remarks>
    RoleCreated = 30,

    /// <summary>
    /// A role was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a role object.
    /// </remarks>
    RoleUpdated = 31,

    /// <summary>
    /// A role was deleted.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a role object.
    /// </remarks>
    RoleDeleted = 32,

    /// <summary>
    /// An invite was created.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an invite object.
    /// </remarks>
    InviteCreated = 40,

    /// <summary>
    /// An invite was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an invite object.
    /// </remarks>
    InviteUpdated = 41,

    /// <summary>
    /// An invite was deleted.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an invite object.
    /// </remarks>
    InviteDeleted = 42,

    /// <summary>
    /// A webhook was created.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a webhook object.
    /// </remarks>
    WebhookCreated = 50,

    /// <summary>
    /// A webhook was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a webhook object.
    /// </remarks>
    WebhookUpdated = 51,

    /// <summary>
    /// A webhook was deleted.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a webhook object.
    /// </remarks>
    WebhookDeleted = 52,

    /// <summary>
    /// An emoji was created.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an emoji object.
    /// </remarks>
    EmojiCreated = 60,

    /// <summary>
    /// An emoji was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an emoji object.
    /// </remarks>
    EmojiUpdated = 61,

    /// <summary>
    /// An emoji was deleted.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an emoji object.
    /// </remarks>
    EmojiDeleted = 62,

    /// <summary>
    /// A message was deleted.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    MessageDeleted = 72,

    /// <summary>
    /// Multiple messages were bulk-deleted.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    MessageBulkDeleted = 73,

    /// <summary>
    /// A message was pinned to a channel.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    MessagePinned = 74,

    /// <summary>
    /// A message was unpinned from a channel.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    MessageUnpinned = 75,

    /// <summary>
    /// An integration was added to a server.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an integration object.
    /// </remarks>
    IntegrationCreated = 80,

    /// <summary>
    /// An integration within a server was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an integration object.
    /// </remarks>
    IntegrationUpdated = 81,

    /// <summary>
    /// An integration was deleted from a server.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an integration object.
    /// </remarks>
    IntegrationDeleted = 82,

    /// <summary>
    /// A stage channel went live.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a stage instance object.
    /// </remarks>
    StageInstanceCreated = 83,

    /// <summary>
    /// A live stage channel was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a stage instance object.
    /// </remarks>
    StageInstanceUpdated = 84,

    /// <summary>
    /// A stage instance ended.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a stage instance object.
    /// </remarks>
    StageInstanceDeleted = 85,

    /// <summary>
    /// A sticker was created.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a sticker object.
    /// </remarks>
    StickerCreated = 90,

    /// <summary>
    /// A sticker was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a sticker object.
    /// </remarks>
    StickerUpdated = 91,

    /// <summary>
    /// A sticker was deleted.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a sticker object.
    /// </remarks>
    StickerDeleted = 92,

    /// <summary>
    /// A scheduled event was created.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a scheduled event object.
    /// </remarks>
    ScheduledEventCreated = 100,

    /// <summary>
    /// A scheduled event was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a scheduled event object.
    /// </remarks>
    ScheduledEventUpdated = 101,

    /// <summary>
    /// A scheduled event was cancelled.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a scheduled event object.
    /// </remarks>
    ScheduledEventDeleted = 102,

    /// <summary>
    /// A thread was created in a channel.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a channel object.
    /// </remarks>
    ThreadCreated = 110,

    /// <summary>
    /// A thread was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a channel object.
    /// </remarks>
    ThreadUpdated = 111,

    /// <summary>
    /// A thread was deleted.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a channel object.
    /// </remarks>
    ThreadDeleted = 112,

    /// <summary>
    /// An application command's permissions were updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an application command permissions object.
    /// </remarks>
    ApplicationCommandPermissionsUpdated = 121,

    /// <summary>
    /// A soundboard sound was created.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a soundboard sound object.
    /// </remarks>
    SoundboardSoundCreated = 130,

    /// <summary>
    /// A soundboard sound was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a soundboard sound object.
    /// </remarks>
    SoundboardSoundUpdated = 131,

    /// <summary>
    /// A soundboard sound was deleted.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for a soundboard sound object.
    /// </remarks>
    SoundboardSoundDeleted = 132,

    /// <summary>
    /// An auto moderation rule was created.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an auto moderation rule object.
    /// </remarks>
    AutoModerationRuleCreated = 140,

    /// <summary>
    /// An auto moderation rule was updated.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an auto moderation rule object.
    /// </remarks>
    AutoModerationRuleUpdated = 141,

    /// <summary>
    /// An auto moderation rule was deleted.
    /// </summary>
    /// <remarks>
    /// Metadata is provided for an auto moderation rule object.
    /// </remarks>
    AutoModerationRuleDeleted = 142,

    /// <summary>
    /// A message was blocked by the discord automod.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    AutoModerationMessageBlocked = 143,

    /// <summary>
    /// A message was flagged and alerted to by the discord automod.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    AutoModerationFlaggedToChannel = 144,

    /// <summary>
    /// A member was timed out by the discord automod.
    /// </summary>
    /// <remarks>
    /// No metadata is provided.
    /// </remarks>
    AutoModerationUserCommunicationDisabled = 145
}
