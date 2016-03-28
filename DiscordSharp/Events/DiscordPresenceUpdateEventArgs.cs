using Newtonsoft.Json.Linq;
using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public enum DiscordUserStatus { ONLINE, IDLE, OFFLINE }

    public class DiscordPresenceUpdateEventArgs : EventArgs
    {
        public DiscordMember User { get; internal set; }
        public DiscordUserStatus Status { get; internal set; }
        public string Game { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}