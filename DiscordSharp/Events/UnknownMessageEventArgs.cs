using Newtonsoft.Json.Linq;

namespace DiscordSharp.Events
{
    public class UnknownMessageEventArgs
    {
        public JObject RawJson { get; internal set; }
    }
}
