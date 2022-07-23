using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;
using ToffyDiscord.App;
// using ToffyDiscord.App;
using ToffyDiscord.Commands;

namespace ToffyDiscord
{
    internal class Program
    {
        private static void Main(string[] args) => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            var token = Startup.BotToken;
             var hostname = Startup.Host;
             var port = Startup.Port;

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot
            });
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration());

            commands.RegisterCommands<IntroductionModule>();
            commands.RegisterCommands<MusicModule>();
            commands.RegisterCommands<EntertainmentModule>();
            commands.RegisterCommands<ModerationModule>();
            commands.SetHelpFormatter<DefaultHelpFormatter>();


            var endpoint = new ConnectionEndpoint {Hostname = hostname, Port = port};

            var lavalinkConfig = new LavalinkConfiguration
            {
                RestEndpoint = endpoint, SocketEndpoint = endpoint, Password = "youshallnotpass"
            };

            discord.MessageCreated += async (s, e) =>
            {
                var badWords = await File.ReadAllLinesAsync("../../../BadWords.txt");
                foreach (var badWord in badWords)
                {
                    if (e.Message.Content.Contains(badWord))
                    {
                        await e.Message.RespondAsync($"{e.Message.Author.Mention} Це погане слово. ВИДАЛЯЮ!");
                        await e.Message.DeleteAsync();
                    }
                }
            };



            var lavalink = discord.UseLavalink();

            await discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);


            discord.MessageCreated += CommandHandleModule.Handle;

        }
    }
}
