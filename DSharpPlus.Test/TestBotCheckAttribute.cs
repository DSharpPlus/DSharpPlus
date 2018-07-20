using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Test
{
    public class TestBotCheckAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            var srv = ctx.Services.GetService<TestBotScopedService>();
            return Task.FromResult(true);
        }
    }
}
