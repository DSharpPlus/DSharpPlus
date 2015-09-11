using Newtonsoft.Json.Linq;

namespace DiscordSharp
{
    public class DiscordLeftVoiceChannelEventArgs
    {
        public DiscordMember user { get; internal set; }
        public DiscordServer guild { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}