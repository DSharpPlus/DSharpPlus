using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying a guild's membership screening.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class MembershipScreeningEditModel : BaseEditModel
{
    /// <summary>
    /// Sets whether membership screening should be enabled for this guild
    /// </summary>
    public Optional<bool> Enabled { get; set; }

    /// <summary>
    /// Sets the server description shown in the membership screening form
    /// </summary>
    public Optional<string> Description { get; set; }

    /// <summary>
    /// Sets the fields in this membership screening form
    /// </summary>
    public Optional<List<DiscordGuildMembershipScreeningField>> Fields { get; set; }

    internal MembershipScreeningEditModel() { }
}
