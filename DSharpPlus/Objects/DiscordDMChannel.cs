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
        public async Task AddDMRecipient(ulong user_id, string accesstoken) => await DiscordClient.InternalGroupDMAddRecipient(ID, user_id, accesstoken);
        /// <summary>
        /// Only use for Group DMs!
        /// </summary>
        public async Task RemoveDMRecipient(ulong user_id, string accesstoken) => await DiscordClient.InternalGroupDMRemoveRecipient(ID, user_id);
    }
}
