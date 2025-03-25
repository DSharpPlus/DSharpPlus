using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using DSharpPlus.Net.InboundWebhooks.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Tools.Benchmarks.Cases;

public class InteractionCommandExecutionBenchmarks
{
    private static readonly byte[] interactionPayload = """{"app_permissions":"2222085723512400","application_id":"1255985704232554496","authorizing_integration_owners":{"0":"1070516376046944286"},"channel":{"flags":0,"guild_id":"1070516376046944286","icon_emoji":{"id":null,"name":"🤖"},"id":"1076966571907481661","last_message_id":"1354209592950915135","name":"bot-usage","nsfw":false,"parent_id":null,"permissions":"2251799813685247","position":6,"rate_limit_per_user":0,"theme_color":null,"topic":null,"type":0},"channel_id":"1076966571907481661","context":0,"data":{"id":"1354205866907140146","name":"enum","options":[{"name":"day","type":4,"value":1}],"type":1},"entitlement_sku_ids":[],"entitlements":[],"guild":{"features":["WELCOME_SCREEN_ENABLED","ACTIVITY_FEED_DISABLED_BY_USER","COMMUNITY","AUTO_MODERATION","GUILD_ONBOARDING","GUILD_ONBOARDING_EVER_ENABLED","SOUNDBOARD","GUILD_ONBOARDING_HAS_PROMPTS","NEWS","CHANNEL_ICON_EMOJIS_GENERATED"],"id":"1070516376046944286","locale":"en-US"},"guild_id":"1070516376046944286","guild_locale":"en-US","id":"1354214732164235476","locale":"en-US","member":{"avatar":null,"banner":null,"communication_disabled_until":null,"deaf":false,"flags":2,"joined_at":"2023-02-02T01:34:40.083000+00:00","mute":false,"nick":null,"pending":false,"permissions":"2251799813685247","premium_since":null,"roles":["1070520199775780894","1070518892243464255"],"unusual_dm_activity_until":null,"user":{"avatar":"9c6d68fafb9b204f9a499c2768e964bc","avatar_decoration_data":{"asset":"a_c3c09bd122898be35093d0d59850f627","expires_at":null,"sku_id":"1144305233707671573"},"clan":null,"collectibles":null,"discriminator":"0","global_name":"Lunar","id":"336733686529654798","primary_guild":null,"public_flags":4194560,"username":"oolunar"}},"token":"aW50ZXJhY3Rpb246MTM1NDIxNDczMjE2NDIzNTQ3NjptZ2RXR21EVUk1bmVXeFRwYXowcUh6YklMRGFvV2daOTZPMUgxR3RjUTljbEVpTnB4UGNuVUU5d1Rqd3FTM1hlTk9NRGFTaUNpa29sbG1uU2NpMEhOY2NEbjZFeXp3MDZKNXJuYU9GdlkwaXZONVhKUjZvcjl6cmt6b3ZDMG5JMA","type":2,"version":1}"""u8.ToArray();

    [GlobalSetup]
    public void Setup() => DiscordData.SetupStaticVariablesAsync().GetAwaiter().GetResult();

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask ExecuteInteractionAsync(DiscordClient client) => await client.ServiceProvider.GetRequiredService<IInteractionTransportService>().HandleHttpInteractionAsync(interactionPayload, default);

    public IEnumerable<object> GetDiscordClient()
    {
        Setup();

        yield return DiscordData.Client;
    }
}
