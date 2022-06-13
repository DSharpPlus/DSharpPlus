using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSharpPlus.Core.Enums
{
    /// <summary>
    /// Permissions in Discord are a way to limit and grant certain abilities to users. A set of base permissions can be configured at the guild level for different roles. When these roles are attached to users, they grant or revoke specific privileges within the guild. Along with the guild-level permissions, Discord also supports permission overwrites that can be assigned to individual guild roles or guild members on a per-channel basis.
    /// </summary>
    [Flags, JsonConverter(typeof(StringEnumConverter))]
    public enum DiscordPermissions : long
    {
        /// <summary>
        /// No permissions.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allows creation of instant invites. Used on text, voice and stage channels.
        /// </summary>
        [Description("Create Instant Invite")]
        CreateInstantInvite = 0x0000000000000001,

        /// <summary>
        /// Allows kicking members.
        /// </summary>
        [Description("Kick Members")]
        KickMembers = 0x0000000000000002,

        /// <summary>
        /// Allows banning members.
        /// </summary>
        [Description("Ban Members")]
        BanMembers = 0x0000000000000004,

        /// <summary>
        /// Allows all permissions and bypasses channel permission overwrites.
        /// </summary>
        [Description("Administrator")]
        Administrator = 0x0000000000000008,

        /// <summary>
        /// Allows management and editing of channels. Used on text, voice and stage channels.
        /// </summary>
        [Description("Manage Channels")]
        ManageChannels = 0x0000000000000010,

        /// <summary>
        /// Allows management and editing of the guild.
        /// </summary>
        [Description("Manage Guild")]
        ManageGuild = 0x0000000000000020,

        /// <summary>
        /// Allows for the addition of reactions to messages. Used on text channels.
        /// </summary>
        [Description("Add Reactions")]
        AddReactions = 0x0000000000000040,

        /// <summary>
        /// Allows for viewing of audit logs.
        /// </summary>
        [Description("View Audit Log")]
        ViewAuditLog = 0x0000000000000080,

        /// <summary>
        /// Allows for using priority speaker in a voice channel.
        /// </summary>
        [Description("Priority Speaker")]
        PrioritySpeaker = 0x0000000000000100,

        /// <summary>
        /// Allows the user to go live. Used on voice channels.
        /// </summary>
        [Description("Stream")]
        Stream = 0x0000000000000200,

        /// <summary>
        /// Allows guild members to view a channel, which includes reading messages in text channels and joining voice channels. Used on text, voice and stage channels.
        /// </summary>
        [Description("View Channel")]
        ViewChannel = 0x0000000000000400,

        /// <summary>
        /// Allows for sending messages in a channel (does not allow sending messages in threads). Used on text channels.
        /// </summary>
        [Description("Send Messages")]
        SendMessages = 0x0000000000000800,

        /// <summary>
        /// Allows for sending of <c>/tts</c> messages
        /// </summary>
        [Description("Send TTS Messages")]
        SendTTSMessages = 0x0000000000001000,

        /// <summary>
        /// Allows for deletion of other users messages. Used on text channels.
        /// </summary>
        [Description("Manage Messages")]
        ManageMessages = 0x0000000000002000,

        /// <summary>
        /// Links sent by users with this permission will be auto-embedded. Used on text channels.
        /// </summary>
        [Description("Embed Links")]
        EmbedLinks = 0x0000000000004000,

        /// <summary>
        /// Allows for uploading images and files. Used on text channels.
        /// </summary>
        [Description("Attach Files")]
        AttachFiles = 0x0000000000008000,

        /// <summary>
        /// Allows for reading of message history. Used on text channels.
        /// </summary>
        [Description("Read Message History")]
        ReadMessageHistory = 0x0000000000010000,

        /// <summary>
        /// Allows for using the <c>@everyone</c> tag to notify all users in a channel, and the <c>@here</c> tag to notify all online users in a channel. Used on text channels.
        /// </summary>
        [Description("Mention Everyone")]
        MentionEveryone = 0x0000000000020000,

        /// <summary>
        /// Allows the usage of custom emojis from other servers. Used on text channels.
        /// </summary>
        [Description("Use External Emojis")]
        UseExternalEmojis = 0x0000000000040000,

        /// <summary>
        /// Allows for viewing guild insights.
        /// </summary>
        [Description("View Guild Insights")]
        ViewGuildInsights = 0x0000000000080000,

        /// <summary>
        /// Allows for joining of a voice channel. Used on voice and stage channels.
        /// </summary>
        [Description("Connect")]
        Connect = 0x0000000000100000,

        /// <summary>
        /// Allows for speaking in a voice channel. Used on voice channels.
        /// </summary>
        [Description("Speak")]
        Speak = 0x0000000000200000,

        /// <summary>
        /// Allows for muting members in a voice channel. Used on voice and stage channels.
        /// </summary>
        [Description("Mute Members")]
        MuteMembers = 0x0000000000400000,

        /// <summary>
        /// Allows for deafening of members in a voice channel. Used on voice and stage channels.
        /// </summary>
        [Description("Deafen Members")]
        DeafenMembers = 0x0000000000800000,

        /// <summary>
        /// Allows for moving of members between voice channels. Used on voice and stage channels.
        /// </summary>
        [Description("Move Members")]
        MoveMembers = 0x0000000001000000,

        /// <summary>
        /// Allows for using voice-activity-detection in a voice channel. Used on voice channels.
        /// </summary>
        [Description("Use Voice Activity")]
        UseVAD = 0x0000000002000000,

        /// <summary>
        /// Allows for modification of own nickname.
        /// </summary>
        [Description("Change Nickname")]
        ChangeNickname = 0x0000000004000000,

        /// <summary>
        /// Allows for modification of other users nicknames.
        /// </summary>
        [Description("Manage Nicknames")]
        ManageNicknames = 0x0000000008000000,

        /// <summary>
        /// Allows management and editing of roles. Used on text, voice and stage channels.
        /// </summary>
        [Description("Manage Roles")]
        ManageRoles = 0x0000000010000000,

        /// <summary>
        /// Allows management and editing of webhooks. Used on text channels.
        /// </summary>
        [Description("Manage Webhooks")]
        ManageWebhooks = 0x0000000020000000,

        /// <summary>
        /// Allows management and editing of emojis and stickers.
        /// </summary>
        [Description("Manage Emojis And Stickers")]
        ManageEmojisAndStickers = 0x0000000040000000,

        /// <summary>
        /// Allows members to use application commands, including slash commands and context menu commands. Used on text channels.
        /// </summary>
        [Description("Use Application Commands")]
        UseApplicationCommands = 0x0000000080000000,

        /// <summary>
        /// Allows for requesting to speak in stage channels. <i>(This permission is under active development and may be changed or removed.)</i> Used on stage channels.
        /// </summary>
        [Description("Request To Speak")]
        RequestToSpeak = 0x0000000100000000,

        /// <summary>
        /// Allows for creating, editing, and deleting scheduled events. Used on voice and stage channels.
        /// </summary>
        [Description("Manage Events")]
        ManageEvents = 0x0000000200000000,

        /// <summary>
        /// Allows for deleting and archiving threads, and viewing all private threads. Used on text channels.
        /// </summary>
        [Description("Manage Threads")]
        ManageThreads = 0x0000000400000000,

        /// <summary>
        /// Allows for creating public and announcement threads. Used on text channels.
        /// </summary>
        [Description("Create Public Threads")]
        CreatePublicThreads = 0x0000000800000000,

        /// <summary>
        /// Allows for creating private threads. Used on text channels.
        /// </summary>
        [Description("Create Private Threads")]
        CreatePrivateThreads = 0x0000001000000000,

        /// <summary>
        /// Allows the usage of custom stickers from other servers. Used on text channels.
        /// </summary>
        [Description("Use External Stickers")]
        UseExternalStickers = 0x0000002000000000,

        /// <summary>
        /// Allows for sending messages in threads. Used on text channels.
        /// </summary>
        [Description("Send Messages In Threads")]
        SendMessagesInThreads = 0x0000004000000000,

        /// <summary>
        /// Allows for launching activities (applications with the <c>EMBEDDED</c> flag) in a voice channel. Used on voice channels.
        /// </summary>
        [Description("Launch Applications")]
        StartEmbeddedActivities = 0x0000008000000000,

        /// <summary>
        /// Allows for timing out users to prevent them from sending or reacting to messages in chat and threads, and from speaking in voice and stage channels.
        /// </summary>
        [Description("Timeout Members")]
        ModerateMembers = 0x0000010000000000,

        /// <summary>
        /// All permissions. Not found in the Discord Api documentation.
        /// </summary>
        [Description("All Permissions")]
        AllPerms = 0xFFFFFFFFFF
    }

    /// <summary>
    /// Adds convenience methods to the <see cref="Permissions"/> enum.
    /// </summary>
    public static class DiscordPermissionsExtensionMethods
    {
        /// <summary>
        /// Tests to see if the specified permissions are allowed by matching against both <see cref="DiscordPermissions.Administrator"/> and the requested permissions.
        /// </summary>
        /// <param name="permissions">The permissions to check against.</param>
        /// <param name="permission">The permissions to check for.</param>
        /// <returns><see cref="true"/> if the permission was found, otherwise <see cref="false"/>.</returns>
        public static bool HasPermission(this DiscordPermissions permissions, DiscordPermissions permission) => (permissions & DiscordPermissions.Administrator) == permissions || (permissions & permission) == permission; // Because Enum.HasFlag is slow.

        /// <summary>
        /// Grants permissions.
        /// </summary>
        /// <param name="permissions">The permissions to add to.</param>
        /// <param name="grant">Permission to add.</param>
        /// <returns>The permission set with the newly aquired permissions.</returns>
        public static DiscordPermissions Grant(this DiscordPermissions permissions, DiscordPermissions grant) => permissions | grant;

        /// <summary>
        /// Revokes permissions.
        /// </summary>
        /// <param name="permissions">The DiscordPermissions to take from.</param>
        /// <param name="revoke">Permission to take.</param>
        /// <returns>The permission set without the now removed permission.</returns>
        public static DiscordPermissions Revoke(this DiscordPermissions permissions, DiscordPermissions revoke) => permissions & ~revoke;
    }
}
