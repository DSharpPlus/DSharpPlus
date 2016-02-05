using System;
using System.Collections.Generic;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordEmbeds
    {
        public string url { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string provider_url { get; set; }
        public string provider_name { get; set; }
        public string description { get; set; }
    }

    public class DiscordURLUpdateEventArgs : EventArgs
    {
        public string id { get; internal set; }
        public string url { get; internal set; }
        public DiscordChannel channel { get; internal set; }
        //When all types are discovered, this will be changed to an enum
        public List<DiscordEmbeds> embeds { get; internal set; } = new List<DiscordEmbeds>();
    }
}