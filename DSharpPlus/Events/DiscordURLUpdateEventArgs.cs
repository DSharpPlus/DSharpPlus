using System;
using System.Collections.Generic;
using DSharpPlus.Objects;

namespace DSharpPlus
{
    public class DiscordURLUpdateEventArgs : EventArgs
    {
        public string ID { get; internal set; }
        public string URL { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        //When all types are discovered, this will be changed to an enum
        public List<DiscordEmbed> Embeds { get; internal set; } = new List<DiscordEmbed>();
    }
}