using Newtonsoft.Json.Linq;
using System;
using SharpCord.Objects;
namespace SharpCord
{
    public class DiscordLeftVoiceChannelEventArgs : EventArgs
    {
        public DiscordMember User { get; internal set; }
        public DiscordServer Server { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}