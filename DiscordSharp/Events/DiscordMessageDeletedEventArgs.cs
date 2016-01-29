using Newtonsoft.Json.Linq;
using System;

namespace DiscordSharp.Events
{
    public class DiscordMessageDeletedEventArgs : EventArgs
    {
        public DiscordMessage DeletedMessage { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}
