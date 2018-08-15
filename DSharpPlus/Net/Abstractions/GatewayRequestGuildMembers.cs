using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    internal sealed class GatewayRequestGuildMembers
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; }

        [JsonProperty("query")]
        public string Query { get; }

        [JsonProperty("limit")]
        public int Limit { get; }

        public GatewayRequestGuildMembers(DiscordGuild guild)
        {
            this.GuildId = guild.Id;
            this.Query = "";
            this.Limit = 0;
        }
    }
}
