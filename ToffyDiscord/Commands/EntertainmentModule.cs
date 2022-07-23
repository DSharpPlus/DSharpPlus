using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace ToffyDiscord.Commands;

public class EntertainmentModule : BaseCommandModule
{
    [Command("8ball")]
    [Description("Запитай в шарика відповідь на твоє питання")]
    public async Task MagicBall(CommandContext ctx)
    {
        await ctx.TriggerTypingAsync();

        var random = new Random();
        int randNum = random.Next(1, 100);
        await ctx.PromotionResponseAsync($"{ctx.Member.Mention} Магічний шарик вважає, що ймовірність {randNum}%.");
    }

    [Command("random")]
    [Description("Покаже випадкове число в будь-якому діапазоні")]
    public async Task Random(CommandContext ctx, [Description("Нижня границя діапозона")] int down=1, [Description("Верхня границя діапозона")] int up=100)
    {
        await ctx.TriggerTypingAsync();

        Random random = new Random();
        int randomNum;
        randomNum = random.Next(down, up);

        await ctx.PromotionResponseAsync($"{ctx.Member.Mention} Твоє число - {randomNum}");
    }
}
