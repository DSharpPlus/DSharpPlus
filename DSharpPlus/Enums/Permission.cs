using System;

namespace DSharpPlus
{
    /// <summary>
    /// Wether a permission is allowed, denied or unset
    /// </summary>
    public enum PermissionLevel
    {
        /// <summary>
        /// Said permission is Allowed
        /// </summary>
        Allowed,

        /// <summary>
        /// Said permission is Denied
        /// </summary>
        Denied,

        /// <summary>
        /// Said permission is Unset
        /// </summary>
        Unset
    }

    /// <summary>
    /// Bitwise permission flags.
    /// </summary>
    [Flags]
    public enum Permissions : ulong
    {
        /// <summary>
        /// Indicates no permissions given.
        /// </summary>
        [PermissionString("No permissions")]
        None = 0x0000000000000000u,

        /// <summary>
        /// Allows creation of instant channel invites.
        /// </summary>
        [PermissionString("Create instant invites")]
        CreateInstantInvite = 0x0000000000000001u,

        /// <summary>
        /// Allows kicking members.
        /// </summary>
        [PermissionString("Kick members")]
        KickMembers = 0x0000000000000002u,

        /// <summary>
        /// Allows banning and unbanning members.
        /// </summary>
        [PermissionString("Ban members")]
        BanMembers = 0x0000000000000004u,

        /// <summary>
        /// Enables full access on a given guild. This also overrides other permissions.
        /// </summary>
        [PermissionString("Administrator")]
        Administrator = 0x0000000000000008u,

        /// <summary>
        /// Allows managing channels.
        /// </summary>
        [PermissionString("Manage channels")]
        ManageChannels = 0x0000000000000010u,

        /// <summary>
        /// Allows managing the guild.
        /// </summary>
        [PermissionString("Manage guild")]
        ManageGuild = 0x0000000000000020u,

        /// <summary>
        /// Allows adding reactions to messages.
        /// </summary>
        [PermissionString("Add reactions")]
        AddReactions = 0x0000000000000040u,

        /// <summary>
        /// Allows viewing audit log entries.
        /// </summary>
        [PermissionString("View audit log")]
        ViewAuditLog = 0x0000000000000080u,

        /// <summary>
        /// Allows reading messages. Disabling this permission hides channels.
        /// </summary>
        [PermissionString("Read messages")]
        ReadMessages = 0x0000000000000400u,

        /// <summary>
        /// Allows sending messages.
        /// </summary>
        [PermissionString("Send messages")]
        SendMessages = 0x0000000000000800u,

        /// <summary>
        /// Allows sending text-to-speech messages.
        /// </summary>
        [PermissionString("Send TTS messages")]
        SendTtsMessages = 0x0000000000001000u,

        /// <summary>
        /// Allows managing messages of other users.
        /// </summary>
        [PermissionString("Manage messages")]
        ManageMessages = 0x0000000000002000u,

        /// <summary>
        /// Allows embedding content in messages.
        /// </summary>
        [PermissionString("Use embeds")]
        EmbedLinks = 0x0000000000004000u,

        /// <summary>
        /// Allows uploading files.
        /// </summary>
        [PermissionString("Attach files")]
        AttachFiles = 0x0000000000008000u,

        /// <summary>
        /// Allows reading message history.
        /// </summary>
        [PermissionString("Read message history")]
        ReadMessageHistory = 0x0000000000010000u,

        /// <summary>
        /// Allows using @everyone and @here mentions.
        /// </summary>
        [PermissionString("Mention everyone")]
        MentionEveryone = 0x0000000000020000u,

        /// <summary>
        /// Allows using emojis from external servers, such as twitch or nitro emojis.
        /// </summary>
        [PermissionString("Use external emojis")]
        UseExternalEmojis = 0x0000000000040000u,

        /// <summary>
        /// Allows connecting to voice chat.
        /// </summary>
        [PermissionString("Use voice chat")]
        UseVoice = 0x0000000000100000u,

        /// <summary>
        /// Allows speaking in voice chat.
        /// </summary>
        [PermissionString("Speak")]
        Speak = 0x0000000000200000u,

        /// <summary>
        /// Allows muting other members in voice chat.
        /// </summary>
        [PermissionString("Mute voice chat members")]
        MuteMembers = 0x0000000000400000u,

        /// <summary>
        /// Allows deafening other members in voice chat.
        /// </summary>
        [PermissionString("Deafen voice chat members")]
        DeafenMembers = 0x0000000000800000u,

        /// <summary>
        /// Allows moving voice chat members.
        /// </summary>
        [PermissionString("Move voice chat members")]
        MoveMembers = 0x0000000001000000u,

        /// <summary>
        /// Allows using voice activation in voice chat. Revoking this will usage of push-to-talk.
        /// </summary>
        [PermissionString("Use voice activity detection")]
        UseVoiceDetection = 0x0000000002000000u,

        /// <summary>
        /// Allows changing of own nickname.
        /// </summary>
        [PermissionString("Change own nickname")]
        ChangeNickname = 0x0000000004000000u,

        /// <summary>
        /// Allows managing nicknames of other members.
        /// </summary>
        [PermissionString("Manage nicknames")]
        ManageNicknames = 0x0000000008000000u,

        /// <summary>
        /// Allows managing roles in a guild.
        /// </summary>
        [PermissionString("Manage roles")]
        ManageRoles = 0x0000000010000000u,

        /// <summary>
        /// Allows managing webhooks in a guild.
        /// </summary>
        [PermissionString("Manage webhooks")]
        ManageWebhooks = 0x0000000020000000u,

        /// <summary>
        /// Allows managing guild emoji.
        /// </summary>
        [PermissionString("Manage emoji")]
        ManageEmojis = 0x0000000040000000u
    }

    /// <summary>
    /// Defines a readable name for this permission.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PermissionStringAttribute : Attribute
    {
        /// <summary>
        /// Gets the readable name for this permission.
        /// </summary>
        public string String { get; private set; }

        /// <summary>
        /// Defines a readable name for this permission.
        /// </summary>
        /// <param name="str">Readable name for this permission.</param>
        public PermissionStringAttribute(string str)
        {
            this.String = str;
        }
    }
}
