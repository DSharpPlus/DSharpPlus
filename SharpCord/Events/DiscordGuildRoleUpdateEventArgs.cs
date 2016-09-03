using Newtonsoft.Json.Linq;
using System;
using SharpCord.Objects;
namespace SharpCord
{
    public class DiscordGuildRoleUpdateEventArgs : EventArgs
    {
        public DiscordRole RoleUpdated { get; internal set; }
        public DiscordServer InServer { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}