using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a Discord invite.
    /// </summary>
    public class DiscordInvite
    {
        internal DiscordClient Discord { get; set; }

        /// <summary>
        /// Gets the invite's code.
        /// </summary>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; internal set; }

        /// <summary>
        /// Gets the guild this invite is for.
        /// </summary>
        [JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInviteGuild Guild { get; internal set; }

        /// <summary>
        /// Gets the channel this invite is for.
        /// </summary>
        [JsonProperty("channel", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInviteChannel Channel { get; internal set; }

        /// <summary>
        /// Deletes the invite.
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task<DiscordInvite> DeleteAsync(string reason = null) => this.Discord._rest_client.InternalDeleteInvite(this.Code, reason);

        /*
         * Disabled due to API restrictions.
         * 
         * /// <summary>
         * /// Accepts an invite. Not available to bot accounts. Requires "guilds.join" scope or user token. Please note that accepting these via the API will get your account unverified.
         * /// </summary>
         * /// <returns></returns>
         * [Obsolete("Using this method will get your account unverified.")]
         * public Task<DiscordInvite> AcceptAsync() => this.Discord._rest_client.InternalAcceptInvite(Code);
         */

        /// <summary>
        /// Converts this invite into an invite link.
        /// </summary>
        /// <returns>A discord.gg invite link.</returns>
        public override string ToString()
        {
            return $"https://discord.gg/{this.Code}";
        }
    }
}