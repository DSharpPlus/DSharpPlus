using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordDMChannel : DiscordChannel
    {
        /// <summary>
        /// The recipient of the dm
        /// </summary>
        [JsonProperty("recipient")]
        public DiscordUser Recipient { get; internal set; }


        /// <summary>
        /// Only use for Group DM's! Whitelised bots only. Requires user's oauth2 access token
        /// </summary>
        public async Task AddDMRecipient(ulong UserID, string accesstoken) => await DiscordClient.InternalGroupDMAddRecipient(ID, UserID, accesstoken);
        /// <summary>
        /// Only use for Group DM's!
        /// </summary>
        public async Task RemoveDMRecipient(ulong UserID, string accesstoken) => await DiscordClient.InternalGroupDMRemoveRecipient(ID, UserID);
    }
}
