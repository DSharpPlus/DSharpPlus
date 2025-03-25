using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Tools.Benchmarks.Cases;

public class TextCommandVarArgsExecutionBenchmarks
{
    private static readonly DiscordMessage message = DiscordJson.ToDiscordObject<DiscordMessage>(JToken.Parse("""{"type":0,"tts":false,"timestamp":"2024-10-02T01:04:48.604-05:00","pinned":false,"nonce":"1290917384118337536","mentions":[],"mention_roles":[],"mention_everyone":false,"member":{"roles":["402444174223343617","1252238597004853379","573612339924959270","379397810476417064","973818122211590154","1204470581274353674","750044518849445909","1059190345688158358","1057954598683414619","1163786594138992670"],"premium_since":null,"pending":false,"nick":null,"mute":false,"joined_at":"2024-09-26T23:50:07.584-05:00","flags":11,"deaf":false,"communication_disabled_until":null,"banner":null,"avatar":null},"id":"1290917384726515742","flags":0,"embeds":[],"edited_timestamp":null,"content":"!enum Monday","components":[],"channel_id":"379379415475552276","author":{"username":"oolunar","public_flags":4194560,"id":"336733686529654798","global_name":"Lunar","discriminator":"0","clan":{"tag":"Moon","identity_guild_id":"832354798153236510","identity_enabled":true,"badge":"f09dccb8074d3c1a3bccadff9ceee10b"},"avatar_decoration_data":null,"avatar":"cb52688afd66f14e8a433396cd84c7c7"},"attachments":[],"guild_id":"379378609942560770"}"""));
    private static MessageCreatedEventArgs varArgsCommand0Args;
    private static MessageCreatedEventArgs varArgsCommand1Args;
    private static MessageCreatedEventArgs varArgsCommand2Args;
    private static MessageCreatedEventArgs varArgsCommand3Args;
    private static MessageCreatedEventArgs varArgsCommand4Args;
    private static MessageCreatedEventArgs varArgsCommand5Args;
    private static MessageCreatedEventArgs varArgsCommand6Args;
    private static MessageCreatedEventArgs varArgsCommand7Args;
    private static MessageCreatedEventArgs varArgsCommand8Args;
    private static MessageCreatedEventArgs varArgsCommand9Args;
    private static MessageCreatedEventArgs varArgsCommand10Args;

    [GlobalSetup]
    public void Setup()
    {
        bool isConnected = DiscordData.IsConnected;
        DiscordData.SetupStaticVariablesAsync().GetAwaiter().GetResult();
        if (!isConnected)
        {
            varArgsCommand0Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            varArgsCommand1Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs 1", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            varArgsCommand2Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs 1 2", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            varArgsCommand3Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs 1 2 3", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            varArgsCommand4Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs 1 2 3 4", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            varArgsCommand5Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs 1 2 3 4 5", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            varArgsCommand6Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs 1 2 3 4 5 6", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            varArgsCommand7Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs 1 2 3 4 5 6 7", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            varArgsCommand8Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs 1 2 3 4 5 6 7 8", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            varArgsCommand9Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs 1 2 3 4 5 6 7 8 9", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            varArgsCommand10Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!varargs 1 2 3 4 5 6 7 8 9 10", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
        }
    }

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute0VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand0Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute1VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand1Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute2VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand2Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute3VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand3Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute4VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand4Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute5VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand5Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute6VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand6Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute7VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand7Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute8VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand8Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute9VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand9Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Execute10VarArgsCommandAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, varArgsCommand10Args);

    public IEnumerable<object> GetDiscordClient()
    {
        Setup();

        yield return DiscordData.Client;
    }
}
