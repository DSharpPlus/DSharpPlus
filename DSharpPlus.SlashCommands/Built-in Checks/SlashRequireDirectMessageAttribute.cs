
using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Attributes;
/// <summary>
/// Defines that this slash command is only usable within a direct message channel.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SlashRequireDirectMessageAttribute : SlashCheckBaseAttribute
{
    /// <summary>
    /// Defines that this command is only usable within a direct message channel.
    /// </summary>
    public SlashRequireDirectMessageAttribute() { }

    /// <summary>
    /// Runs checks.
    /// </summary>
    public override Task<bool> ExecuteChecksAsync(InteractionContext ctx) => Task.FromResult(ctx.Channel is DiscordDmChannel);
}
