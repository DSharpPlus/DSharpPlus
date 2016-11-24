using Newtonsoft.Json.Linq;
using System;
using DSharpPlus.Objects;

namespace DSharpPlus.Events
{
    public class DiscordGuildDeleteEventArgs : EventArgs
    {
        public DiscordServer Server { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}
