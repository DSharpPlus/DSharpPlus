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
    public enum DiscordMessageType
    {
        Default = 0,
        RecipientAdd = 1,
        RecipientRemove = 2,
        Call = 3,
        ChannelNameChange = 4,
        ChannelIconChange = 5,
        ChannelPinnedMessage = 6,
        GuildMemberJoin = 7,
        UserPremiumGuildSubscription = 8,
        UserPremiumGuildSubscriptionTier1 = 9,
        UserPremiumGuildSubscriptionTier2 = 10,
        UserPremiumGuildSubscriptionTier3 = 11,
        ChannelFollowAdd = 12,
        GuildDiscoveryDisqualified = 14,
        GuildDiscoveryRequalified = 15,
        GuildDiscoveryGracePeriodInitialWarning = 16,
        GuildDiscoveryGracePeriodFinalWarning = 17,
        ThreadCreated = 18,

        /// <remarks>
        /// Only in API v8. In v6, they are <see cref="Default"/>
        /// </remarks>
        Reply = 19,

        /// <remarks>
        /// Only in API v8. In v6, they are <see cref="Default"/>
        /// </remarks>
        ChatInputCommand = 20,

        /// <remarks>
        /// Only in API v9.
        /// </remarks>
        ThreadStarterMessage = 21,
        GuildInviteReminder = 22,
        ContextMenuCommand = 23
    }
}
