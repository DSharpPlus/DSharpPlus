using Newtonsoft.Json.Linq;

namespace DiscordSharp
{
    public class DiscordGuildRoleDeleteEventArgs
    {
        public DiscordRole DeletedRole { get; internal set; }
        public DiscordServer Guild { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}