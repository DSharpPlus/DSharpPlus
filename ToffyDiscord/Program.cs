using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;
using ToffyDiscord.Commands;

namespace ToffyDiscord
{
    class Program
    {
        static void Main(string[] args) => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "MTAwMDA1MjA0MzY1NTk1NDU4Mg.GU9s8U.AOp3rKGCUTScAqr9CApn6VfVnhmYfEgoZJbdm0",
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Trace
            });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration { });

            commands.RegisterCommands<IntroductionModule>();
            commands.RegisterCommands<MusicModule>();
            commands.SetHelpFormatter<DefaultHelpFormatter>();

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1", // From your server configuration.
                Port = 2333, // From your server configuration
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                RestEndpoint = endpoint, SocketEndpoint = endpoint, Password = "test"
            };


            var lavalink = discord.UseLavalink();

            await discord.ConnectAsync();

            var lavalinkNode = await lavalink.ConnectAsync(lavalinkConfig);


            discord.MessageCreated += CommandHandler;

            discord.GuildMemberAdded += DiscordOnGuildMemberAdded;


            await Task.Delay(-1);
        }

        private static Task DiscordOnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            return Task.CompletedTask;
        }


        private static Task CommandHandler(DiscordClient client, MessageCreateEventArgs e)
        {
            var nextCommand = client.GetCommandsNext();
            var msg = e.Message;

            var cmdStart = msg.GetStringPrefixLength("!");
            if (cmdStart == -1) return Task.CompletedTask;

            var prefix = msg.Content[..cmdStart];
            var cmdString = msg.Content[cmdStart..];

            var command = nextCommand.FindCommand(cmdString, out var args);
            if (command == null) return Task.CompletedTask;

            var ctx = nextCommand.CreateContext(msg, prefix, command, args);
            Task.Run(async () => await nextCommand.ExecuteCommandAsync(ctx));

            return Task.CompletedTask;
        }
    }
}
