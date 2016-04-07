using System;
using System.Collections.Generic;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordEmbeds
    {
        public string URL { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string ProviderURL { get; set; }
        public string ProviderName { get; set; }
        public string Description { get; set; }
    }

    public class DiscordURLUpdateEventArgs : EventArgs
    {
        public string ID { get; internal set; }
        public string URL { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        //When all types are discovered, this will be changed to an enum
        public List<DiscordEmbeds> Embeds { get; internal set; } = new List<DiscordEmbeds>();
    }
}