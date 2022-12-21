namespace DSharpPlus.Entities;

public enum DiscordAuditLogEvent
{
    /// <summary>
    /// Server settings were updated
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordGuild"/>
    /// </remarks>
    GuildUpdate = 1,

    /// <summary>
    /// Channel was created
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordChannel"/>
    /// </remarks>
    ChannelCreate = 10,

    /// <summary>
    /// Channel settings were updated
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordChannel"/>
    /// </remarks>
    ChannelUpdate = 11,

    /// <summary>
    /// Channel was deleted
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordChannel"/>
    /// </remarks>
    ChannelDelete = 12,

    /// <summary>
    /// Permission overwrite was added to a channel
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordChannelOverwrite"/>
    /// </remarks>
    ChannelOverwriteCreate = 13,

    /// <summary>
    /// Permission overwrite was updated for a channel
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordChannelOverwrite"/>
    /// </remarks>
    ChannelOverwriteUpdate = 14,

    /// <summary>
    /// Permission overwrite was deleted from a channel
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordChannelOverwrite"/>
    /// </remarks>
    ChannelOverwriteDelete = 15,

    /// <summary>
    /// Member was removed from server
    /// </summary>
    MemberKick = 20,

    /// <summary>
    /// Members were pruned from server
    /// </summary>
    MemberPrune = 21,

    /// <summary>
    /// Member was banned from server
    /// </summary>
    MemberBanAdd = 22,

    /// <summary>
    /// Server ban was lifted for a member
    /// </summary>
    MemberBanRemove = 23,

    /// <summary>
    /// Member was updated in server
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordMember"/>
    /// </remarks>
    MemberUpdate = 24,

    /// <summary>
    /// Member was added or removed from a role
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordRole"/>
    /// </remarks>
    MemberRoleUpdate = 25,

    /// <summary>
    /// Member was moved to a different voice channel
    /// </summary>
    MemberMove = 26,

    /// <summary>
    /// Member was disconnected from a voice channel
    /// </summary>
    MemberDisconnect = 27,

    /// <summary>
    /// Bot user was added to server
    /// </summary>
    BotAdd = 28,

    /// <summary>
    /// Role was created
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordRole"/>
    /// </remarks>
    RoleCreate = 30,

    /// <summary>
    /// Role was edited
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordRole"/>
    /// </remarks>
    RoleUpdate = 31,

    /// <summary>
    /// Role was deleted
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordRole"/>
    /// </remarks>
    RoleDelete = 32,

    /// <summary>
    /// Server invite was created
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordInvite"/>
    /// </remarks>
    InviteCreate = 40,

    /// <summary>
    /// Server invite was updated
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordInvite"/>
    /// </remarks>
    InviteUpdate = 41,

    /// <summary>
    /// Server invite was deleted
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordInvite"/>
    /// </remarks>
    InviteDelete = 42,

    /// <summary>
    /// Webhook was created
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordWebhook"/>
    /// </remarks>
    WebhookCreate = 50,

    /// <summary>
    /// Webhook properties or channel were updated
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordWebhook"/>
    /// </remarks>
    WebhookUpdate = 51,

    /// <summary>
    /// Webhook was deleted
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordWebhook"/>
    /// </remarks>
    WebhookDelete = 52,

    /// <summary>
    /// Emoji was created
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordEmoji"/>
    /// </remarks>
    EmojiCreate = 60,

    /// <summary>
    /// Emoji name was updated
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordEmoji"/>
    /// </remarks>
    EmojiUpdate = 61,

    /// <summary>
    /// Emoji was deleted
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordEmoji"/>
    /// </remarks>
    EmojiDelete = 62,

    /// <summary>
    /// Single message was deleted
    /// </summary>
    MessageDelete = 72,

    /// <summary>
    /// Multiple messages were deleted
    /// </summary>
    MessageBulkDelete = 73,

    /// <summary>
    /// Message was pinned to a channel
    /// </summary>
    MessagePin = 74,

    /// <summary>
    /// Message was unpinned from a channel
    /// </summary>
    MessageUnpin = 75,

    /// <summary>
    /// App was added to server
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordIntegration"/>
    /// </remarks>
    IntegrationCreate = 80,

    /// <summary>
    /// App was updated (as an example, its scopes were updated)
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordIntegration"/>
    /// </remarks>
    IntegrationUpdate = 81,

    /// <summary>
    /// App was removed from server
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordIntegration"/>
    /// </remarks>
    IntegrationDelete = 82,

    /// <summary>
    /// Stage instance was created (stage channel becomes live)
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordStageInstance"/>
    /// </remarks>
    StageInstanceCreate = 83,

    /// <summary>
    /// Stage instance details were updated
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordStageInstance"/>
    /// </remarks>
    StageInstanceUpdate = 84,

    /// <summary>
    /// Stage instance was deleted (stage channel no longer live)
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordStageInstance"/>
    /// </remarks>
    StageInstanceDelete = 85,

    /// <summary>
    /// Sticker was created
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordSticker"/>
    /// </remarks>
    StickerCreate = 90,

    /// <summary>
    /// Sticker details were updated
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordSticker"/>
    /// </remarks>
    StickerUpdate = 91,

    /// <summary>
    /// Sticker was deleted
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordSticker"/>
    /// </remarks>
    StickerDelete = 92,

    /// <summary>
    /// Event was created
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordGuildScheduledEvent"/>
    /// </remarks>
    GuildScheduledEventCreate = 100,

    /// <summary>
    /// Event was updated
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordGuildScheduledEvent"/>
    /// </remarks>
    GuildScheduledEventUpdate = 101,

    /// <summary>
    /// Event was cancelled
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordGuildScheduledEvent"/>
    /// </remarks>
    GuildScheduledEventDelete = 102,

    /// <summary>
    /// Thread was created in a channel
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordThread"/>
    /// </remarks>
    ThreadCreate = 110,

    /// <summary>
    /// Thread was updated
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordThread"/>
    /// </remarks>
    ThreadUpdate = 111,

    /// <summary>
    /// Thread was deleted
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="Entities.DiscordThread"/>
    /// </remarks>
    ThreadDelete = 112,

    /// <summary>
    /// Permissions were updated for a command
    /// </summary>
    /// <remarks>
    /// Object Changed: <see cref="DiscordPermissions"/>
    /// </remarks>
    ApplicationCommandPermissionUpdate = 121,

    /// <summary>
    /// An Auto Moderation rule was created.
    /// </summary>
    AutoModerationRuleCreate = 140,

    /// <summary>
    /// An Auto Moderation rule was updated.
    /// </summary>
    AutoModerationRuleUpdate = 141,

    /// <summary>
    /// An Auto Moderation rule was deleted.
    /// </summary>
    AutoModerationRuleDelete = 142,

    /// <summary>
    /// A message was blocked by AutoMod (according to a rule).
    /// </summary>
    AutoModerationRuleBlockMessage = 143
}
