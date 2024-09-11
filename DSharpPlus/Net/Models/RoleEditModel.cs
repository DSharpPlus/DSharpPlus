using System.IO;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying a role.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class RoleEditModel : BaseEditModel
{
    /// <summary>
    /// New role name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// New role permissions
    /// </summary>
    public DiscordPermissions? Permissions { get; set; }

    /// <summary>
    /// New role color
    /// </summary>
    public DiscordColor? Color { get; set; }

    /// <summary>
    /// Whether new role should be hoisted
    /// </summary>
    public bool? Hoist { get; set; } //tbh what is hoist

    /// <summary>
    /// Whether new role should be mentionable
    /// </summary>
    public bool? Mentionable { get; set; }

    /// <summary>
    /// The emoji to set for role role icon; must be unicode.
    /// </summary>
    public DiscordEmoji Emoji { get; set; }

    /// <summary>
    /// The stream to use for uploading a new role icon.
    /// </summary>
    public Stream Icon { get; set; }

    internal RoleEditModel() { }
}
