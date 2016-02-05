using Newtonsoft.Json.Linq;
using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public enum DiscordUserStatus { ONLINE, IDLE, OFFLINE }

    public class DiscordPresenceUpdateEventArgs : EventArgs
    {
        public DiscordMember user { get; internal set; }
        public DiscordUserStatus status { get; internal set; }
        public string game { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}