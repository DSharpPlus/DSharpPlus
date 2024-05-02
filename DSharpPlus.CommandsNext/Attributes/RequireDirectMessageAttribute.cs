
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Attributes;
/// <summary>
/// Defines that a command is only usable within a direct message channel.
/// </summary>
public sealed class RequireDirectMessageAttribute : CheckBaseAttribute
{
    /// <summary>
    /// Defines that this command is only usable within a direct message channel.
    /// </summary>
    public RequireDirectMessageAttribute()
    { }

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        => Task.FromResult(ctx.Channel is DiscordDmChannel);
}
