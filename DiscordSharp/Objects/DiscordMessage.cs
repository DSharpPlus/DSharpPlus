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
        public string content { get; set; }
        public string id { get; internal set; }

        [Obsolete] //Obsolete, mention array is no longer the proper way to do this.
        public string[] mentions { get; internal set; }

        public string[] attachments { get; internal set; }
        public string recipient_id { get; set; }
        public DiscordMember author { get; internal set; }
        public DiscordChannel channel { get; internal set; }
        public DateTime timestamp { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}
