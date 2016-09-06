using Newtonsoft.Json.Linq;
using System;
using SharpCord.Objects;
namespace SharpCord
{
    public class DiscordMentionEventArgs : EventArgs
    {
        public string Message { get; internal set; }
        public DiscordServer Server { get; internal set; }
        public DiscordMember Author { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordMessageType MessageType { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}