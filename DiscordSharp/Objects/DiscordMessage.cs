using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Objects
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
        public string content { get; set; }
        [JsonProperty("id")]
        public string id { get; internal set; }

        [Obsolete] //Obsolete, mention array is no longer the proper way to do this.
        public string[] mentions { get; internal set; }

        [JsonProperty("attachments")]
        public DiscordAttachment[] attachments { get; internal set; }


        //public string recipient_id { get; set; }
        public DiscordMember Recipient { get; internal set; }

        public DiscordMember author { get; internal set; }
        internal DiscordChannelBase channel { get; set; }
        public Type TypeOfChannelObject { get; internal set; }

        public dynamic Channel() =>
            Convert.ChangeType(this.channel, TypeOfChannelObject);

        [JsonProperty("timestamp")]
        public DateTime timestamp { get; internal set; }

        public JObject RawJson { get; internal set; }


        internal DiscordMessage() { }
    }
}
