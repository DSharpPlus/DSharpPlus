
using DSharpPlus.CommandsNext;
using ToffyDiscord.Commands;

namespace ToffyDiscord;

public static class CommandContextExtensions
{
    public static async Task PromotionResponseAsync(this CommandContext ctx, string message)
    {
        if (PromotionModule.Promotions.ContainsKey(ctx.Guild.Id))
        {
            var promotion = PromotionModule.Promotions[ctx.Guild.Id];
            if (promotion.IsEnabled)
            {

                await ctx.RespondAsync($"{message}\n\"{promotion.Text}\"");
                return;
            }
        }
        await ctx.RespondAsync(message);
    }
}
