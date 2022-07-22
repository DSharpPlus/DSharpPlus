using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
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
                Token = "MTAwMDA1MjA0MzY1NTk1NDU4Mg.GhNYxe.yck7Icxj5rphOpCl3xNVuejP6xjrxP8E-S7wTY",
                TokenType = TokenType.Bot
            });

            discord.MessageCreated += async (s, e) =>
            {
                if (!e.Author.IsBot)
                    if (e.Message.Content.ToLower().StartsWith("hello"))
                        await e.Message.RespondAsync($"Hello {e.Message.Author.Username}!");
            };
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration {StringPrefixes = new[] {"!"}});
            commands.RegisterCommands<IntroductionModule>();

            discord.GuildMemberAdded += DiscordOnGuildMemberAdded;

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static Task DiscordOnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
