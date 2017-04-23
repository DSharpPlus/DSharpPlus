using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordDMChannel : DiscordChannel
    {
        /// <summary>
        /// The recipient of the DM.
        /// </summary>
        [JsonProperty("recipient", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser Recipient { get; internal set; }


        /// <summary>
        /// Only use for Group DMs! Whitelised bots only. Requires user's oauth2 access token
        /// </summary>
        public Task AddDmRecipientAsync(ulong user_id, string accesstoken) => this.Discord._rest_client.InternalGroupDMAddRecipient(Id, user_id, accesstoken);
        /// <summary>
        /// Only use for Group DMs!
        /// </summary>
        public Task RemoveDmRecipientAsync(ulong user_id, string accesstoken) => this.Discord._rest_client.InternalGroupDMRemoveRecipient(Id, user_id);
    }
}
