namespace DSharpPlus.Core.Enums
{
    public enum DiscordGatewayIntents
    {
        /// <summary>
        /// Contains the following events:<br/>
        ///   - GUILD_CREATE<br/>
        ///   - GUILD_UPDATE<br/>
        ///   - GUILD_DELETE<br/>
        ///   - GUILD_ROLE_CREATE<br/>
        ///   - GUILD_ROLE_UPDATE<br/>
        ///   - GUILD_ROLE_DELETE<br/>
        ///   - CHANNEL_CREATE<br/>
        ///   - CHANNEL_UPDATE<br/>
        ///   - CHANNEL_DELETE<br/>
        ///   - CHANNEL_PINS_UPDATE<br/>
        ///   - THREAD_CREATE<br/>
        ///   - THREAD_UPDATE<br/>
        ///   - THREAD_DELETE<br/>
        ///   - THREAD_LIST_SYNC<br/>
        ///   - THREAD_MEMBER_UPDATE<br/>
        ///   - THREAD_MEMBERS_UPDATE<br/> *
        ///   - STAGE_INSTANCE_CREATE<br/>
        ///   - STAGE_INSTANCE_UPDATE<br/>
        ///   - STAGE_INSTANCE_DELETE<br/>
        /// </summary>
        Guilds = 1 << 0,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - GUILD_MEMBER_ADD<br/>
        ///   - GUILD_MEMBER_UPDATE<br/>
        ///   - GUILD_MEMBER_REMOVE<br/>
        ///   - THREAD_MEMBERS_UPDATE<br/>
        /// </summary>
        /// <remarks>
        /// GUILD_MEMBER_UPDATE is sent for current-user updates regardless of whether the <see cref="GuildMembers"/> intent is set.
        /// Thread Members Update by default only includes if the current user was added to or removed from a thread. To receive these updates for other users, request the <see cref="GuildMembers"/> Gateway Intent.
        /// </remarks>
        GuildMembers = 1 << 1,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - GUILD_BAN_ADD<br/>
        ///   - GUILD_BAN_REMOVE<br/>
        /// </summary>
        GuildBans = 1 << 2,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - GUILD_EMOJIS_UPDATE<br/>
        ///   - GUILD_STICKERS_UPDATE<br/>
        /// </summary>
        GuildEmojisAndStickers = 1 << 3,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - GUILD_INTEGRATIONS_UPDATE<br/>
        ///   - INTEGRATION_CREATE<br/>
        ///   - INTEGRATION_UPDATE<br/>
        ///   - INTEGRATION_DELETE<br/>
        /// </summary>
        GuildIntegrations = 1 << 4,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - WEBHOOKS_UPDATE<br/>
        /// </summary>
        GuildWebhooks = 1 << 5,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - INVITE_CREATE<br/>
        ///   - INVITE_DELETE<br/>
        /// </summary>
        GuildInvites = 1 << 6,

        /// <summary>
        /// Contains the following events:<br/>
        ///    - VOICE_STATE_UPDATE
        /// </summary>
        GuildVoiceStates = 1 << 7,

        /// <summary>
        /// Contains the following events:<br/>
        ///    - PRESENCE_UPDATE
        /// </summary>
        GuildPresences = 1 << 8,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - MESSAGE_CREATE<br/>
        ///   - MESSAGE_UPDATE<br/>
        ///   - MESSAGE_DELETE<br/>
        ///   - MESSAGE_DELETE_BULK<br/>
        /// </summary>
        GuildMessages = 1 << 9,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - MESSAGE_REACTION_ADD<br/>
        ///   - MESSAGE_REACTION_REMOVE<br/>
        ///   - MESSAGE_REACTION_REMOVE_ALL<br/>
        ///   - MESSAGE_REACTION_REMOVE_EMOJI<br/>
        /// </summary>
        GuildMessageReactions = 1 << 10,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - TYPING_START<br/>
        /// </summary>
        GuildMessageTyping = 1 << 11,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - MESSAGE_CREATE<br/>
        ///   - MESSAGE_UPDATE<br/>
        ///   - MESSAGE_DELETE<br/>
        ///   - CHANNEL_PINS_UPDATE<br/>
        /// </summary>
        DirectMessages = 1 << 12,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - MESSAGE_REACTION_ADD<br/>
        ///   - MESSAGE_REACTION_REMOVE<br/>
        ///   - MESSAGE_REACTION_REMOVE_ALL<br/>
        ///   - MESSAGE_REACTION_REMOVE_EMOJI<br/>
        /// </summary>
        DirectMessageReactions = 1 << 13,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - TYPING_START<br/>
        /// </summary>
        DirectMessageTyping = 1 << 14,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - GUILD_SCHEDULED_EVENT_CREATE<br/>
        ///   - GUILD_SCHEDULED_EVENT_UPDATE<br/>
        ///   - GUILD_SCHEDULED_EVENT_DELETE<br/>
        ///   - GUILD_SCHEDULED_EVENT_USER_ADD<br/>
        ///   - GUILD_SCHEDULED_EVENT_USER_REMOVE<br/>
        /// </summary>
        GuildScheduledEvents = 1 << 16,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - AUTO_MODERATION_RULE_CREATE<br/>
        ///   - AUTO_MODERATION_RULE_UPDATE<br/>
        ///   - AUTO_MODERATION_RULE_DELETE<br/>
        /// </summary>
        /// <remarks>
        /// All auto moderation related events are currently only sent to bot users which have the <see cref="DiscordPermissions.ManageGuild"/> permission.
        /// </remarks>
        AutoModerationConfiguration = 1 << 20,

        /// <summary>
        /// Contains the following events:<br/>
        ///   - AUTO_MODERATION_ACTION_EXECUTION<br/>
        /// </summary>
        /// <remarks>
        /// All auto moderation related events are currently only sent to bot users which have the <see cref="DiscordPermissions.ManageGuild"/> permission.
        /// </remarks>
        AutoModerationExecution = 1 << 21
    }
}
