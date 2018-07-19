using System.Globalization;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.Entities
{
    internal sealed class LavalinkDestroy : LavalinkPayload
    {
        [JsonProperty("guildId")]
        public string GuildId { get; }

        public LavalinkDestroy(DiscordChannel chn)
            : base("destroy")
        {
            this.GuildId = chn.Guild.Id.ToString(CultureInfo.InvariantCulture);
        }
    }
}
