using Newtonsoft.Json;
using System.Threading.Tasks;

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

        public async Task<DiscordInvite> Delete() => await DiscordClient.InternalDeleteInvite(Code);
        /// <summary>
        /// USER TOKEN ONLY
        /// </summary>
        /// <returns></returns>
        public async Task<DiscordInvite> Accept() => await DiscordClient.InternalAcceptInvite(Code);
    }
}