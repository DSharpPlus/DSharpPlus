using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordWebhook : SnowflakeObject
    {
        /// <summary>
        /// The guild id this webhook is for
        /// </summary>
        [JsonProperty("guild_id")]
        public ulong GuildID { get; internal set; }
        /// <summary>
        /// The channel id this webhook is for
        /// </summary>
        [JsonProperty("channel_id")]
        public ulong ChannelID { get; internal set; }
        /// <summary>
        /// The user this webhook was created by
        /// </summary>
        [JsonProperty("user")]
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// The default name of webhook
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }
        /// <summary>
        /// The default avatar of webhook
        /// </summary>
        [JsonProperty("avatar")]
        public string Avatar { get; internal set; }
        /// <summary>
        /// The secure token of the webhook
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; internal set; }

        /// <summary>
        /// Modify the webhook
        /// </summary>
        /// <param name="name"></param>
        /// <param name="base64avatar"></param>
        /// <returns></returns>
        public async Task<DiscordWebhook> Modify(string name = "", string base64avatar = "") => await DiscordClient.InternalModifyWebhook(ID, name, base64avatar, Token);
        /// <summary>
        /// Delete the webhook permanently
        /// </summary>
        /// <returns></returns>
        public async Task Delete() => await DiscordClient.InternalDeleteWebhook(ID, Token);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="username"></param>
        /// <param name="avatarurl"></param>
        /// <param name="tts"></param>
        /// <param name="embeds"></param>
        /// <returns></returns>
        public async Task Execute(string content = "", string username = "", string avatarurl = "", bool tts = false, List<DiscordEmbed> embeds = null)
            => await DiscordClient.InternalExecuteWebhook(ID, Token, content, username, avatarurl, tts, embeds);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Json"></param>
        /// <returns></returns>
        public async Task ExecuteSlack(string Json) => await DiscordClient.InternalExecuteWebhookSlack(ID, Token, Json);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Json"></param>
        /// <returns></returns>
        public async Task ExecuteGithub(string Json) => await DiscordClient.InternalExecuteWebhookGithub(ID, Token, Json);
    }
}
