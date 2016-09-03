using Newtonsoft.Json.Linq;
using System;
using SharpCord.Objects;

namespace SharpCord
{
    public class DiscordTypingStartEventArgs : EventArgs
    {
        public DiscordMember user { get; internal set; }
        public int Timestamp { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public JObject RawJson { get; internal set; } 
    }
}