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
        public string ID { get; internal set; }
        [JsonProperty("is_private")]
        public bool Private { get; internal set; }

        internal DiscordChannelBase() { }
    }

    public enum ChannelType
    {
        Text, Voice
    }

    public class DiscordChannel : DiscordChannelBase
    {
        [JsonProperty("type")]
        public ChannelType Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        private int __bitrate;
        /// <summary>
        /// (Voice only) The channel's configured bitrate, in bps (bits per second).
        /// It's highly recommended you use this as opposed to your own bitrate.
        /// To retrieve the bitrate in kbps, divide by 1000.
        /// </summary>
        [JsonProperty("bitrate")]
        public int Bitrate
        {
            get
            {
                if (Type != ChannelType.Voice)
                    return -1;
                else
                    return __bitrate;
            }
            internal set
            {
                __bitrate = value;
            }
        }

        public List<DiscordPermissionOverride> PermissionOverrides { get; set; }

        public DiscordServer parent { get; internal set; }

        public DiscordMessage SendMessage(string message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID}" + Endpoints.Messages;
            JObject result = JObject.Parse(WebWrapper.Post(url, DiscordClient.token, JsonConvert.SerializeObject(Utils.GenerateMessage(message))));

            if (result["content"].IsNullOrEmpty())
                throw new InvalidOperationException("Request returned a blank message, you may not have permission to send messages yet!");

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
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID    }" + Endpoints.Messages + $"/{message.id}";
            var result = JObject.Parse(WebWrapper.Delete(url, DiscordClient.token));
        }

        internal DiscordChannel() { }
    }

    public class DiscordPrivateChannel : DiscordChannelBase
    {
        internal string user_id { get; set; }

        public DiscordMember recipient { get; set; }
        [JsonProperty("last_message_id")]
        private string LastMessageID { get; set; }

        internal DiscordPrivateChannel() { }
    }

    //kinda like the author
    [Obsolete]
    public class DiscordRecipient
    {
        public string username { get; set; }
        public string id { get; set; }
        internal DiscordRecipient() { }
    }

    
}
