using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DSharpPlus.Test.Tests
{
    public class DiscordRoleBuilderTests: BaseCommandModule
    {
        [Command]
        public async Task CreateRole(CommandContext ctx, [RemainingText] string name)
        {
            await new DiscordRoleCreateBuilder()
                .WithName(name)
                .WithColor(DiscordColor.Blurple)
                .WithHoist(true)
                .WithMentionable(true)
                .WithPermissions(Permissions.AddReactions)
                .CreateAsync(ctx.Guild);
        }

        [Command]
        public async Task CreateRoleAction(CommandContext ctx, [RemainingText] string name)
        {
            await ctx.Guild.CreateRoleAsync(x =>
            {
                x.WithName(name)
                .WithColor(DiscordColor.Blurple)
                .WithHoist(true)
                .WithMentionable(true)
                .WithPermissions(Permissions.AddReactions);
            });                
        }

        [Command]
        public async Task ModifyRole(CommandContext ctx, DiscordRole role, [RemainingText] string name)
        {
            await new DiscordRoleModifyBuilder()
                .WithName(name)
                .ModifyAsync(role);
        }

        [Command]
        public async Task ModifyRoleAction(CommandContext ctx, DiscordRole role, [RemainingText] string name)
        {
            await role.ModifyAsync(x => {
                x.WithName(name);
            });
        }
    }
}
