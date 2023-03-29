using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DSharpPlus.Test;

[CannotUse]
public sealed class PermissionInheritanceCommands : BaseCommandModule
{
    [Group("borked")]
    public sealed class BrokenPermissionCommands : BaseCommandModule
    {
        [Command("command")]
        public async Task CommandAsync(CommandContext ctx)
            => await ctx.RespondAsync("it didn't work");
    }

    public sealed class NestedPermissionCommands : BaseCommandModule
    {
        [Command("nested")]
        public async Task NestedAsync(CommandContext ctx)
            => await ctx.RespondAsync("it also bork");
    }
}

public sealed class CannotUseAttribute : CheckBaseAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        => Task.FromResult(false);
}
