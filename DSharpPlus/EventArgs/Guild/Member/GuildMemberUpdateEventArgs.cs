using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildMemberUpdated"/> event.
/// </summary>
public class GuildMemberUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild in which the update occurred.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Get the member with post-update info
    /// </summary>
    public DiscordMember MemberAfter { get; internal set; }

    /// <summary>
    /// Get the member with pre-update info
    /// </summary>
    public DiscordMember MemberBefore { get; internal set; }

    /// <summary>
    /// Gets a collection containing post-update roles.
    /// </summary>
    public IReadOnlyList<DiscordRole> RolesAfter => new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(MemberAfter.Roles));

    /// <summary>
    /// Gets a collection containing pre-update roles.
    /// </summary>
    public IReadOnlyList<DiscordRole> RolesBefore => new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(MemberBefore.Roles));

    /// <summary>
    /// Gets the member's new nickname.
    /// </summary>
    public string NicknameAfter => MemberAfter.Nickname;

    /// <summary>
    /// Gets the member's old nickname.
    /// </summary>
    public string NicknameBefore => MemberBefore.Nickname;

    /// <summary>
    /// Gets the member's old guild avatar hash.
    /// </summary>
    public string GuildAvatarHashBefore => MemberBefore.GuildAvatarHash;

    /// <summary>
    /// Gets the member's new guild avatar hash.
    /// </summary>
    public string GuildAvatarHashAfter => MemberAfter.GuildAvatarHash;

    /// <summary>
    /// Gets the member's old username.
    /// </summary>
    public string UsernameBefore => MemberBefore.Username;

    /// <summary>
    /// Gets the member's new username.
    /// </summary>
    public string UsernameAfter => MemberAfter.Username;

    /// <summary>
    /// Gets the member's old avatar hash.
    /// </summary>
    public string AvatarHashBefore => MemberBefore.AvatarHash;

    /// <summary>
    /// Gets the member's new avatar hash.
    /// </summary>
    public string AvatarHashAfter => MemberAfter.AvatarHash;

    /// <summary>
    /// Gets whether the member had passed membership screening before the update
    /// </summary>
    public bool? PendingBefore => MemberBefore.IsPending;

    /// <summary>
    /// Gets whether the member had passed membership screening after the update
    /// </summary>
    public bool? PendingAfter => MemberAfter.IsPending;

    /// <summary>
    /// Gets the member's communication restriction before the update
    /// </summary>
    public DateTimeOffset? CommunicationDisabledUntilBefore => MemberBefore.CommunicationDisabledUntil;

    /// <summary>
    /// Gets the member's communication restriction after the update
    /// </summary>
    public DateTimeOffset? CommunicationDisabledUntilAfter => MemberAfter.CommunicationDisabledUntil;

    /// <summary>
    /// Gets the member that was updated.
    /// </summary>
    public DiscordMember Member => MemberAfter;

    internal GuildMemberUpdateEventArgs() : base() { }
}
