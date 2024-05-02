// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2024 DSharpPlus Contributors
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

namespace DSharpPlus.Entities.AuditLogs;


// below is taken from
// https://discord.com/developers/docs/resources/audit-log#audit-log-entry-object-audit-log-events

/// <summary>
/// Represents type of the action that was taken in given audit log event.
/// </summary>
public enum DiscordAuditLogActionType
{
    /// <summary>
    /// Indicates that the guild was updated.
    /// </summary>
    GuildUpdate = 1,

    /// <summary>
    /// Indicates that the channel was created.
    /// </summary>
    ChannelCreate = 10,

    /// <summary>
    /// Indicates that the channel was updated.
    /// </summary>
    ChannelUpdate = 11,

    /// <summary>
    /// Indicates that the channel was deleted.
    /// </summary>
    ChannelDelete = 12,

    /// <summary>
    /// Indicates that the channel permission overwrite was created.
    /// </summary>
    OverwriteCreate = 13,

    /// <summary>
    /// Indicates that the channel permission overwrite was updated.
    /// </summary>
    OverwriteUpdate = 14,

    /// <summary>
    /// Indicates that the channel permission overwrite was deleted.
    /// </summary>
    OverwriteDelete = 15,

    /// <summary>
    /// Indicates that the user was kicked.
    /// </summary>
    Kick = 20,

    /// <summary>
    /// Indicates that users were pruned.
    /// </summary>
    Prune = 21,

    /// <summary>
    /// Indicates that the user was banned.
    /// </summary>
    Ban = 22,

    /// <summary>
    /// Indicates that the user was unbanned.
    /// </summary>
    Unban = 23,

    /// <summary>
    /// Indicates that the member was updated.
    /// </summary>
    MemberUpdate = 24,

    /// <summary>
    /// Indicates that the member's roles were updated.
    /// </summary>
    MemberRoleUpdate = 25,

    /// <summary>
    /// Indicates that the member has moved to another voice channel.
    /// </summary>
    MemberMove = 26,

    /// <summary>
    /// Indicates that the member has disconnected from a voice channel.
    /// </summary>
    MemberDisconnect = 27,

    /// <summary>
    /// Indicates that a bot was added to the guild.
    /// </summary>
    BotAdd = 28,

    /// <summary>
    /// Indicates that the role was created.
    /// </summary>
    RoleCreate = 30,

    /// <summary>
    /// Indicates that the role was updated.
    /// </summary>
    RoleUpdate = 31,

    /// <summary>
    /// Indicates that the role was deleted.
    /// </summary>
    RoleDelete = 32,

    /// <summary>
    /// Indicates that the invite was created.
    /// </summary>
    InviteCreate = 40,

    /// <summary>
    /// Indicates that the invite was updated.
    /// </summary>
    InviteUpdate = 41,

    /// <summary>
    /// Indicates that the invite was deleted.
    /// </summary>
    InviteDelete = 42,

    /// <summary>
    /// Indicates that the webhook was created.
    /// </summary>
    WebhookCreate = 50,

    /// <summary>
    /// Indicates that the webhook was updated.
    /// </summary>
    WebhookUpdate = 51,

    /// <summary>
    /// Indicates that the webhook was deleted.
    /// </summary>
    WebhookDelete = 52,

    /// <summary>
    /// Indicates that the emoji was created.
    /// </summary>
    EmojiCreate = 60,

    /// <summary>
    /// Indicates that the emoji was updated.
    /// </summary>
    EmojiUpdate = 61,

    /// <summary>
    /// Indicates that the emoji was deleted.
    /// </summary>
    EmojiDelete = 62,

    /// <summary>
    /// Indicates that the message was deleted.
    /// </summary>
    MessageDelete = 72,

    /// <summary>
    /// Indicates that messages were bulk-deleted.
    /// </summary>
    MessageBulkDelete = 73,

    /// <summary>
    /// Indicates that a message was pinned.
    /// </summary>
    MessagePin = 74,

    /// <summary>
    /// Indicates that a message was unpinned.
    /// </summary>
    MessageUnpin = 75,

    /// <summary>
    /// Indicates that an integration was created.
    /// </summary>
    IntegrationCreate = 80,

    /// <summary>
    /// Indicates that an integration was updated.
    /// </summary>
    IntegrationUpdate = 81,

    /// <summary>
    /// Indicates that an integration was deleted.
    /// </summary>
    IntegrationDelete = 82,

    /// <summary>
    /// Stage instance was created (stage channel becomes live)
    /// </summary>
    StageInstanceCreate = 83,

    /// <summary>
    /// Stage instance details were updated
    /// </summary>
    StageInstanceUpdate = 84,

    /// <summary>
    /// Stage instance was deleted (stage channel no longer live)
    /// </summary>
    StageInstanceDelete = 85,

    /// <summary>
    /// Indicates that an sticker was created.
    /// </summary>
    StickerCreate = 90,

    /// <summary>
    /// Indicates that an sticker was updated.
    /// </summary>
    StickerUpdate = 91,

    /// <summary>
    /// Indicates that an sticker was deleted.
    /// </summary>
    StickerDelete = 92,

    /// <summary>
    /// Indicates that a guild event was created.
    /// </summary>
    GuildScheduledEventCreate = 100,

    /// <summary>
    /// Indicates that a guild event was updated.
    /// </summary>
    GuildScheduledEventUpdate = 101,

    /// <summary>
    /// Indicates that a guild event was deleted.
    /// </summary>
    GuildScheduledEventDelete = 102,

    /// <summary>
    /// Indicates that a thread was created.
    /// </summary>
    ThreadCreate = 110,

    /// <summary>
    /// Indicates that a thread was updated.
    /// </summary>
    ThreadUpdate = 111,

    /// <summary>
    /// Indicates that a thread was deleted.
    /// </summary>
    ThreadDelete = 112,

    /// <summary>
    /// Permissions were updated for a command
    /// </summary>
    ApplicationCommandPermissionUpdate = 121,

    /// <summary>
    /// Auto Moderation rule was created
    /// </summary>
    AutoModerationRuleCreate = 140,

    /// <summary>
    /// Auto Moderation rule was updated
    /// </summary>
    AutoModerationRuleUpdate = 141,

    /// <summary>
    /// Auto Moderation rule was deleted
    /// </summary>
    AutoModerationRuleDelete = 142,

    /// <summary>
    /// Message was blocked by Auto Moderation
    /// </summary>
    AutoModerationBlockMessage = 143,

    /// <summary>
    /// Message was flagged by Auto Moderation
    /// </summary>
    AutoModerationFlagToChannel = 144,

    /// <summary>
    /// Member was timed out by Auto Moderation
    /// </summary>
    AutoModerationUserCommunicationDisabled = 145
}
