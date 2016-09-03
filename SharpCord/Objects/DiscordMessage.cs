using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCord.Objects
{
    public struct DiscordAttachment
    {
        [JsonProperty("width")]
        public int Width { get; internal set; }
        [JsonProperty("height")]
        public int Height { get; internal set; }
        [JsonProperty("size")]
        public int Size { get; internal set; }
        [JsonProperty("filename")]
        public string Filename { get; internal set; }
        [JsonProperty("proxy_url")]
        public string ProxyURL { get; internal set; }
        [JsonProperty("url")]
        public string URL { get; internal set; }
    }

    /// <summary>
    /// Message to be sent
    /// </summary>
    public class DiscordMessage
    {
        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("id")]
        public string ID { get; internal set; }
        [JsonProperty("tts")]
        public bool TTS { get; set; }
        [JsonProperty("pinned")]
        public bool pinned { get; }

        [JsonProperty("attachments")]
        public DiscordAttachment[] Attachments { get; internal set; }


        //public string recipient_id { get; set; }
        public DiscordMember Recipient { get; internal set; }

        public DiscordMember Author { get; internal set; }
        internal DiscordChannelBase channel { get; set; }
        public Type TypeOfChannelObject { get; internal set; }

#if NETFX4_5
        public dynamic Channel() =>
            Convert.ChangeType(this.channel, TypeOfChannelObject);
#else
        public DiscordChannel Channel() => (DiscordChannel)channel;
        public DiscordPrivateChannel ChannelAsPrivate() => (DiscordPrivateChannel)channel;
#endif

        [JsonProperty("timestamp")]
        public DateTime timestamp { get; internal set; }

        public JObject RawJson { get; internal set; }


        public void Pin()
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID    }" + Endpoints.Pins + $"/{ID    }";
            WebWrapper.Put(url, DiscordClient.token);
        }

        public void Unpin()
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID    }" + Endpoints.Pins + $"/{ID    }";
            var result = JObject.Parse(WebWrapper.Delete(url, DiscordClient.token));
        }

        internal DiscordMessage() { }
    }
}
