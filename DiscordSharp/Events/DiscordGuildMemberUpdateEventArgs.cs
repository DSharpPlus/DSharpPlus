using Newtonsoft.Json.Linq;
using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordGuildMemberUpdateEventArgs : EventArgs
    {
        public DiscordMember MemberUpdate { get; internal set; }
        public DiscordServer ServerUpdated { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}