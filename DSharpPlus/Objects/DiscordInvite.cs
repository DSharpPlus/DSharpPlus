using Newtonsoft.Json;
using System.Threading.Tasks;

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
        [JsonProperty("code")]
        public string Code { get; internal set; }
        /// <summary>
        /// The guild this invite is for
        /// </summary>
        [JsonProperty("guild")]
        public DiscordInviteGuild Guild { get; internal set; }
        /// <summary>
        /// The channel this invite is for
        /// </summary>
        [JsonProperty("channel")]
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