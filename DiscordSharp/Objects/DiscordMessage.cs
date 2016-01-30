using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
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
        public string[] attachments { get; internal set; }


        //public string recipient_id { get; set; }
        public DiscordMember Recipient { get; internal set; }

        public DiscordMember author { get; internal set; }
        internal DiscordChannelBase channel { get; set; }
        public Type TypeOfChannelObject { get; internal set; }

        public dynamic Channel() =>
            Convert.ChangeType(this, TypeOfChannelObject);

        [JsonProperty("timestamp")]
        public DateTime timestamp { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}
