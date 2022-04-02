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
    public enum DiscordAuditLogEvent
    {
        GuildUpdate = 1,
        ChannelCreate = 10,
        ChannelUpdate = 11,
        ChannelDelete = 12,
        ChannelOverwriteCreate = 13,
        ChannelOverwriteUpdate = 14,
        ChannelOverwriteDelete = 15,
        MemberKick = 20,
        MemberPrune = 21,
        MemberBanAdd = 22,
        MemberBanRemove = 23,
        MemberUpdate = 24,
        MemberRoleUpdate = 25,
        MemberMove = 26,
        MemberDisconnect = 27,
        BotAdd = 28,
        RoleCreate = 30,
        RoleUpdate = 31,
        RoleDelete = 32,
        InviteCreate = 40,
        InviteUpdate = 41,
        InviteDelete = 42,
        WebhookCreate = 50,
        WebhookUpdate = 51,
        WebhookDelete = 52,
        EmojiCreate = 60,
        EmojiUpdate = 61,
        EmojiDelete = 62,
        MessageDelete = 72,
        MessageBulkDelete = 73,
        MessagePin = 74,
        MessageUnpin = 75,
        IntegrationCreate = 80,
        IntegrationUpdate = 81,
        IntegrationDelete = 82,
        StageInstanceCreate = 83,
        StageInstanceUpdate = 84,
        StageInstanceDelete = 85,
        StickerCreate = 90,
        StickerUpdate = 91,
        StickerDelete = 92,
        GuildScheduledEventCreate = 100,
        GuildScheduledEventUpdate = 101,
        GuildScheduledEventDelete = 102,
        ThreadCreate = 110,
        ThreadUpdate = 111,
        ThreadDelete = 112
    }
}
