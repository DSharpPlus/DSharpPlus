namespace DSharpPlus.Lavalink.Entities;
using System.Globalization;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

internal sealed class LavalinkVoiceServerUpdate
{
    [JsonProperty("token")]
    public string Token { get; }

    [JsonProperty("guild_id")]
    public string GuildId { get; }

    [JsonProperty("endpoint")]
    public string Endpoint { get; }

    internal LavalinkVoiceServerUpdate(VoiceServerUpdateEventArgs vsu)
    {
        Token = vsu.VoiceToken;
        GuildId = vsu.Guild.Id.ToString(CultureInfo.InvariantCulture);
        Endpoint = vsu.Endpoint;
    }
}

internal sealed class LavalinkVoiceUpdate : LavalinkPayload
{
    [JsonProperty("sessionId")]
    public string SessionId { get; }

    [JsonProperty("event")]
    internal LavalinkVoiceServerUpdate Event { get; }

    public LavalinkVoiceUpdate(VoiceStateUpdateEventArgs vstu, VoiceServerUpdateEventArgs vsrvu)
        : base("voiceUpdate", vstu.Guild.Id.ToString(CultureInfo.InvariantCulture))
    {
        SessionId = vstu.SessionId;
        Event = new LavalinkVoiceServerUpdate(vsrvu);
    }
}
