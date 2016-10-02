using DSharpPlus.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Hook
{
    public class Webhook
    {
        [JsonProperty("id")]
        public string ID { get; internal set; }

        [JsonProperty("guild_id")]
        public string ServerID { get; internal set; }

        [JsonProperty("channel_id")]
        public string ChannelID { get; internal set; }

        [JsonProperty("user")]
        public DiscordMember User { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("avatar")]
        public string Avatar { get; internal set; }

        [JsonProperty("token")]
        public string Token { get; internal set; }

        public void Modify(string Name = "", string Avatar = "")
        {
            string url = Endpoints.BaseAPI + Endpoints.Webhooks + $"/{ID}";
            JObject content = new JObject() { { "name", Name }, { "avatar", Avatar } };
            var result = JObject.Parse(WebWrapper.Patch(url, DiscordClient.token, content.ToString()));
            Webhook updated = JsonConvert.DeserializeObject<Webhook>(result.ToString());
            this.Name = updated.Name;
            this.Avatar = updated.Avatar;
        }

        public void Delete()
        {
            string url = Endpoints.BaseAPI + Endpoints.Webhooks + $"/{ID}/{Token}";
            WebWrapper.Delete(url, DiscordClient.token);
        }

        public void SendMessage(string message, bool wait = false)
        {
            JObject content = new JObject() { { "content", message } };
            Execute(content[0], wait);
        }

        public void SendEmbeds(List<DiscordEmbed> embeds, bool wait = false)
        {
            JObject content = new JObject() { { "embeds", JsonConvert.SerializeObject(embeds) } };
            Execute(content[0], wait);
        }

        public void SendSlack(JObject message, bool wait = false)
        {
            if (message["username"] == null)
                message.Add("username", Name);
            
            string url = Endpoints.BaseAPI + Endpoints.Webhooks + $"/{ID}/{Token}" + Endpoints.Slack + $"?wait={wait.ToString()}";
            WebWrapper.Post(url, message.ToString());
        }

        private void Execute(JToken message, bool wait = false)
        {
            string url = Endpoints.BaseAPI + Endpoints.Webhooks + $"/{ID}/{Token}?wait={wait.ToString()}";
            JObject content = new JObject() { { "username", Name }, message };
            WebWrapper.Post(url, content.ToString());
        }
    }
}
