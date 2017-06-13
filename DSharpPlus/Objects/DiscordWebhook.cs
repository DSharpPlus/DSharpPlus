using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildID { get; internal set; }
        /// <summary>
        /// The channel id this webhook is for
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelID { get; internal set; }
        /// <summary>
        /// The user this webhook was created by
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// The default name of webhook
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }
        /// <summary>
        /// The default avatar of webhook
        /// </summary>
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string Avatar { get; internal set; }
        /// <summary>
        /// The secure token of the webhook
        /// </summary>
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; internal set; }

        /// <summary>
        /// Modify the webhook
        /// </summary>
        /// <param name="name"></param>
        /// <param name="base64avatar"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> ModifyAsync(string name = "", string base64avatar = "") =>
            this.Discord._rest_client.InternalModifyWebhookAsync(Id, name, base64avatar, Token);
        /// <summary>
        /// Delete the webhook permanently
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync() => 
            this.Discord._rest_client.InternalDeleteWebhookAsync(Id, Token);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="username"></param>
        /// <param name="avatar_url"></param>
        /// <param name="tts"></param>
        /// <param name="embeds"></param>
        /// <returns></returns>
        public Task ExecuteAsync(string content = "", string username = "", string avatar_url = "", bool tts = false, List<DiscordEmbed> embeds = null) =>
            this.Discord._rest_client.InternalExecuteWebhookAsync(Id, Token, content, username, avatar_url, tts, embeds);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public Task ExecuteSlackAsync(string json) =>
            this.Discord._rest_client.InternalExecuteWebhookSlackAsync(Id, Token, json);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public Task ExecuteGithubAsync(string json) =>
            this.Discord._rest_client.InternalExecuteWebhookGithubAsync(Id, Token, json);
    }
}
