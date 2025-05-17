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
    /// By default, no Discord Intents are requested from the Discord gateway.
    /// </summary>
    None = 0,

    /// <summary>
    /// Whether to include general guild events.
    /// These include GuildCreated, GuildDeleted, GuildAvailable, GuildDownloadCompleted,
    /// GuildRoleCreated, GuildRoleUpdated, GuildRoleDeleted,
    /// ChannelCreated, ChannelUpdated, ChannelDeleted, and ChannelPinsUpdated.
    /// </summary>
    Guilds = 1 << 0,

    /// <summary>
    /// Whether to include guild member events.
    /// These include GuildMemberAdded, GuildMemberUpdated, and GuildMemberRemoved.
    /// This is a privileged intent, and must be enabled on the bot's developer page.
    /// </summary>
    GuildMembers = 1 << 1,

    /// <summary>
    /// Whether to include guild ban events.
    /// <para>These include GuildBanAdded, GuildBanRemoved and GuildAuditLogCreated.</para>
    /// </summary>
    GuildModeration = 1 << 2,

    /// <summary>
    /// Whether to include guild emoji events.
    /// <para>This includes GuildEmojisUpdated.</para>
    /// </summary>
    GuildEmojisAndStickers = 1 << 3,

    /// <summary>
    /// Whether to include guild integration events.
    /// <para>This includes GuildIntegrationsUpdated.</para>
    /// </summary>
    GuildIntegrations = 1 << 4,

    /// <summary>
    /// Whether to include guild webhook events.
    /// <para>This includes WebhooksUpdated.</para>
    /// </summary>
    GuildWebhooks = 1 << 5,

    /// <summary>
    /// Whether to include guild invite events.
    /// <para>These include InviteCreated and InviteDeleted.</para>
    /// </summary>
    GuildInvites = 1 << 6,

    /// <summary>
    /// Whether to include guild voice state events.
    /// <para>This includes VoiceStateUpdated.</para>
    /// </summary>
    GuildVoiceStates = 1 << 7,

    /// <summary>
    /// Whether to include guild presence events.
    /// <para>This includes PresenceUpdated.</para>
    /// <para>This is a privileged intent, and must be enabled on the bot's developer page.</para>
    /// </summary>
    GuildPresences = 1 << 8,

    /// <summary>
    /// Whether to include guild message events.
    /// <para>These include MessageCreated, MessageUpdated, and MessageDeleted.</para>
    /// </summary>
    GuildMessages = 1 << 9,

    /// <summary>
    /// Whether to include guild reaction events.
    /// These include MessageReactionAdded, MessageReactionRemoved, MessageReactionsCleared,
    /// and MessageReactionRemovedEmoji.
    /// </summary>
    GuildMessageReactions = 1 << 10,

    /// <summary>
    /// Whether to include guild typing events.
    /// <para>These include TypingStarted.</para>
    /// </summary>
    GuildMessageTyping = 1 << 11,

    /// <summary>
    /// Whether to include general direct message events.
    /// These include ChannelCreated, MessageCreated, MessageUpdated,
    /// MessageDeleted, ChannelPinsUpdated.
    /// These events only fire for DM channels.
    /// </summary>
    DirectMessages = 1 << 12,

    /// <summary>
    /// Whether to include direct message reaction events.
    /// These include MessageReactionAdded, MessageReactionRemoved,
    /// MessageReactionsCleared, and MessageReactionRemovedEmoji.
    /// These events only fire for DM channels.
    /// </summary>
    DirectMessageReactions = 1 << 13,

    /// <summary>
    /// Whether to include direct message typing events.
    /// <para>This includes TypingStarted.</para>
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
    ///  Whetever to include add and remove of a poll votes events in guilds.
    /// <para>This includes MessagePollVoted</para>
    /// </summary>
    GuildMessagePolls = 1 << 24,

    /// <summary>
    ///  Whetever to include add and remove of a poll votes events in direct messages.
    /// <para>This includes MessagePollVoted</para>
    /// </summary>
    DirectMessagePolls = 1 << 25,

    /// <summary>
    /// Includes all unprivileged intents.
    /// <para>These are all intents excluding <see cref="DiscordIntents.GuildMembers"/> and <see cref="DiscordIntents.GuildPresences"/>.</para>
    /// </summary>
    AllUnprivileged = Guilds | GuildModeration | GuildEmojisAndStickers | GuildIntegrations | GuildWebhooks | GuildInvites | GuildVoiceStates | GuildMessages |
                      GuildMessageReactions | GuildMessageTyping | DirectMessages | DirectMessageReactions | DirectMessageTyping | ScheduledGuildEvents |
                      AutoModerationEvents | AutoModerationExecution | GuildMessagePolls | DirectMessagePolls,

    /// <summary>
    /// Includes all intents.
    /// <para>The <see cref="DiscordIntents.GuildMembers"/> and <see cref="DiscordIntents.GuildPresences"/> intents are privileged, and must be enabled on the bot's developer page.</para>
    /// </summary>
    All = AllUnprivileged | GuildMembers | GuildPresences | MessageContents
}
