using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace DiscordSharp.Objects
{
    public class DiscordChannelBase
    {
        [JsonProperty("id")]
        public string id { get; internal set; }
        [JsonProperty("is_private")]
        public bool is_private { get; internal set; }

        internal DiscordChannelBase() { }
    }

    public class DiscordChannel : DiscordChannelBase
    {
        public string type { get; set; }
        public string name { get; set; }
        public string topic { get; set; }
        public string icon { get; set; }

        public List<DiscordPermissionOverride> PermissionOverrides { get; set; }

        public DiscordServer parent { get; internal set; }

        public DiscordMessage SendMessage(string message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{id}" + Endpoints.Messages;
            JObject result = JObject.Parse(WebWrapper.Post(url, DiscordClient.token, JsonConvert.SerializeObject(Utils.GenerateMessage(message))));

            DiscordMessage m = new DiscordMessage
            {
                id = result["id"].ToString(),
                attachments = result["attachments"].ToObject<DiscordAttachment[]>(),
                author = this.parent.members.Find(x => x.ID == result["author"]["id"].ToString()),
                channel = this,
                content = result["content"].ToString(),
                RawJson = result,
                timestamp = result["timestamp"].ToObject<DateTime>()
            };
            return m;
        }

        private void DeleteMessage(DiscordMessage message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{id}" + Endpoints.Messages + $"/{message.id}";
            var result = JObject.Parse(WebWrapper.Delete(url, DiscordClient.token));
        }

        internal DiscordChannel() { }
    }

    public class DiscordPrivateChannel : DiscordChannelBase
    {
        public DiscordMember recipient { get; set; }
        [JsonProperty("last_message_id")]
        private string LastMessageID { get; set; }

        internal DiscordPrivateChannel() { }
    }

    //kinda like the author
    public class DiscordRecipient
    {
        public string username { get; set; }
        public string id { get; set; }
        internal DiscordRecipient() { }
    }

    
}
