using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Events
{
    public class DiscordChannelUpdateEventArgs
    {
        public JObject RawJson { get; internal set; }
        public DiscordChannel OldChannel { get; internal set; }
        public DiscordChannel NewChannel { get; internal set; }
    }
}
