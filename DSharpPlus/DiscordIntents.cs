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

namespace DSharpPlus;

public static class DiscordIntentExtensions
{
    /// <summary>
    /// Calculates whether these intents have a certain intent.
    /// </summary>
    /// <param name="intents">The base intents.</param>
    /// <param name="search">The intents to search for.</param>
    /// <returns></returns>
    public static bool HasIntent(this DiscordIntents intents, DiscordIntents search)
        => (intents & search) == search;

    /// <summary>
    /// Adds an intent to these intents.
    /// </summary>
    /// <param name="intents">The base intents.</param>
    /// <param name="toAdd">The intents to add.</param>
    /// <returns></returns>
    public static DiscordIntents AddIntent(this DiscordIntents intents, DiscordIntents toAdd)
        => intents |= toAdd;

    /// <summary>
    /// Removes an intent from these intents.
    /// </summary>
    /// <param name="intents">The base intents.</param>
    /// <param name="toRemove">The intents to remove.</param>
    /// <returns></returns>
    public static DiscordIntents RemoveIntent(this DiscordIntents intents, DiscordIntents toRemove)
        => intents &= ~toRemove;

    internal static bool HasAllPrivilegedIntents(this DiscordIntents intents)
        => intents.HasIntent(DiscordIntents.GuildMembers | DiscordIntents.GuildPresences);
}

/// <summary>
/// Represents gateway intents to be specified for connecting to Discord.
/// </summary>
[Flags]
public enum DiscordIntents
{
    /// <summary>
    /// Whether to include general guild events.
    /// <para>These include <see cref="DiscordClient.GuildCreated"/>, <see cref="DiscordClient.GuildDeleted"/>, <see cref="DiscordClient.GuildAvailable"/>, <see cref="DiscordClient.GuildDownloadCompleted"/>,</para>
    /// <para><see cref="DiscordClient.GuildRoleCreated"/>, <see cref="DiscordClient.GuildRoleUpdated"/>, <see cref="DiscordClient.GuildRoleDeleted"/>,</para>
    /// <para><see cref="DiscordClient.ChannelCreated"/>, <see cref="DiscordClient.ChannelUpdated"/>, <see cref="DiscordClient.ChannelDeleted"/>, and <see cref="DiscordClient.ChannelPinsUpdated"/>.</para>
    /// </summary>
    Guilds = 1 << 0,

    /// <summary>
    /// Whether to include guild member events.
    /// <para>These include <see cref="DiscordClient.GuildMemberAdded"/>, <see cref="DiscordClient.GuildMemberUpdated"/>, and <see cref="DiscordClient.GuildMemberRemoved"/>.</para>
    /// <para>This is a privileged intent, and must be enabled on the bot's developer page.</para>
    /// </summary>
    GuildMembers = 1 << 1,

    /// <summary>
    /// Whether to include guild ban events.
    /// <para>These include <see cref="DiscordClient.GuildBanAdded"/>, <see cref="DiscordClient.GuildBanRemoved"/> and <see cref="DiscordClient.GuildAuditLogCreated"/>.</para>
    /// </summary>
    GuildModeration = 1 << 2,

    /// <summary>
    /// Whether to include guild emoji events.
    /// <para>This includes <see cref="DiscordClient.GuildEmojisUpdated"/>.</para>
    /// </summary>
    GuildEmojisAndStickers = 1 << 3,

    /// <summary>
    /// Whether to include guild integration events.
    /// <para>This includes <see cref="DiscordClient.GuildIntegrationsUpdated"/>.</para>
    /// </summary>
    GuildIntegrations = 1 << 4,

    /// <summary>
    /// Whether to include guild webhook events.
    /// <para>This includes <see cref="DiscordClient.WebhooksUpdated"/>.</para>
    /// </summary>
    GuildWebhooks = 1 << 5,

    /// <summary>
    /// Whether to include guild invite events.
    /// <para>These include <see cref="DiscordClient.InviteCreated"/>, and <see cref="DiscordClient.InviteDeleted"/>.</para>
    /// </summary>
    GuildInvites = 1 << 6,

    /// <summary>
    /// Whether to include guild voice state events.
    /// <para>This includes <see cref="DiscordClient.VoiceStateUpdated"/>.</para>
    /// </summary>
    GuildVoiceStates = 1 << 7,

    /// <summary>
    /// Whether to include guild presence events.
    /// <para>This includes <see cref="DiscordClient.PresenceUpdated"/>.</para>
    /// <para>This is a privileged intent, and must be enabled on the bot's developer page.</para>
    /// </summary>
    GuildPresences = 1 << 8,

    /// <summary>
    /// Whether to include guild message events.
    /// <para>These include <see cref="DiscordClient.MessageCreated"/>, <see cref="DiscordClient.MessageUpdated"/>, and <see cref="DiscordClient.MessageDeleted"/>.</para>
    /// </summary>
    GuildMessages = 1 << 9,

    /// <summary>
    /// Whether to include guild reaction events.
    /// <para>These include <see cref="DiscordClient.MessageReactionAdded"/>, <see cref="DiscordClient.MessageReactionRemoved"/>, <see cref="DiscordClient.MessageReactionsCleared"/>,</para>
    /// <para>and <see cref="DiscordClient.MessageReactionRemovedEmoji"/>.</para>
    /// </summary>
    GuildMessageReactions = 1 << 10,

    /// <summary>
    /// Whether to include guild typing events.
    /// <para>These include <see cref="DiscordClient.TypingStarted"/>.</para>
    /// </summary>
    GuildMessageTyping = 1 << 11,

    /// <summary>
    /// Whether to include general direct message events.
    /// <para>These include <see cref="DiscordClient.ChannelCreated"/>, <see cref="DiscordClient.MessageCreated"/>, <see cref="DiscordClient.MessageUpdated"/>, </para>
    /// <para><see cref="DiscordClient.MessageDeleted"/>, <see cref="DiscordClient.ChannelPinsUpdated"/>.</para>
    /// <para>These events only fire for DM channels.</para>
    /// </summary>
    DirectMessages = 1 << 12,

    /// <summary>
    /// Whether to include direct message reaction events.
    /// <para>These include <see cref="DiscordClient.MessageReactionAdded"/>, <see cref="DiscordClient.MessageReactionRemoved"/>,</para>
    /// <para><see cref="DiscordClient.MessageReactionsCleared"/>, and <see cref="DiscordClient.MessageReactionRemovedEmoji"/>.</para>
    /// <para>These events only fire for DM channels.</para>
    /// </summary>
    DirectMessageReactions = 1 << 13,

    /// <summary>
    /// Whether to include direct message typing events.
    /// <para>This includes <see cref="DiscordClient.TypingStarted"/>.</para>
    /// <para>This event only fires for DM channels.</para>
    /// </summary>
    DirectMessageTyping = 1 << 14,

    /// <summary>
    /// Whether to include message content. This is a privileged event.
    /// <para>Message content includes text, attachments, embeds, components, and reply content.</para>
    /// <para>This intent is required for CommandsNext to function correctly.</para>
    /// </summary>
    MessageContents = 1 << 15,

    /// <summary>
    /// Whether to include scheduled event messages.
    /// </summary>
    ScheduledGuildEvents = 1 << 16,

    /// <summary>
    /// Whetever to include creation, modification or deletion of an auto-Moderation rule.
    /// </summary>
    AutoModerationEvents = 1 << 20,

    /// <summary>
    /// Whetever to include when an auto-moderation rule was fired.
    /// </summary>
    AutoModerationExecution = 1 << 21,

    /// <summary>
    /// Includes all unprivileged intents.
    /// <para>These are all intents excluding <see cref="DiscordIntents.GuildMembers"/> and <see cref="DiscordIntents.GuildPresences"/>.</para>
    /// </summary>
    AllUnprivileged = Guilds | GuildModeration | GuildEmojisAndStickers | GuildIntegrations | GuildWebhooks | GuildInvites | GuildVoiceStates | GuildMessages |
                      GuildMessageReactions | GuildMessageTyping | DirectMessages | DirectMessageReactions | DirectMessageTyping | ScheduledGuildEvents | 
                      AutoModerationEvents | AutoModerationExecution,

    /// <summary>
    /// Includes all intents.
    /// <para>The <see cref="DiscordIntents.GuildMembers"/> and <see cref="DiscordIntents.GuildPresences"/> intents are privileged, and must be enabled on the bot's developer page.</para>
    /// </summary>
    All = AllUnprivileged | GuildMembers | GuildPresences | MessageContents
}
