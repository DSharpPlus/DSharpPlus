using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Tools.Benchmarks.Cases.Commands.Converters;

public class EnumConverterBenchmarks
{
    private static readonly EnumConverter converter = new();
    private static readonly EnumConverter<DayOfWeek> genericConverter = new();

    [GlobalSetup]
    public void Setup() => DiscordData.SetupStaticVariablesAsync().GetAwaiter().GetResult();

    [Benchmark, ArgumentsSource(nameof(GetSuccessfulConverterObject))]
    public async ValueTask ConvertEnumAsync(ConverterContext context) => await converter.ConvertAsync(context);

    [Benchmark, ArgumentsSource(nameof(GetSuccessfulConverterObject))]
    public async ValueTask ConvertGenericEnumAsync(ConverterContext context) => await genericConverter.ConvertAsync(context);

    [Benchmark, ArgumentsSource(nameof(GetFailedConverterObject))]
    public async ValueTask ConvertEnumFailAsync(ConverterContext context) => await converter.ConvertAsync(context);

    [Benchmark, ArgumentsSource(nameof(GetFailedConverterObject))]
    public async ValueTask ConvertGenericEnumFailAsync(ConverterContext context) => await genericConverter.ConvertAsync(context);

    [Benchmark, ArgumentsSource(nameof(GetSuccessfulIntConverterObject))]
    public async ValueTask ConvertIntAsync(ConverterContext context) => await converter.ConvertAsync(context);

    [Benchmark, ArgumentsSource(nameof(GetSuccessfulIntConverterObject))]
    public async ValueTask ConvertGenericIntAsync(ConverterContext context) => await genericConverter.ConvertAsync(context);

    [Benchmark, ArgumentsSource(nameof(GetFailedIntConverterObject))]
    public async ValueTask ConvertIntFailAsync(ConverterContext context) => await converter.ConvertAsync(context);

    [Benchmark, ArgumentsSource(nameof(GetFailedIntConverterObject))]
    public async ValueTask ConvertGenericIntFailAsync(ConverterContext context) => await genericConverter.ConvertAsync(context);

    public IEnumerable<object> GetSuccessfulConverterObject()
    {
        Setup();

        TextConverterContext converterContext = new()
        {
            Channel = DiscordData.Channel,
            Command = DiscordData.CommandsExtension.Commands["enum"],
            Extension = DiscordData.CommandsExtension,
            Message = DiscordData.Message,
            RawArguments = "Monday",
            ServiceScope = DiscordData.ServiceProvider.CreateScope(),
            Splicer = DefaultTextArgumentSplicer.Splice,
            PrefixLength = 0,
            User = DiscordData.User
        };

        converterContext.NextParameter();
        converterContext.NextArgument();
        yield return converterContext;
    }

    public IEnumerable<object> GetFailedConverterObject()
    {
        Setup();

        TextConverterContext converterContext = new()
        {
            Channel = DiscordData.Channel,
            Command = DiscordData.CommandsExtension.Commands["enum"],
            Extension = DiscordData.CommandsExtension,
            Message = DiscordData.Message,
            RawArguments = "NoneDay",
            ServiceScope = DiscordData.ServiceProvider.CreateScope(),
            Splicer = DefaultTextArgumentSplicer.Splice,
            PrefixLength = 0,
            User = DiscordData.User
        };

        converterContext.NextParameter();
        converterContext.NextArgument();
        yield return converterContext;
    }

    public IEnumerable<object> GetSuccessfulIntConverterObject()
    {
        Setup();

        InteractionConverterContext converterContext = new()
        {
            Channel = DiscordData.Channel,
            Command = DiscordData.CommandsExtension.Commands["enum"],
            Extension = DiscordData.CommandsExtension,
            Interaction = DiscordJson.ToDiscordObject<DiscordInteraction>(JToken.Parse("""{"app_permissions":"2222085723512400","application_id":"1255985704232554496","authorizing_integration_owners":{"0":"1070516376046944286"},"channel":{"flags":0,"guild_id":"1070516376046944286","icon_emoji":{"id":null,"name":"🤖"},"id":"1076966571907481661","last_message_id":"1290818288351182910","name":"bot-usage","nsfw":false,"parent_id":null,"permissions":"2251799813685247","position":5,"rate_limit_per_user":0,"theme_color":null,"topic":null,"type":0},"channel_id":"1076966571907481661","context":0,"data":{"id":"1280184452382855340","name":"enum","options":[{"name":"day","type":4,"value":1}],"type":1},"entitlement_sku_ids":[],"entitlements":[],"guild":{"features":["NEWS","PREVIEW_ENABLED","MEMBER_VERIFICATION_GATE_ENABLED","SOUNDBOARD","WELCOME_SCREEN_ENABLED","COMMUNITY","ACTIVITY_FEED_DISABLED_BY_USER","CHANNEL_ICON_EMOJIS_GENERATED"],"id":"1070516376046944286","locale":"en-US"},"guild_id":"1070516376046944286","guild_locale":"en-US","id":"1290818422656995428","locale":"en-US","member":{"avatar":null,"banner":null,"communication_disabled_until":null,"deaf":false,"flags":0,"joined_at":"2023-02-02T01:34:40.083000+00:00","mute":false,"nick":null,"pending":false,"permissions":"2251799813685247","premium_since":null,"roles":["1070520199775780894","1070518892243464255"],"unusual_dm_activity_until":null,"user":{"avatar":"cb52688afd66f14e8a433396cd84c7c7","avatar_decoration_data":null,"clan":{"badge":"f09dccb8074d3c1a3bccadff9ceee10b","identity_enabled":true,"identity_guild_id":"832354798153236510","tag":"Moon"},"discriminator":"0","global_name":"Lunar","id":"336733686529654798","public_flags":4194560,"username":"oolunar"}},"token":"aW50ZXJhY3Rpb246MTI5MDgxODQyMjY1Njk5NTQyODo4ZEJxY0pjUHZNclpjQXI0MnJEVVFmekJpcjJrdUFJUGowNml1Sk5iRkJKRUtCeEluRkFnUkhNa29OWmszYzJQQlpiUThjNW1TVEtzQXJKMkpUUWc0Nk1ZNlUzaFNiMGNMMDFhNTF5U0RBRldOdkF1S05PT1V5Vlp2SnpkblFaVg","type":2,"version":1}""")),
            Options = DiscordJson.ToDiscordObject<IReadOnlyList<DiscordInteractionDataOption>>(JToken.Parse("""[{"name":"day","type":4,"value":1}]""")),
            ParameterNamePolicy = new SnakeCaseNamingPolicy(),
            ServiceScope = DiscordData.ServiceProvider.CreateScope(),
            User = DiscordData.User
        };

        converterContext.NextParameter();
        converterContext.NextArgument();
        yield return converterContext;
    }

    public IEnumerable<object> GetFailedIntConverterObject()
    {
        Setup();

        InteractionConverterContext converterContext = new()
        {
            Channel = DiscordData.Channel,
            Command = DiscordData.CommandsExtension.Commands["enum"],
            Extension = DiscordData.CommandsExtension,
            Interaction = DiscordJson.ToDiscordObject<DiscordInteraction>(JToken.Parse("""{"app_permissions":"2222085723512400","application_id":"1255985704232554496","authorizing_integration_owners":{"0":"1070516376046944286"},"channel":{"flags":0,"guild_id":"1070516376046944286","icon_emoji":{"id":null,"name":"🤖"},"id":"1076966571907481661","last_message_id":"1290818288351182910","name":"bot-usage","nsfw":false,"parent_id":null,"permissions":"2251799813685247","position":5,"rate_limit_per_user":0,"theme_color":null,"topic":null,"type":0},"channel_id":"1076966571907481661","context":0,"data":{"id":"1280184452382855340","name":"enum","options":[{"name":"day","type":4,"value":10}],"type":1},"entitlement_sku_ids":[],"entitlements":[],"guild":{"features":["NEWS","PREVIEW_ENABLED","MEMBER_VERIFICATION_GATE_ENABLED","SOUNDBOARD","WELCOME_SCREEN_ENABLED","COMMUNITY","ACTIVITY_FEED_DISABLED_BY_USER","CHANNEL_ICON_EMOJIS_GENERATED"],"id":"1070516376046944286","locale":"en-US"},"guild_id":"1070516376046944286","guild_locale":"en-US","id":"1290818422656995428","locale":"en-US","member":{"avatar":null,"banner":null,"communication_disabled_until":null,"deaf":false,"flags":0,"joined_at":"2023-02-02T01:34:40.083000+00:00","mute":false,"nick":null,"pending":false,"permissions":"2251799813685247","premium_since":null,"roles":["1070520199775780894","1070518892243464255"],"unusual_dm_activity_until":null,"user":{"avatar":"cb52688afd66f14e8a433396cd84c7c7","avatar_decoration_data":null,"clan":{"badge":"f09dccb8074d3c1a3bccadff9ceee10b","identity_enabled":true,"identity_guild_id":"832354798153236510","tag":"Moon"},"discriminator":"0","global_name":"Lunar","id":"336733686529654798","public_flags":4194560,"username":"oolunar"}},"token":"aW50ZXJhY3Rpb246MTI5MDgxODQyMjY1Njk5NTQyODo4ZEJxY0pjUHZNclpjQXI0MnJEVVFmekJpcjJrdUFJUGowNml1Sk5iRkJKRUtCeEluRkFnUkhNa29OWmszYzJQQlpiUThjNW1TVEtzQXJKMkpUUWc0Nk1ZNlUzaFNiMGNMMDFhNTF5U0RBRldOdkF1S05PT1V5Vlp2SnpkblFaVg","type":2,"version":1}""")),
            Options = DiscordJson.ToDiscordObject<IReadOnlyList<DiscordInteractionDataOption>>(JToken.Parse("""[{"name":"day","type":4,"value":10}]""")),
            ParameterNamePolicy = new SnakeCaseNamingPolicy(),
            ServiceScope = DiscordData.ServiceProvider.CreateScope(),
            User = DiscordData.User
        };

        converterContext.NextParameter();
        converterContext.NextArgument();
        yield return converterContext;
    }
}
