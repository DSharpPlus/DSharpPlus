using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Test
{
    public class HangmanGame
    {
        private string _id;
        private InteractivityExtension _interactivity;
        private CommandContext ctx;
        private string _word;
        private char[] _word_temp;
        private int _index;

        public static Task StartAsync(CommandContext context, string word)
            => new HangmanGame(context, word).StartAsync();

        HangmanGame(CommandContext context, string word)
        {
            _id = $"{new Random().Next(10000, 99999):x2}";
            ctx = context;
            _interactivity = context.Client.GetInteractivity();

            _word = word;
            _word_temp = new char[word.Length];

            for (var i = 0; i < word.Length; i++)
            {
                if (char.IsWhiteSpace(word[i]))
                    _word_temp[i] = ' ';
                else
                    _word_temp[i] = '_';
            }

            _index = 1;
        }

        public async Task StartAsync()
        {
            if (_interactivity == null)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Módulo de interatividade não encontrado.");
                return;
            }

            // sample embed
            var deb = new DiscordEmbedBuilder()
                .WithDescription(Formatter.Sanitize(new string(_word_temp)) + ": [" + (_index + 1) + "/7]")
                .WithAuthor("SkyLar: Hangman", iconUrl: ctx.Client.CurrentUser.GetAvatarUrl(ImageFormat.Png))
                .WithFooter($"Jogando com {ctx.User.Username}#{ctx.User.Discriminator} | {_id}", ctx.User.GetAvatarUrl(ImageFormat.Jpeg))
                .WithColor(DiscordColor.Orange);

            // initial message
            var msg = await ctx.RespondAsync(embed: deb);

        game:
            {
                // wait for user input
                var xmsg = await _interactivity.WaitForMessageAsync(xm => xm.Author == ctx.User, TimeSpan.FromMinutes(1));

                // is timeout? declare game ended
                if (xmsg.TimedOut)
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} :x: Tempo limite de jogo excedido.");
                    return;
                }

                // check if is valid content.
                if (string.IsNullOrEmpty(xmsg.Result.Content))
                {
                    var xerr = await ctx.RespondAsync($"{ctx.User.Mention} :x: Você precisa fornecer uma letra.");
                    await Task.Delay(3500);
                    await xerr.DeleteAsync();

                    // go back to game, if message content isn't valid, just "warn" to user.
                    goto game;
                }

                // used to store if user match letter in word. 
                var _state = false;

                // current user input letter 
                var _letter = xmsg.Result.Content.First();

                for (var i = 0; i < _word.Length; i++)
                {
                    // match for insensitive case char
                    if (_letter == _word[i] || char.ToUpperInvariant(_letter) == _word[i] || char.ToLowerInvariant(_letter) == _word[i])
                    {
                        _word_temp[i] = _word[i];
                        _state = true;
                    }
                }

                try
                {
                    // delete user input
                    await xmsg.Result.DeleteAsync();
                }
                catch { /* prevent exception thrown if bot has no permission to manage messages */ }

                // check if final word is current built word
                if (new string(_word_temp) == _word)
                {
                    await msg.DeleteAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} :tada: Parabéns... Você venceu!"); // declare victory.
                    return; // stop "game" label.
                }

                // no tries :( user lost.
                if (_index > 7)
                {
                    await msg.DeleteAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} :cry: Infelizmente você não acertou! A palavra era `{Formatter.Sanitize(_word)}`");
                    return; // stop "game" label.
                }

                // increment tries if has no letter match in final word
                if (!_state)
                    _index++;

                // update embed with matched letters and tries count.
                deb.WithDescription(Formatter.Sanitize(new string(_word_temp)) + ": [" + (_index) + "/7]");
                //await msg.ModifyAsync(embed: deb.Build());

                // go back to next "round"
                goto game;
            }
        }
    }
}