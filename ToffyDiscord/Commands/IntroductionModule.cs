using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace ToffyDiscord.Commands;


public class IntroductionModule : BaseCommandModule
{
    [Command("greet")]
    public async Task GreetCommandAsync(CommandContext ctx) => await ctx.PromotionResponseAsync("Доброго вечора, ми з України!");
}
