using System;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SlashCommandPermissionsAttribute : Attribute
{
    public DiscordPermission[] Permissions { get; }

    public SlashCommandPermissionsAttribute(params DiscordPermission[] permissions) => this.Permissions = permissions;
}
