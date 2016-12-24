using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordInvite
    {
        /// <summary>
        /// The invite code (unique ID)
        /// </summary>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; internal set; }
        /// <summary>
        /// The guild this invite is for
        /// </summary>
        [JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInviteGuild Guild { get; internal set; }
        /// <summary>
        /// The channel this invite is for
        /// </summary>
        [JsonProperty("channel", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInviteChannel Channel { get; internal set; }

        /// <summary>
        /// Delete the invite
        /// </summary>
        /// <returns></returns>
        public async Task<DiscordInvite> Delete() => await DiscordClient.InternalDeleteInvite(Code);
        /// <summary>
        /// Accept an invite. Not available to bot accounts. Requires "guilds.join" scope or user token.
        /// </summary>
        /// <returns></returns>
        public async Task<DiscordInvite> Accept() => await DiscordClient.InternalAcceptInvite(Code);
    }
}