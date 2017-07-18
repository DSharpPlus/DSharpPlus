using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordWebhook : SnowflakeObject, IEquatable<DiscordWebhook>
    {
        /// <summary>
        /// The guild id this webhook is for
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// The channel id this webhook is for
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelId { get; internal set; }

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
        internal string AvatarHash { get; set; }

        /// <summary>
        /// Gets the default avatar URL for this webhook.
        /// </summary>
        public string AvatarUrl => $"https://cdn.discordapp.com/avatars/{this.Id}/{this.AvatarHash}.png?size=1024";

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
        public Task ExecuteAsync(string content = "", string username = "", string avatar_url = "", bool tts = false, IEnumerable<DiscordEmbed> embeds = null) =>
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

        /// <summary>
        /// Checks whether this <see cref="DiscordWebhook"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordWebhook"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordWebhook);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordWebhook"/> is equal to another <see cref="DiscordWebhook"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordWebhook"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordWebhook"/> is equal to this <see cref="DiscordWebhook"/>.</returns>
        public bool Equals(DiscordWebhook e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordWebhook"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordWebhook"/>.</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordWebhook"/> objects are equal.
        /// </summary>
        /// <param name="e1">First webhook to compare.</param>
        /// <param name="e2">Second webhook to compare.</param>
        /// <returns>Whether the two webhooks are equal.</returns>
        public static bool operator ==(DiscordWebhook e1, DiscordWebhook e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            if (o1 == null && o2 == null)
                return true;

            return e1.Id == e2.Id;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordWebhook"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First webhook to compare.</param>
        /// <param name="e2">Second webhook to compare.</param>
        /// <returns>Whether the two webhooks are not equal.</returns>
        public static bool operator !=(DiscordWebhook e1, DiscordWebhook e2) =>
            !(e1 == e2);
    }
}
