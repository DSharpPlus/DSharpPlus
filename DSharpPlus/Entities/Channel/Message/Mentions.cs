
using System.Collections.Generic;

namespace DSharpPlus.Entities;
/// <summary>
/// Interface for mentionables
/// </summary>
public interface IMention { }

/// <summary>
/// Allows a reply to ping the user being replied to.
/// </summary>
public readonly struct RepliedUserMention : IMention
{
    //This is pointless because new RepliedUserMention() will work, but it is here for consistency with the other mentionables.
    /// <summary>
    /// Mention the user being replied to.  Alias to <see cref="RepliedUserMention()"/> constructor.
    /// </summary>
    public static readonly RepliedUserMention All = new();
}

/// <summary>
/// Allows @everyone and @here pings to mention in the message.
/// </summary>
public readonly struct EveryoneMention : IMention
{
    //This is pointless because new EveryoneMention() will work, but it is here for consistency with the other mentionables.
    /// <summary>
    /// Allow the mentioning of @everyone and @here. Alias to <see cref="EveryoneMention()"/> constructor.
    /// </summary>
    public static readonly EveryoneMention All = new();
}

/// <summary>
/// Allows @user pings to mention in the message.
/// </summary>
/// <remarks>
/// Allows the specific user to be mentioned
/// </remarks>
/// <param name="id"></param>
public readonly struct UserMention(ulong id) : IMention
{
    /// <summary>
    /// Allow mentioning of all users. Alias to <see cref="UserMention()"/> constructor.
    /// </summary>
    public static readonly UserMention All = new();

    /// <summary>
    /// Optional Id of the user that is allowed to be mentioned. If null, then all user mentions will be allowed.
    /// </summary>
    public ulong? Id { get; } = id;

    /// <summary>
    /// Allows the specific user to be mentioned
    /// </summary>
    /// <param name="user"></param>
    public UserMention(DiscordUser user) : this(user.Id) { }

    public static implicit operator UserMention(DiscordUser user) => new(user.Id);
}

/// <summary>
/// Allows @role pings to mention in the message.
/// </summary>
/// <remarks>
/// Allows the specific id to be mentioned
/// </remarks>
/// <param name="id"></param>
public readonly struct RoleMention(ulong id) : IMention
{
    /// <summary>
    /// Allow the mentioning of all roles.  Alias to <see cref="RoleMention()"/> constructor.
    /// </summary>
    public static readonly RoleMention All = new();

    /// <summary>
    /// Optional Id of the role that is allowed to be mentioned. If null, then all role mentions will be allowed.
    /// </summary>
    public ulong? Id { get; } = id;

    /// <summary>
    /// Allows the specific role to be mentioned
    /// </summary>
    /// <param name="role"></param>
    public RoleMention(DiscordRole role) : this(role.Id) { }

    public static implicit operator RoleMention(DiscordRole role) => new(role.Id);
}

/// <summary>
/// Contains static instances of common mention patterns.
/// </summary>
public static class Mentions
{
    /// <summary>
    /// All possible mentions - @everyone + @here, users, and roles.
    /// </summary>
    public static IEnumerable<IMention> All { get; } = [EveryoneMention.All, UserMention.All, RoleMention.All];

    /// <summary>
    /// No mentions allowed.
    /// </summary>
    public static IEnumerable<IMention> None { get; } = [];
}
