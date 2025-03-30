using System;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands;

/// <summary>
/// Specifies which permissions a command requires for the invoking user and the bot.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequirePermissionsAttribute : Attribute
{
    /// <summary>
    /// The permissions required of the bot in order for this command to be executed.
    /// </summary>
    public DiscordPermissions BotPermissions { get; init; }

    /// <summary>
    /// The permissions required of the invoking user in order for this command to be executed.
    /// </summary>
    public DiscordPermissions UserPermissions { get; init; }

    /// <summary>
    /// Creates a new instance of this attribute, requiring the specified permissions from both bot and user.
    /// </summary>
    public RequirePermissionsAttribute(params DiscordPermission[] permissions) 
        => this.BotPermissions = this.UserPermissions = new(permissions);

    /// <summary>
    /// Creates a new instance of this attribute.
    /// </summary>
    public RequirePermissionsAttribute(DiscordPermission[] botPermissions, DiscordPermission[] userPermissions)
    {
        this.BotPermissions = new(botPermissions);
        this.UserPermissions = new(userPermissions);
    }
}
