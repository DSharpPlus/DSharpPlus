using System.IO;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

public class RoleEditModel : BaseEditModel
{
    /// <summary>
    /// New role name
    /// </summary>
    public string Name { internal get; set; }
    /// <summary>
    /// New role permissions
    /// </summary>
    public DiscordPermissions? Permissions { internal get; set; }
    /// <summary>
    /// New role color
    /// </summary>
    public DiscordColor? Color { internal get; set; }
    /// <summary>
    /// Whether new role should be hoisted
    /// </summary>
    public bool? Hoist { internal get; set; } //tbh what is hoist
    /// <summary>
    /// Whether new role should be mentionable
    /// </summary>
    public bool? Mentionable { internal get; set; }

    /// <summary>
    /// The emoji to set for role role icon; must be unicode.
    /// </summary>
    public DiscordEmoji Emoji { internal get; set; }

    /// <summary>
    /// The stream to use for uploading a new role icon.
    /// </summary>
    public Stream Icon { internal get; set; }

    internal RoleEditModel()
    {
        Name = null;
        Permissions = null;
        Color = null;
        Hoist = null;
        Mentionable = null;
    }
}
