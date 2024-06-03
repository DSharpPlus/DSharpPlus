using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace DSharpPlus.Tests.Commands.CommandFiltering;

public class TestMultiLevelSubCommandsFiltered
{
    [Command("root")]
    public class RootCommand
    {
        [Command("subgroup")]
        public class GroupCommand
        {
            [Command("command-text-only-attribute"), AllowedProcessors(typeof(TextCommandProcessor))]
            public static ValueTask TextOnlyAsync(CommandContext context) => default;

            [Command("command-text-only-parameter")]
            public static ValueTask TextOnlyAsync2(TextCommandContext context) => default;

            [Command("command-slash-only-attribute"), AllowedProcessors(typeof(SlashCommandProcessor))]
            public static ValueTask SlashOnlyAsync(CommandContext context) => default;

            [Command("command-slash-only-parameter")]
            public static ValueTask SlashOnlyAsync2(SlashCommandContext context) => default;
        }

        [Command("subgroup-slash-only"), AllowedProcessors(typeof(SlashCommandProcessor))]
        public class SlashGroupCommand
        {
            [Command("slash-only-group")]
            public static ValueTask SlashOnlyAsync(CommandContext context, DiscordChannel channel) => default;

            [Command("slash-only-group2")]
            public static ValueTask SlashOnlyAsync2(SlashCommandContext context, DiscordChannel channel) => default;
        }
        
        [Command("subgroup-text-only"), AllowedProcessors(typeof(TextCommandProcessor))]
        public class TextGroupCommand
        {
            [Command("text-only-group")]
            public static ValueTask SlashOnlyAsync(CommandContext context, DiscordChannel channel) => default;

            [Command("text-only-group2")]
            public static ValueTask SlashOnlyAsync2(TextCommandContext context, DiscordChannel channel) => default;
        }
    }
    
    public class ContextMenues
    {
        [Command("UserContextOnly"), SlashCommandTypes(DiscordApplicationCommandType.UserContextMenu)]
        public static ValueTask UserCommand(SlashCommandContext context, DiscordUser user) => default;
        
        [Command("SlashUserContext"), SlashCommandTypes(DiscordApplicationCommandType.UserContextMenu, DiscordApplicationCommandType.SlashCommand)]
        public static ValueTask SlashUserCommand(SlashCommandContext context, DiscordUser user) => default;
        
        [Command("MessageContextOnly"), SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
        public static ValueTask MessageCommand(SlashCommandContext context, DiscordMessage message) => default;
            
        [Command("SlashMessageContext"), SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu, DiscordApplicationCommandType.SlashCommand)]
        public static ValueTask SlashMessageCommand(SlashCommandContext context, DiscordMessage message) => default;
    }
}
