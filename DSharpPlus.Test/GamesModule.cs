using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Test
{
    public class GamesModule : BaseCommandModule
    {
        public IReadOnlyList<string> Cache { get; } = new List<string>
        {
            "Ovelha",
            "Bloco",
            "Som",
            "Chuva",
            "Cadeira",
            "Caixa",
            "Fogão",
            "Frio",
            "Calor",
            "Tapete",
            "Grama",
            "Servidor",
            "Bot",
            "Discord",
            "Minecraft",
            "Overwatch"
        };

        public Random Rnd { get; } = new Random();

        [Command]
        public async Task Hangman(CommandContext ctx)
        {
            var word = this.Cache[this.Rnd.Next(0, this.Cache.Count)];
            await HangmanGame.StartAsync(ctx, word);
        }
    }
}
