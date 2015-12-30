using Newtonsoft.Json.Linq;

namespace DiscordSharp
{
    public class DiscordGuildMemberUpdateEventArgs
    {
        public DiscordMember MemberUpdate { get; internal set; }
        public DiscordServer ServerUpdated { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}