using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace ToffyDiscord.Commands;

public class EntertainmentModule
{
    [Command("8ball")]
    [Description("Ask the magic 8 ball a question")]
    public async Task MagicBall(CommandContext ctx)
    {
        await ctx.TriggerTypingAsync();

        var random = new Random();
        int randNum = random.Next(1, 100);
        await ctx.RespondAsync($"{ctx.Member.Mention} Магический шар считает, что вероятность равна {randNum}%.");
    }
}
