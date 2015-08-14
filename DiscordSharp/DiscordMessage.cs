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
        public string[] mentions { get; set; }
        public string recipient_id { get; set; }
    }
}
