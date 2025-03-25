using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Extensions;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Tools.Benchmarks;

public static class DiscordData
{
    private static readonly string applicationCommands = """[{"id":"1354205866907140146","application_id":"481314095723839508","version":"1354205866907140147","default_member_permissions":"2147483648","type":1,"name":"enum","description":"No description provided.","dm_permission":false,"contexts":null,"integration_types":[0,1],"options":[{"type":4,"name":"day","description":"No description provided.","required":true,"choices":[{"name":"Sunday","value":0},{"name":"Monday","value":1},{"name":"Tuesday","value":2},{"name":"Wednesday","value":3},{"name":"Thursday","value":4},{"name":"Friday","value":5},{"name":"Saturday","value":6}]}],"nsfw":false},{"id":"1354205868585127956","application_id":"481314095723839508","version":"1354205868585127957","default_member_permissions":"2147483648","type":1,"name":"five","description":"No description provided.","dm_permission":false,"contexts":null,"integration_types":[0,1],"options":[{"type":4,"name":"num1","description":"No description provided.","required":true},{"type":4,"name":"num2","description":"No description provided.","required":true},{"type":4,"name":"num3","description":"No description provided.","required":true},{"type":4,"name":"num4","description":"No description provided.","required":true},{"type":4,"name":"num5","description":"No description provided.","required":true}],"nsfw":false},{"id":"1354205870258655536","application_id":"481314095723839508","version":"1354205870258655537","default_member_permissions":"2147483648","type":1,"name":"four","description":"No description provided.","dm_permission":false,"contexts":null,"integration_types":[0,1],"options":[{"type":4,"name":"num1","description":"No description provided.","required":true},{"type":4,"name":"num2","description":"No description provided.","required":true},{"type":4,"name":"num3","description":"No description provided.","required":true},{"type":4,"name":"num4","description":"No description provided.","required":true}],"nsfw":false},{"id":"1354205871843971156","application_id":"481314095723839508","version":"1354205871843971157","default_member_permissions":"2147483648","type":1,"name":"varargs","description":"No description provided.","dm_permission":false,"contexts":null,"integration_types":[0,1],"options":[{"type":4,"name":"number_0","description":"No description provided.","required":true},{"type":4,"name":"number_1","description":"No description provided."},{"type":4,"name":"number_2","description":"No description provided."},{"type":4,"name":"number_3","description":"No description provided."},{"type":4,"name":"number_4","description":"No description provided."},{"type":4,"name":"number_5","description":"No description provided."},{"type":4,"name":"number_6","description":"No description provided."},{"type":4,"name":"number_7","description":"No description provided."},{"type":4,"name":"number_8","description":"No description provided."}],"nsfw":false},{"id":"1354205874947625121","application_id":"481314095723839508","version":"1354205874947625122","default_member_permissions":"2147483648","type":1,"name":"none","description":"No description provided.","dm_permission":false,"contexts":null,"integration_types":[0,1],"nsfw":false},{"id":"1354205954178285910","application_id":"481314095723839508","version":"1354205954178285911","default_member_permissions":"2147483648","type":1,"name":"one","description":"No description provided.","dm_permission":false,"contexts":null,"integration_types":[0,1],"options":[{"type":4,"name":"num","description":"No description provided.","required":true}],"nsfw":false},{"id":"1354205957462163526","application_id":"481314095723839508","version":"1354205957462163527","default_member_permissions":"2147483648","type":1,"name":"params","description":"No description provided.","dm_permission":false,"contexts":null,"integration_types":[0,1],"options":[{"type":4,"name":"number_0","description":"No description provided.","required":true},{"type":4,"name":"number_1","description":"No description provided."},{"type":4,"name":"number_2","description":"No description provided."},{"type":4,"name":"number_3","description":"No description provided."},{"type":4,"name":"number_4","description":"No description provided."},{"type":4,"name":"number_5","description":"No description provided."},{"type":4,"name":"number_6","description":"No description provided."},{"type":4,"name":"number_7","description":"No description provided."},{"type":4,"name":"number_8","description":"No description provided."},{"type":4,"name":"number_9","description":"No description provided."},{"type":4,"name":"number_10","description":"No description provided."},{"type":4,"name":"number_11","description":"No description provided."},{"type":4,"name":"number_12","description":"No description provided."},{"type":4,"name":"number_13","description":"No description provided."},{"type":4,"name":"number_14","description":"No description provided."},{"type":4,"name":"number_15","description":"No description provided."},{"type":4,"name":"number_16","description":"No description provided."},{"type":4,"name":"number_17","description":"No description provided."},{"type":4,"name":"number_18","description":"No description provided."},{"type":4,"name":"number_19","description":"No description provided."},{"type":4,"name":"number_20","description":"No description provided."},{"type":4,"name":"number_21","description":"No description provided."},{"type":4,"name":"number_22","description":"No description provided."},{"type":4,"name":"number_23","description":"No description provided."},{"type":4,"name":"number_24","description":"No description provided."}],"nsfw":false},{"id":"1354205959223906555","application_id":"481314095723839508","version":"1354205959223906556","default_member_permissions":"2147483648","type":1,"name":"six","description":"No description provided.","dm_permission":false,"contexts":null,"integration_types":[0,1],"options":[{"type":4,"name":"num1","description":"No description provided.","required":true},{"type":4,"name":"num2","description":"No description provided.","required":true},{"type":4,"name":"num3","description":"No description provided.","required":true},{"type":4,"name":"num4","description":"No description provided.","required":true},{"type":4,"name":"num5","description":"No description provided.","required":true},{"type":4,"name":"num6","description":"No description provided.","required":true}],"nsfw":false},{"id":"1354205960675131494","application_id":"481314095723839508","version":"1354205960675131495","default_member_permissions":"2147483648","type":1,"name":"three","description":"No description provided.","dm_permission":false,"contexts":null,"integration_types":[0,1],"options":[{"type":4,"name":"num1","description":"No description provided.","required":true},{"type":4,"name":"num2","description":"No description provided.","required":true},{"type":4,"name":"num3","description":"No description provided.","required":true}],"nsfw":false},{"id":"1354205963024076861","application_id":"481314095723839508","version":"1354205963024076862","default_member_permissions":"2147483648","type":1,"name":"two","description":"No description provided.","dm_permission":false,"contexts":null,"integration_types":[0,1],"options":[{"type":4,"name":"num1","description":"No description provided.","required":true},{"type":4,"name":"num2","description":"No description provided.","required":true}],"nsfw":false}]""";

    public static bool IsConnected => Client is not null && Client.AllShardsConnected;
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    public static DiscordClient Client { get; private set; } = null!;
    public static CommandsExtension CommandsExtension { get; private set; } = null!;
    public static TextCommandProcessor TextCommandProcessor { get; private set; } = null!;
    public static SlashCommandProcessor SlashCommandProcessor { get; private set; } = null!;
    public static DiscordGuild Guild { get; private set; } = null!;
    public static DiscordChannel Channel { get; private set; } = null!;
    public static DiscordUser User { get; private set; } = null!;
    public static DiscordMessage Message { get; private set; } = null!;

    public static Task SetupStaticVariablesAsync()
    {
        if (IsConnected)
        {
            return Task.CompletedTask;
        }

        ServiceCollection services = new();
        services.AddLogging(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Trace));
        services.AddDiscordClient(Environment.GetEnvironmentVariable("DISCORD_TOKEN")!, DiscordIntents.None);
        services.AddCommandsExtension((serviceProvider, extension) =>
        {
            TextCommandProcessor = new(new()
            {
                EnableCommandNotFoundException = false,
                IgnoreBots = false
            });

            SlashCommandProcessor = new SlashCommandProcessor(new()
            {
                RegisterCommands = false
            });

            extension.AddProcessors([TextCommandProcessor, SlashCommandProcessor]);
            extension.AddCommands(typeof(Program).Assembly);
            CommandsExtension = extension;
        }, new CommandsConfiguration()
        {
            RegisterDefaultCommandProcessors = false,
            UseDefaultCommandErrorHandler = false
        });

        ServiceProvider = services.BuildServiceProvider();
        Client = ServiceProvider.GetRequiredService<DiscordClient>();
        CommandsExtension = ServiceProvider.GetRequiredService<CommandsExtension>();
        return MakeApiRequestsAsync();
    }

    private static async Task MakeApiRequestsAsync()
    {
        try
        {
            await Client.ConnectAsync();
            Guild = await Client.GetGuildAsync(ulong.Parse(Environment.GetEnvironmentVariable("DISCORD_GUILD_ID")!));
            Channel = await Client.GetChannelAsync(ulong.Parse(Environment.GetEnvironmentVariable("DISCORD_CHANNEL_ID")!));
            Message = await Channel.GetMessageAsync(ulong.Parse(Environment.GetEnvironmentVariable("DISCORD_MESSAGE_ID")!));
            User = Client.CurrentUser;
            SlashCommandProcessor.AddApplicationCommands(DiscordJson.ToDiscordObject<DiscordApplicationCommand[]>(JToken.Parse(applicationCommands)));
            await CommandsExtension.RefreshAsync();
        }
        catch (DiscordException)
        {
            await Task.Delay(10000);
        }
    }
}
