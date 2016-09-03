using System;
using SharpCord.Objects;
using Newtonsoft.Json.Linq;

namespace SharpCord
{
    public class DiscordPrivateMessageEventArgs : EventArgs
    {
        public DiscordPrivateChannel Channel { get; internal set; }
        public DiscordMember Author { get; internal set; }
        public string Message { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}