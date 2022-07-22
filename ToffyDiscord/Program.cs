using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using ToffyDiscord.Commands;

namespace ToffyDiscord
{
    class Program
    {
        static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "MTAwMDA1MjA0MzY1NTk1NDU4Mg.GDViR5.gOQCVgcnwmp5cpVz4xvFasbcgCI9vdh5JSdpqE",
                TokenType = TokenType.Bot
            });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                UseDefaultCommandHandler = false
            });


            var lavalink = discord.UseLavalink();



            commands.RegisterCommands<IntroductionModule>();
            commands.RegisterCommands<MusicModule>();
            commands.RegisterCommands<ModerationModule>();
            commands.SetHelpFormatter<DefaultHelpFormatter>();

            discord.MessageCreated += CommandHandler;

            discord.GuildMemberAdded += DiscordOnGuildMemberAdded;

            await discord.ConnectAsync();
            await lavalink.ConnectAsync(new LavalinkConfiguration{ }); // Make sure this is after Discord.ConnectAsync().
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
