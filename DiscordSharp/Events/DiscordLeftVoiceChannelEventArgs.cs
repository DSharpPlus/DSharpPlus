using Newtonsoft.Json.Linq;
using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordLeftVoiceChannelEventArgs : EventArgs
    {
        public DiscordMember User { get; internal set; }
        public DiscordServer Guild { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}