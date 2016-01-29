using Newtonsoft.Json.Linq;
using System;

namespace DiscordSharp.Events
{
    public class UnknownMessageEventArgs : EventArgs
    {
        public JObject RawJson { get; internal set; }
    }
}
