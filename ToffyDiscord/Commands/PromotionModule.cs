using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace ToffyDiscord.Commands;

public class PromotionModule : BaseCommandModule
{
    public class Promotion
    {

        private string _text;
        public string Text
        {
            get => this._text;
            set
            {
                this._text = value;
                this.IsEnabled = true;
            }
        }

        public bool IsEnabled { get; set; }


        public Promotion(string text)
        {
            this.Text = text;
            this.IsEnabled = false;
        }


    }

    public static Dictionary<ulong, Promotion> Promotions;


    public PromotionModule()
    {
        Promotions = new Dictionary<ulong, Promotion>();
    }

    [Command]
    public async Task PromoteAsync(CommandContext ctx, [RemainingText] string? text = null)
    {
        var serverId = ctx.Guild.Id;

        if (!string.IsNullOrEmpty(text))
        {
            if (Promotions.ContainsKey(serverId))
            {
                Promotions[serverId].Text = text;
                await ctx.PromotionResponseAsync("Ви підключили промо");
            }
            else
            {
                Promotions.Add(serverId, new Promotion(text));
                Promotions[serverId].IsEnabled = true;
                await ctx.PromotionResponseAsync("Ви підключили промо");
            }
        }
        else
        {
            Promotions[serverId].IsEnabled = false;
            await ctx.PromotionResponseAsync("Ви відключили промо");
        }
    }
}
