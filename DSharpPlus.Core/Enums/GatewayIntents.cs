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

namespace DSharpPlus.Core.Enums
{
    public enum GatewayIntents
    {
        /// <summary>
        /// Contains the following events:
        ///   - GUILD_CREATE
        ///   - GUILD_UPDATE
        ///   - GUILD_DELETE
        ///   - GUILD_ROLE_CREATE
        ///   - GUILD_ROLE_UPDATE
        ///   - GUILD_ROLE_DELETE
        ///   - CHANNEL_CREATE
        ///   - CHANNEL_UPDATE
        ///   - CHANNEL_DELETE
        ///   - CHANNEL_PINS_UPDATE
        ///   - THREAD_CREATE
        ///   - THREAD_UPDATE
        ///   - THREAD_DELETE
        ///   - THREAD_LIST_SYNC
        ///   - THREAD_MEMBER_UPDATE
        ///   - THREAD_MEMBERS_UPDATE *
        ///   - STAGE_INSTANCE_CREATE
        ///   - STAGE_INSTANCE_UPDATE
        ///   - STAGE_INSTANCE_DELETE
        /// </summary>
        Guilds = 1 << 0,

        /// <summary>
        /// Contains the following events:
        ///   - GUILD_MEMBER_ADD
        ///   - GUILD_MEMBER_UPDATE
        ///   - GUILD_MEMBER_REMOVE
        ///   - THREAD_MEMBERS_UPDATE
        /// GUILD_MEMBER_UPDATE is sent for current-user updates regardless of whether the <see cref="GuildMembers"/> intent is set.
        /// Thread Members Update by default only includes if the current user was added to or removed from a thread. To receive these updates for other users, request the <see cref="GuildMembers"/> Gateway Intent.
        /// </summary>
        GuildMembers = 1 << 1,

        /// <summary>
        /// Contains the following events:
        ///   - GUILD_BAN_ADD
        ///   - GUILD_BAN_REMOVE
        /// </summary>
        GuildBans = 1 << 2,

        /// <summary>
        /// Contains the following events:
        ///   - GUILD_EMOJIS_UPDATE
        ///   - GUILD_STICKERS_UPDATE
        /// </summary>
        GuildEmojisAndStickers = 1 << 3,

        /// <summary>
        /// Contains the following events:
        ///   - GUILD_INTEGRATIONS_UPDATE
        ///   - INTEGRATION_CREATE
        ///   - INTEGRATION_UPDATE
        ///   - INTEGRATION_DELETE
        /// </summary>
        GuildIntegrations = 1 << 4,

        /// <summary>
        /// Contains the following events:
        ///   - WEBHOOKS_UPDATE
        /// </summary>
        GuildWebhooks = 1 << 5,

        /// <summary>
        /// Contains the following events:
        ///   - INVITE_CREATE
        ///   - INVITE_DELETE
        /// </summary>
        GuildInvites = 1 << 6,

        /// <summary>
        /// Contains the following events:
        ///    - VOICE_STATE_UPDATE
        /// </summary>
        GuildVoiceStates = 1 << 7,

        /// <summary>
        /// Contains the following events:
        ///    - PRESENCE_UPDATE
        /// </summary>
        GuildPresences = 1 << 8,

        /// <summary>
        /// Contains the following events:
        ///   - MESSAGE_CREATE
        ///   - MESSAGE_UPDATE
        ///   - MESSAGE_DELETE
        ///   - MESSAGE_DELETE_BULK
        /// </summary>
        GuildMessages = 1 << 9,

        /// <summary>
        /// Contains the following events:
        ///   - MESSAGE_REACTION_ADD
        ///   - MESSAGE_REACTION_REMOVE
        ///   - MESSAGE_REACTION_REMOVE_ALL
        ///   - MESSAGE_REACTION_REMOVE_EMOJI
        /// </summary>
        GuildMessageReactions = 1 << 10,

        /// <summary>
        /// Contains the following events:
        ///   - TYPING_START
        /// </summary>
        GuildMessageTyping = 1 << 11,

        /// <summary>
        /// Contains the following events:
        ///   - MESSAGE_CREATE
        ///   - MESSAGE_UPDATE
        ///   - MESSAGE_DELETE
        ///   - CHANNEL_PINS_UPDATE
        /// </summary>
        DirectMessages = 1 << 12,

        /// <summary>
        /// Contains the following events:
        ///   - MESSAGE_REACTION_ADD
        ///   - MESSAGE_REACTION_REMOVE
        ///   - MESSAGE_REACTION_REMOVE_ALL
        ///   - MESSAGE_REACTION_REMOVE_EMOJI
        /// </summary>
        DirectMessageReactions = 1 << 13,

        /// <summary>
        /// Contains the following events:
        ///   - TYPING_START
        /// </summary>
        DirectMessageTyping = 1 << 14,

        /// <summary>
        /// Contains the following events:
        ///   - GUILD_SCHEDULED_EVENT_CREATE
        ///   - GUILD_SCHEDULED_EVENT_UPDATE
        ///   - GUILD_SCHEDULED_EVENT_DELETE
        ///   - GUILD_SCHEDULED_EVENT_USER_ADD
        ///   - GUILD_SCHEDULED_EVENT_USER_REMOVE
        /// </summary>
        GuildScheduledEvents = 1 << 16
    }
}
