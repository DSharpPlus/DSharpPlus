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

namespace DSharpPlus.Core.Enums
{
    /// <summary>
    /// Permissions in Discord are a way to limit and grant certain abilities to users. A set of base permissions can be configured at the guild level for different roles. When these roles are attached to users, they grant or revoke specific privileges within the guild. Along with the guild-level permissions, Discord also supports permission overwrites that can be assigned to individual guild roles or guild members on a per-channel basis.
    /// </summary>
    [Flags]
    public enum DiscordPermissions : long
    {
        /// <summary>
        /// No permissions.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allows creation of instant invites. Used on text, voice and stage channels.
        /// </summary>
        CreateInstantInvite = 0x0000000000000001,

        /// <summary>
        /// Allows kicking members.
        /// </summary>
        KickMembers = 0x0000000000000002,

        /// <summary>
        /// Allows banning members.
        /// </summary>
        BanMembers = 0x0000000000000004,

        /// <summary>
        /// Allows all permissions and bypasses channel permission overwrites.
        /// </summary>
        Administrator = 0x0000000000000008,

        /// <summary>
        /// Allows management and editing of channels. Used on text, voice and stage channels.
        /// </summary>
        ManageChannels = 0x0000000000000010,

        /// <summary>
        /// Allows management and editing of the guild.
        /// </summary>
        ManageGuild = 0x0000000000000020,

        /// <summary>
        /// Allows for the addition of reactions to messages. Used on text channels.
        /// </summary>
        AddReactions = 0x0000000000000040,

        /// <summary>
        /// Allows for viewing of audit logs.
        /// </summary>
        ViewAuditLog = 0x0000000000000080,

        /// <summary>
        /// Allows for using priority speaker in a voice channel.
        /// </summary>
        PrioritySpeaker = 0x0000000000000100,

        /// <summary>
        /// Allows the user to go live. Used on voice channels.
        /// </summary>
        Stream = 0x0000000000000200,

        /// <summary>
        /// Allows guild members to view a channel, which includes reading messages in text channels and joining voice channels. Used on text, voice and stage channels.
        /// </summary>
        ViewChannel = 0x0000000000000400,

        /// <summary>
        /// Allows for sending messages in a channel (does not allow sending messages in threads). Used on text channels.
        /// </summary>
        SendMessages = 0x0000000000000800,

        /// <summary>
        /// Allows for sending of <c>/tts</c> messages
        /// </summary>
        SendTTSMessages = 0x0000000000001000,

        /// <summary>
        /// Allows for deletion of other users messages. Used on text channels.
        /// </summary>
        ManageMessages = 0x0000000000002000,

        /// <summary>
        /// Links sent by users with this permission will be auto-embedded. Used on text channels.
        /// </summary>
        EmbedLinks = 0x0000000000004000,

        /// <summary>
        /// Allows for uploading images and files. Used on text channels.
        /// </summary>
        AttachFiles = 0x0000000000008000,

        /// <summary>
        /// Allows for reading of message history. Used on text channels.
        /// </summary>
        ReadMessageHistory = 0x0000000000010000,

        /// <summary>
        /// Allows for using the <c>@everyone</c> tag to notify all users in a channel, and the <c>@here</c> tag to notify all online users in a channel. Used on text channels.
        /// </summary>
        MentionEveryone = 0x0000000000020000,

        /// <summary>
        /// Allows the usage of custom emojis from other servers. Used on text channels.
        /// </summary>
        UseExternalEmojis = 0x0000000000040000,

        /// <summary>
        /// Allows for viewing guild insights.
        /// </summary>
        ViewGuildInsights = 0x0000000000080000,

        /// <summary>
        /// Allows for joining of a voice channel. Used on voice and stage channels.
        /// </summary>
        Connect = 0x0000000000100000,

        /// <summary>
        /// Allows for speaking in a voice channel. Used on voice channels.
        /// </summary>
        Speak = 0x0000000000200000,

        /// <summary>
        /// Allows for muting members in a voice channel. Used on voice and stage channels.
        /// </summary>
        MuteMembers = 0x0000000000400000,

        /// <summary>
        /// Allows for deafening of members in a voice channel. Used on voice and stage channels.
        /// </summary>
        DeafenMembers = 0x0000000000800000,

        /// <summary>
        /// Allows for moving of members between voice channels. Used on voice and stage channels.
        /// </summary>
        MoveMembers = 0x0000000001000000,

        /// <summary>
        /// Allows for using voice-activity-detection in a voice channel. Used on voice channels.
        /// </summary>
        UseVAD = 0x0000000002000000,

        /// <summary>
        /// Allows for modification of own nickname.
        /// </summary>
        ChangeNickname = 0x0000000004000000,

        /// <summary>
        /// Allows for modification of other users nicknames.
        /// </summary>
        ManageNicknames = 0x0000000008000000,

        /// <summary>
        /// Allows management and editing of roles. Used on text, voice and stage channels.
        /// </summary>
        ManageRoles = 0x0000000010000000,

        /// <summary>
        /// Allows management and editing of webhooks. Used on text channels.
        /// </summary>
        ManageWebhooks = 0x0000000020000000,

        /// <summary>
        /// Allows management and editing of emojis and stickers.
        /// </summary>
        ManageEmojisAndStickers = 0x0000000040000000,

        /// <summary>
        /// Allows members to use application commands, including slash commands and context menu commands. Used on text channels.
        /// </summary>
        UseApplicationCommands = 0x0000000080000000,

        /// <summary>
        /// Allows for requesting to speak in stage channels. <i>(This permission is under active development and may be changed or removed.)</i> Used on stage channels.
        /// </summary>
        RequestToSpeak = 0x0000000100000000,

        /// <summary>
        /// Allows for creating, editing, and deleting scheduled events. Used on voice and stage channels.
        /// </summary>
        ManageEvents = 0x0000000200000000,

        /// <summary>
        /// Allows for deleting and archiving threads, and viewing all private threads. Used on text channels.
        /// </summary>
        ManageThreads = 0x0000000400000000,

        /// <summary>
        /// Allows for creating public and announcement threads. Used on text channels.
        /// </summary>
        CreatePublicThreads = 0x0000000800000000,

        /// <summary>
        /// Allows for creating private threads. Used on text channels.
        /// </summary>
        CreatePrivateThreads = 0x0000001000000000,

        /// <summary>
        /// Allows the usage of custom stickers from other servers. Used on text channels.
        /// </summary>
        UseExternalStickers = 0x0000002000000000,

        /// <summary>
        /// Allows for sending messages in threads. Used on text channels.
        /// </summary>
        SendMessagesInThreads = 0x0000004000000000,

        /// <summary>
        /// Allows for launching activities (applications with the <c>EMBEDDED</c> flag) in a voice channel. Used on voice channels.
        /// </summary>
        StartEmbeddedActivities = 0x0000008000000000,

        /// <summary>
        /// Allows for timing out users to prevent them from sending or reacting to messages in chat and threads, and from speaking in voice and stage channels.
        /// </summary>
        ModerateMembers = 0x0000010000000000
    }
}
