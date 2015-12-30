using Newtonsoft.Json.Linq;

namespace DiscordSharp
{
    public class DiscordGuildRoleUpdateEventArgs
    {
        public DiscordRole RoleUpdated { get; internal set; }
        public DiscordServer InServer { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}