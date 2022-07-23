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

    [Command("random")]
    [Description("Выводит случайное число из заданного диапозона")]
    public async Task Random(CommandContext ctx, [Description("Нижняя граница диапозона")] int down=1, [Description("Верхняя граница диапозона")] int up=100)
    {
        await ctx.TriggerTypingAsync();

        Random random = new Random();
        int randomNum;
        randomNum = random.Next(down, up);

        await ctx.RespondAsync($"{ctx.Member.Mention} Твоё число - {randomNum}");
    }
}
