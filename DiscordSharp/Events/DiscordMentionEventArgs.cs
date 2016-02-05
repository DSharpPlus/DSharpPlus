using Newtonsoft.Json.Linq;
using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordMentionEventArgs : EventArgs
    {
        public string message { get; internal set; }
        public DiscordMember author { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordMessageType MessageType { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}