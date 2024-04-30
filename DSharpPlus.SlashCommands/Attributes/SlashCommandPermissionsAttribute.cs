namespace DSharpPlus.SlashCommands;
using System;

using DSharpPlus.Entities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SlashCommandPermissionsAttribute : Attribute
{
    public DiscordPermissions Permissions { get; }

    public SlashCommandPermissionsAttribute(DiscordPermissions permissions) => Permissions = permissions;
}
