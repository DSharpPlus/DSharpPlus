using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying a member.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class MemberEditModel : BaseEditModel
{
    /// <summary>
    /// New nickname
    /// </summary>
    public Optional<string> Nickname { get; set; }

    /// <summary>
    /// New roles
    /// </summary>
    public Optional<List<DiscordRole>> Roles { get; set; }

    /// <summary>
    /// Whether this user should be muted in voice channels
    /// </summary>
    public Optional<bool> Muted { get; set; }

    /// <summary>
    /// Whether this user should be deafened
    /// </summary>
    public Optional<bool> Deafened { get; set; }

    /// <summary>
    /// Voice channel to move this user to, set to null to kick
    /// </summary>
    public Optional<DiscordChannel> VoiceChannel { get; set; }

    /// <summary>
    /// Whether this member should have communication restricted
    /// </summary>
    public Optional<DateTimeOffset?> CommunicationDisabledUntil { get; set; }

    internal MemberEditModel() { }
}
