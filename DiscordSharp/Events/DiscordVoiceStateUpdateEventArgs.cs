using Newtonsoft.Json.Linq;

namespace DiscordSharp
{
    public class DiscordVoiceStateUpdateEventArgs
    {
        public DiscordMember user { get; internal set; }
        public DiscordServer guild { get; internal set; }
        public bool self_mute { get; set; }
        public bool self_deaf { get; internal set; }
        public bool mute { get; internal set; }
        public bool suppress { get; internal set; }
        public bool deaf { get; internal set; }
        public string token {get; internal set;}
        public DiscordChannel channel{get; internal set;}

        public JObject RawJson { get; internal set; }
    }
}