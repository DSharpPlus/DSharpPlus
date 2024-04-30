namespace DSharpPlus.CommandsNext.Attributes;

using System.Threading.Tasks;

/// <summary>
/// Defines that a command is only usable within a guild.
/// </summary>
public sealed class RequireGuildAttribute : CheckBaseAttribute
{
    /// <summary>
    /// Defines that this command is only usable within a guild.
    /// </summary>
    public RequireGuildAttribute()
    { }

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        => Task.FromResult(ctx.Guild != null);
}
