using System;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SlashCommandPermissionsAttribute : Attribute
{
    public DiscordPermissions Permissions { get; }

    public SlashCommandPermissionsAttribute(DiscordPermissions permissions) => Permissions = permissions;
}
