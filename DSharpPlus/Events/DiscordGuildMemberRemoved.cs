using Newtonsoft.Json.Linq;
using System;
using DSharpPlus.Objects;

namespace DSharpPlus.Events
{
    public class DiscordGuildMemberRemovedEventArgs : EventArgs
    {
        public DiscordMember MemberRemoved { get; internal set; }
        public DiscordServer Server { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}
