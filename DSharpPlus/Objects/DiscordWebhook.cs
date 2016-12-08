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
<<<<<<< HEAD
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("guild_id")]
>>>>>>> master
        public ulong GuildID { get; internal set; }
        /// <summary>
        /// The channel id this webhook is for
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("channel_id")]
>>>>>>> master
        public ulong ChannelID { get; internal set; }
        /// <summary>
        /// The user this webhook was created by
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("user")]
>>>>>>> master
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// The default name of webhook
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("name")]
>>>>>>> master
        public string Name { get; internal set; }
        /// <summary>
        /// The default avatar of webhook
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("avatar")]
>>>>>>> master
        public string Avatar { get; internal set; }
        /// <summary>
        /// The secure token of the webhook
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("token")]
>>>>>>> master
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
