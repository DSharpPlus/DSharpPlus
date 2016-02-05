using Newtonsoft.Json.Linq;
using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordLeftVoiceChannelEventArgs : EventArgs
    {
        public DiscordMember user { get; internal set; }
        public DiscordServer guild { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}