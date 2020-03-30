using System;

namespace DSharpPlus
{
    public static class PermissionMethods
    {
        internal static Permissions FULL_PERMS { get; } = (Permissions)2147483647L;

        /// <summary>
        /// Calculates whether this permission set contains the given permission.
        /// </summary>
        /// <param name="p">The permissions to calculate from</param>
        /// <param name="permission">permission you want to check</param>
        /// <returns></returns>
        public static bool HasPermission(this Permissions p, Permissions permission) => (p & permission) == permission;

        /// <summary>
        /// Grants permissions.
        /// </summary>
        /// <param name="p">The permissions to add to.</param>
        /// <param name="grant">Permission to add.</param>
        /// <returns></returns>
        public static Permissions Grant(this Permissions p, Permissions grant) => p | grant;

        /// <summary>
        /// Revokes permissions.
        /// </summary>
        /// <param name="p">The permissions to take from.</param>
        /// <param name="revoke">Permission to take.</param>
        /// <returns></returns>
        public static Permissions Revoke(this Permissions p, Permissions revoke) => p & ~revoke;
    }

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
    public enum Permissions : long
    {
        /// <summary>
        /// Indicates no permissions given.
        /// </summary>
        [PermissionString("No permissions")]
        None = 0x0000000000000000,

        /// <summary>
        /// Indicates all permissions are granted
        /// </summary>
        [PermissionString("All permissions")]
        All = 2147483647,

        /// <summary>
        /// Allows creation of instant channel invites.
        /// </summary>
        [PermissionString("Create instant invites")]
        CreateInstantInvite = 0x0000000000000001,

        /// <summary>
        /// Allows kicking members.
        /// </summary>
        [PermissionString("Kick members")]
        KickMembers = 0x0000000000000002,

        /// <summary>
        /// Allows banning and unbanning members.
        /// </summary>
        [PermissionString("Ban members")]
        BanMembers = 0x0000000000000004,

        /// <summary>
        /// Enables full access on a given guild. This also overrides other permissions.
        /// </summary>
        [PermissionString("Administrator")]
        Administrator = 0x0000000000000008,

        /// <summary>
        /// Allows managing channels.
        /// </summary>
        [PermissionString("Manage channels")]
        ManageChannels = 0x0000000000000010,

        /// <summary>
        /// Allows managing the guild.
        /// </summary>
        [PermissionString("Manage guild")]
        ManageGuild = 0x0000000000000020,

        /// <summary>
        /// Allows adding reactions to messages.
        /// </summary>
        [PermissionString("Add reactions")]
        AddReactions = 0x0000000000000040,

        /// <summary>
        /// Allows viewing audit log entries.
        /// </summary>
        [PermissionString("View audit log")]
        ViewAuditLog = 0x0000000000000080,

        /// <summary>
        /// Allows the use of priority speaker.
        /// </summary>
        [PermissionString("Use priority speaker")]
        PrioritySpeaker = 0x0000000000000100,

        /// <summary>
        /// Allows accessing text and voice channels. Disabling this permission hides channels.
        /// </summary>
        [PermissionString("Read messages")]
        AccessChannels = 0x0000000000000400,

        /// <summary>
        /// Allows sending messages.
        /// </summary>
        [PermissionString("Send messages")]
        SendMessages = 0x0000000000000800,

        /// <summary>
        /// Allows sending text-to-speech messages.
        /// </summary>
        [PermissionString("Send TTS messages")]
        SendTtsMessages = 0x0000000000001000,

        /// <summary>
        /// Allows managing messages of other users.
        /// </summary>
        [PermissionString("Manage messages")]
        ManageMessages = 0x0000000000002000,

        /// <summary>
        /// Allows embedding content in messages.
        /// </summary>
        [PermissionString("Use embeds")]
        EmbedLinks = 0x0000000000004000,

        /// <summary>
        /// Allows uploading files.
        /// </summary>
        [PermissionString("Attach files")]
        AttachFiles = 0x0000000000008000,

        /// <summary>
        /// Allows reading message history.
        /// </summary>
        [PermissionString("Read message history")]
        ReadMessageHistory = 0x0000000000010000,

        /// <summary>
        /// Allows using @everyone and @here mentions.
        /// </summary>
        [PermissionString("Mention everyone")]
        MentionEveryone = 0x0000000000020000,

        /// <summary>
        /// Allows using emojis from external servers, such as twitch or nitro emojis.
        /// </summary>
        [PermissionString("Use external emojis")]
        UseExternalEmojis = 0x0000000000040000,

        /// <summary>
        /// Allows connecting to voice chat.
        /// </summary>
        [PermissionString("Use voice chat")]
        UseVoice = 0x0000000000100000,

        /// <summary>
        /// Allows speaking in voice chat.
        /// </summary>
        [PermissionString("Speak")]
        Speak = 0x0000000000200000,

        /// <summary>
        /// Allows muting other members in voice chat.
        /// </summary>
        [PermissionString("Mute voice chat members")]
        MuteMembers = 0x0000000000400000,

        /// <summary>
        /// Allows deafening other members in voice chat.
        /// </summary>
        [PermissionString("Deafen voice chat members")]
        DeafenMembers = 0x0000000000800000,

        /// <summary>
        /// Allows moving voice chat members.
        /// </summary>
        [PermissionString("Move voice chat members")]
        MoveMembers = 0x0000000001000000,

        /// <summary>
        /// Allows using voice activation in voice chat. Revoking this will usage of push-to-talk.
        /// </summary>
        [PermissionString("Use voice activity detection")]
        UseVoiceDetection = 0x0000000002000000,

        /// <summary>
        /// Allows changing of own nickname.
        /// </summary>
        [PermissionString("Change own nickname")]
        ChangeNickname = 0x0000000004000000,

        /// <summary>
        /// Allows managing nicknames of other members.
        /// </summary>
        [PermissionString("Manage nicknames")]
        ManageNicknames = 0x0000000008000000,

        /// <summary>
        /// Allows managing roles in a guild.
        /// </summary>
        [PermissionString("Manage roles")]
        ManageRoles = 0x0000000010000000,

        /// <summary>
        /// Allows managing webhooks in a guild.
        /// </summary>
        [PermissionString("Manage webhooks")]
        ManageWebhooks = 0x0000000020000000,

        /// <summary>
        /// Allows managing guild emoji.
        /// </summary>
        [PermissionString("Manage emoji")]
        ManageEmojis = 0x0000000040000000,

        /// <summary>
        /// Allows the user to go live.
        /// </summary>
        [PermissionString("Allow stream")]
        Stream	= 0x0000000000000200
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
        public string String { get; }

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
