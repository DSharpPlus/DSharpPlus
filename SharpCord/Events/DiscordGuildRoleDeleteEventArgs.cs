using Newtonsoft.Json.Linq;
using System;
using SharpCord.Objects;
namespace SharpCord
{
    public class DiscordGuildRoleDeleteEventArgs : EventArgs
    {
        public DiscordRole DeletedRole { get; internal set; }
        public DiscordServer Server { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}