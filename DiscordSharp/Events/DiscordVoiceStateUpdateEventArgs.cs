using Newtonsoft.Json.Linq;
using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordVoiceStateUpdateEventArgs : EventArgs
    {
        public DiscordMember User { get; internal set; }
        public DiscordServer Guild { get; internal set; }
        public bool SelfMute { get; set; }
        public bool SelfDeaf { get; internal set; }
        public bool Mute { get; internal set; }
        public bool Suppress { get; internal set; }
        public bool Deaf { get; internal set; }
        public string Token {get; internal set;}
        public DiscordChannel Channel{get; internal set;}

        public JObject RawJson { get; internal set; }
    }
}