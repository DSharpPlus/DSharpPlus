using System;
using DSharpPlus.CommandsNext.Attributes;

namespace DSharpPlus.CommandsNext
{
    [Flags]
    public enum Permission : ulong
    {
        [PermissionString("No permissions")]
        None = 0x0000000000000000u,

        [PermissionString("Create instant invites")]
        CreateInstantInvite = 0x0000000000000001u,

        [PermissionString("Kick members")]
        KickMembers = 0x0000000000000002u,

        [PermissionString("Ban members")]
        BanMembers = 0x0000000000000004u,

        [PermissionString("Administrator")]
        Administrator = 0x0000000000000008u,

        [PermissionString("Manage channels")]
        ManageChannels = 0x0000000000000010u,

        [PermissionString("Manage guild")]
        ManageGuild = 0x0000000000000020u,

        [PermissionString("Add reactions")]
        AddReactions = 0x0000000000000040u,

        [PermissionString("Read messages")]
        ReadMessages = 0x0000000000000400u,

        [PermissionString("Send messages")]
        SendMessages = 0x0000000000000800u,

        [PermissionString("Send TTS messages")]
        SendTtsMessages = 0x0000000000001000u,

        [PermissionString("Manage messages")]
        ManageMessages = 0x0000000000002000u,

        [PermissionString("Use embeds")]
        EmbedLinks = 0x0000000000004000u,

        [PermissionString("Attach files")]
        AttachFiles = 0x0000000000008000u,

        [PermissionString("Read message history")]
        ReadMessageHistory = 0x0000000000010000u,

        [PermissionString("Mention everyone")]
        MentionEveryone = 0x0000000000020000u,

        [PermissionString("Use external emojis")]
        UseExternalEmojis = 0x0000000000040000u,

        [PermissionString("Use voice chat")]
        UseVoice = 0x0000000000100000u,

        [PermissionString("Speak")]
        Speak = 0x0000000000200000u,

        [PermissionString("Mute voice chat members")]
        MuteMembers = 0x0000000000400000u,

        [PermissionString("Deafen voice chat members")]
        DeafenMembers = 0x0000000000800000u,

        [PermissionString("Move voice chat members")]
        MoveMembers = 0x0000000001000000u,

        [PermissionString("Use voice activity detection")]
        UseVoiceDetection = 0x0000000002000000u,

        [PermissionString("Change own nickname")]
        ChangeNickname = 0x0000000004000000u,

        [PermissionString("Manage nicknames")]
        ManageNicknames = 0x0000000008000000u,

        [PermissionString("Manage roles")]
        ManageRoles = 0x0000000010000000u,

        [PermissionString("Manage webhooks")]
        ManageWebhooks = 0x0000000020000000u,

        [PermissionString("Manage emoji")]
        ManageEmoji = 0x0000000040000000u
    }
}
