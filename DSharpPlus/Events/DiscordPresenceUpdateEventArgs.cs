using Newtonsoft.Json.Linq;
using System;
using DSharpPlus.Objects;

namespace DSharpPlus
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