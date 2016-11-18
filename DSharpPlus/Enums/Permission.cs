namespace DSharpPlus
{
    /// <summary>
    /// Bitwise permission flags
    /// </summary>
    public enum Permission : int
    {
        /// <summary>
        /// Allow creation of instant invites
        /// </summary>
        CreateInstantInvite     = 0x00000001,
        /// <summary>
        /// Allows kicking members
        /// </summary>
        KickMembers             = 0x00000002,
        /// <summary>
        /// Allows banning members
        /// </summary>
        BanMembers              = 0x00000004,
        /// <summary>
        /// Allows all permissions and bypasses channel permission overwrites
        /// </summary>
        Administrator           = 0x00000008,
        /// <summary>
        /// Allows management and editing of channels
        /// </summary>
        ManageChannels          = 0x00000010,
        /// <summary>
        /// Allows management and editing of the guild
        /// </summary>
        ManageGuild             = 0x00000020,
        /// <summary>
        /// Allows for the addition of reactions to messages
        /// </summary>
        AddReactions            = 0x00000040,
        /// <summary>
        /// Allows reading messages in a channel. The channel will not appear for users without this permission
        /// </summary>
        ReadMessages            = 0x00000400,
        /// <summary>
        /// Allows for sending messages in a channel.
        /// </summary>
        SendMessages            = 0x00000800,
        /// <summary>
        /// Allows for sending of tts messages
        /// </summary>
        SendTTSMessages         = 0x00001000,
        /// <summary>
        /// Allows for deletion of other users messages
        /// </summary>
        ManageMessags           = 0x00002000,
        /// <summary>
        /// Links sent by this user will be auto-embedded
        /// </summary>
        EmbedLinks              = 0x00004000,
        /// <summary>
        /// Allows for uploading images and files
        /// </summary>
        AttachFiles             = 0x00008000,
        /// <summary>
        /// Allows for reading of message history
        /// </summary>
        ReadMessageHistory      = 0x00010000,
        /// <summary>
        /// Allows for using the @everyone tag to notify all users in a channel, and the @here tag to notify all online users in a channel
        /// </summary>
        MentionEveryone         = 0x00020000,
        /// <summary>
        /// Allows the usage of custom emojis from other servers
        /// </summary>
        UseExternalEmojis       = 0x00040000,
        /// <summary>
        /// Allows for joining of a voice channel
        /// </summary>
        Connect                 = 0x00100000,
        /// <summary>
        /// Allows for speaking in a voice channel
        /// </summary>
        Speak                   = 0x00200000,
        /// <summary>
        /// Allows for muting members in a voice channel
        /// </summary>
        MuteMembers             = 0x00400000,
        /// <summary>
        /// Allows for deafening of members in a voice channel
        /// </summary>
        DeafenMembers           = 0x00800000,
        /// <summary>
        /// Allows for moving of members between voice channels
        /// </summary>
        MoveMembers             = 0x01000000,
        /// <summary>
        /// Allows for using voice-activity-detection in a voice channel
        /// </summary>
        UseVAD                  = 0x02000000,
        /// <summary>
        /// Allows for modification of own nickname
        /// </summary>
        ChangeNickname          = 0x04000000,
        /// <summary>
        /// Allows for modification of other users nicknames
        /// </summary>
        ManageNicknames         = 0x08000000,
        /// <summary>
        /// Allows management and editing of roles
        /// </summary>
        ManageRoles             = 0x10000000,
        /// <summary>
        /// Allows management and editing of webhooks
        /// </summary>
        ManageWebhooks          = 0x20000000,
        /// <summary>
        /// Allows management and editing of emojis
        /// </summary>
        ManageEmojis            = 0x40000000
    }
}
