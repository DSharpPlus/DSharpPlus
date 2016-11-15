using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordInvite
    {
        [JsonProperty("code")]
        public string Code { get; internal set; }
        [JsonProperty("guild")]
        public DiscordInviteGuild Guild { get; internal set; }
        [JsonProperty("channel")]
        public DiscordInviteChannel Channel { get; internal set; }
    }
}