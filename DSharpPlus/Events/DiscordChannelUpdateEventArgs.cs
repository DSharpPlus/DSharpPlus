using DSharpPlus.Objects;
using Newtonsoft.Json.Linq;
using System;

namespace DSharpPlus.Events
{
    public class DiscordChannelUpdateEventArgs : EventArgs
    {
        public JObject RawJson { get; internal set; }
        public DiscordChannel OldChannel { get; internal set; }
        public DiscordChannel NewChannel { get; internal set; }
    }
}
