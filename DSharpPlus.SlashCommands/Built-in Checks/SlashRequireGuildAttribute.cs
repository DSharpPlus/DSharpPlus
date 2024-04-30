namespace DSharpPlus.SlashCommands.Attributes;
using System;
using System.Threading.Tasks;

/// <summary>
/// Defines that this slash command is only usable within a guild.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SlashRequireGuildAttribute : SlashCheckBaseAttribute
{
    /// <summary>
    /// Defines that this command is only usable within a guild.
    /// </summary>
    public SlashRequireGuildAttribute() { }

    /// <summary>
    /// Runs checks.
    /// </summary>
    public override Task<bool> ExecuteChecksAsync(InteractionContext ctx) => Task.FromResult(ctx.Guild != null);
}
