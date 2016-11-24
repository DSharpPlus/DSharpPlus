using Newtonsoft.Json.Linq;
using System;
using DSharpPlus.Objects;

namespace DSharpPlus.Events
{
    public class DiscordUserUpdateEventArgs : EventArgs
    {
        public DiscordMember OriginalMember { get; internal set; }
        public DiscordMember NewMember { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}
