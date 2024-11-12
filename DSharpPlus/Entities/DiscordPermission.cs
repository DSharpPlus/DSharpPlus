using System.ComponentModel.DataAnnotations;

using NetEscapades.EnumGenerators;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a single discord permission.
/// </summary>
[EnumExtensions]
public enum DiscordPermission
{
    /// <summary>
    /// Allows members to create invites.
    /// </summary>
    [Display(Name = "Create Invites")]
    CreateInvite = 0,

    /// <summary>
    /// Allows members to kick others, limited by role hierarchy.
    /// </summary>
    [Display(Name = "Kick Members")]
    KickMembers = 1,

    /// <summary>
    /// Allows members to ban others, limited by role hierarchy.
    /// </summary>
    [Display(Name = "Ban Members")]
    BanMembers = 2,

    /// <summary>
    /// Administrator permission. Overrides every other permission, allows bypassing channel-specific restrictions.
    /// </summary>
    [Display(Name = "Administrator")]
    Administrator = 3,

    /// <summary>
    /// Allows members to create, edit and delete channels.
    /// </summary>
    [Display(Name = "Manage Channels")]
    ManageChannels = 4,

    /// <summary>
    /// Allows members to change (most) guild settings.
    /// </summary>
    [Display(Name = "Manage Guild")]
    ManageGuild = 5,

    /// <summary>
    /// Allows members to add a reaction to a message.
    /// </summary>
    [Display(Name = "Add Reactions")]
    AddReactions = 6,

    /// <summary>
    /// Allows members to access the guild's audit logs.
    /// </summary>
    [Display(Name = "View Audit Log")]
    ViewAuditLog = 7,

    /// <summary>
    /// Allows members to use Priority Speaker functionality.
    /// </summary>
    [Display(Name = "Priority Speaker")]
    PrioritySpeaker = 8,

    /// <summary>
    /// Allows members to go live in voice channels.
    /// </summary>
    [Display(Name = "Stream")]
    Stream = 9,

    /// <summary>
    /// Allows members to view (read) channels.
    /// </summary>
    [Display(Name = "View Channel")]
    ViewChannel = 10,

    /// <summary>
    /// Allows members to send messages in channels and to create threads in a forum channel.
    /// </summary>
    [Display(Name = "Send Messages")]
    SendMessages = 11,

    /// <summary>
    /// Allows members to send text-to-speech messages.
    /// </summary>
    [Display(Name = "Send Text-to-speech Messages")]
    SendTtsMessages = 12,

    /// <summary>
    /// Allows members to delete other's messages.
    /// </summary>
    [Display(Name = "Manage Messages")]
    ManageMessages = 13,

    /// <summary>
    /// Allows members' messages to embed sent links.
    /// </summary>
    [Display(Name = "Embed Links")]
    EmbedLinks = 14,

    /// <summary>
    /// Allows members to attach files.
    /// </summary>
    [Display(Name = "Attach Files")]
    AttachFiles = 15,

    /// <summary>
    /// Allows members to read a channels' message history.
    /// </summary>
    [Display(Name = "Read Message History")]
    ReadMessageHistory = 16,

    /// <summary>
    /// Allows members to mention @everyone, @here and all roles.
    /// </summary>
    [Display(Name = "Mention Everyone")]
    MentionEveryone = 17,

    /// <summary>
    /// Allows members to use emojis from other guilds.
    /// </summary>
    [Display(Name = "Use External Emojis")]
    UseExternalEmojis = 18,

    /// <summary>
    /// Allows members to access and view the Guild Insights menu.
    /// </summary>
    [Display(Name = "View Guild Insights")]
    ViewGuildInsights = 19,

    /// <summary>
    /// Allows members to connect to voice channels.
    /// </summary>
    [Display(Name = "Connect")]
    Connect = 20,

    /// <summary>
    /// Allows members to speak in voice channels.
    /// </summary>
    [Display(Name = "Speak")]
    Speak = 21,

    /// <summary>
    /// Allows members to mute others in voice channels.
    /// </summary>
    [Display(Name = "Mute Members")]
    MuteMembers = 22,

    /// <summary>
    /// Allows members to deafen others in voice channels.
    /// </summary>
    [Display(Name = "Deafen Members")]
    DeafenMembers = 23,

    /// <summary>
    /// Allows members to move others between voice channels they have access to.
    /// </summary>
    [Display(Name = "Move Members")]
    MoveMembers = 24,

    /// <summary>
    /// Allows members to use voice activity detection instead of push-to-talk.
    /// </summary>
    [Display(Name = "Use Voice Activity Detection")]
    UseVoiceActivity = 25,

    /// <summary>
    /// Allows members to change their own nickname.
    /// </summary>
    [Display(Name = "Change Nickname")]
    ChangeNickname = 26,

    /// <summary>
    /// Allows members to change and remove other's nicknames.
    /// </summary>
    [Display(Name = "Manage Nicknames")]
    ManageNicknames = 27,

    /// <summary>
    /// Allows members to create, change and grant roles lower than their highest role.
    /// </summary>
    [Display(Name = "Manage Roles")]
    ManageRoles = 28,

    /// <summary>
    /// Allows members to create and delete webhooks.
    /// </summary>
    [Display(Name = "Manage Webhooks")]
    ManageWebhooks = 29,

    /// <summary>
    /// Allows members to manage guild emojis and stickers.
    /// </summary>
    [Display(Name = "Manage Guild Expressions")]
    ManageGuildExpressions = 30,

    /// <summary>
    /// Allows members to use slash and right-click commands.
    /// </summary>
    [Display(Name = "Use Application Commands")]
    UseApplicationCommands = 31,

    /// <summary>
    /// Allows members to request to speak in stage channels.
    /// </summary>
    [Display(Name = "Request to Speak")]
    RequestToSpeak = 32,

    /// <summary>
    /// Allows members to create, edit and delete events.
    /// </summary>
    [Display(Name = "Manage Events")]
    ManageEvents = 33,

    /// <summary>
    /// Allows members to manage threads.
    /// </summary>
    [Display(Name = "Manage Threads")]
    ManageThreads = 34,

    /// <summary>
    /// Allows members to create public threads.
    /// </summary>
    [Display(Name = "Create Public Threads")]
    CreatePublicThreads = 35,

    /// <summary>
    /// Allows members to create private threads.
    /// </summary>
    [Display(Name = "Create Private Threads")]
    CreatePrivateThreads = 36,

    /// <summary>
    /// Allows members to use stickers from other guilds.
    /// </summary>
    [Display(Name = "Use External Stickers")]
    UseExternalStickers = 37,

    /// <summary>
    /// Allows members to send messages in threads.
    /// </summary>
    [Display(Name = "Send Messages in Threads")]
    SendThreadMessages = 38,

    /// <summary>
    /// Allows members to start embedded activities.
    /// </summary>
    [Display(Name = "Start Embedded Activities")]
    StartEmbeddedActivities = 39,

    /// <summary>
    /// Allows members to time out other members.
    /// </summary>
    [Display(Name = "Moderate Members")]
    ModerateMembers = 40,

    /// <summary>
    /// Allows members to view role subscription insights.
    /// </summary>
    [Display(Name = "View Creator Monetization Analytics")]
    ViewCreatorMonetizationAnalytics = 41,

    /// <summary>
    /// Allows members to use the soundboard in a voice channel.
    /// </summary>
    [Display(Name = "Use Soundboard")]
    UseSoundboard = 42,

    /// <summary>
    /// Allows members to send voice messages.
    /// </summary>
    [Display(Name = "Send Voice Messages")]
    SendVoiceMessages = 46,

    /// <summary>
    /// Allows members to send polls.
    /// </summary>
    [Display(Name = "Send Polls")]
    SendPolls = 49,

    /// <summary>
    /// Allows members to use external, user-installable apps.
    /// </summary>
    [Display(Name = "Use External Apps")]
    UseExternalApps = 50
}

