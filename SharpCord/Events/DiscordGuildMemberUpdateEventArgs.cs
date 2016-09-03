using Newtonsoft.Json.Linq;
using System;
using SharpCord.Objects;
namespace SharpCord
{
    public class DiscordGuildMemberUpdateEventArgs : EventArgs
    {
        public DiscordMember MemberUpdate { get; internal set; }
        public DiscordServer ServerUpdated { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}