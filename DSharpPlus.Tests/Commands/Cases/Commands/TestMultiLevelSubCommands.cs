
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace DSharpPlus.Tests.Commands.Cases.Commands;
public class TestMultiLevelSubCommands
{
    [Command("info")]
    public class InfoCommand
    {
        [Command("user")]
        public class UserCommand
        {
            [Command("avatar")]
            public static ValueTask AvatarAsync(CommandContext context, DiscordUser user) => default;

            [Command("roles")]
            public static ValueTask RolesAsync(CommandContext context, DiscordUser user) => default;

            [Command("permissions")]
            public static ValueTask PermissionsAsync(CommandContext context, DiscordUser user, DiscordChannel? channel = null) => default;
        }

        [Command("channel")]
        public class ChannelCommand
        {
            [Command("created")]
            public static ValueTask PermissionsAsync(CommandContext context, DiscordChannel channel) => default;

            [Command("members")]
            public static ValueTask MembersAsync(CommandContext context, DiscordChannel channel) => default;
        }
    }
}
