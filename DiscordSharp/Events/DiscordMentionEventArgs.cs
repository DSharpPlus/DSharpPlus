using Newtonsoft.Json.Linq;

namespace DiscordSharp
{
    public class DiscordMentionEventArgs
    {
        public string message { get; internal set; }
        public DiscordMember author { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordMessageType MessageType { get; internal set; }

        public JObject RawJson { get; internal set; }
    }
}