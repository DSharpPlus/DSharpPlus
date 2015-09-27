using Newtonsoft.Json.Linq;

namespace DiscordSharp.Events
{
    public class DiscordMessageDeletedEventArgs
    {
        public DiscordMessage DeletedMessage { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public JObject RawJson { get; internal set; }
    }
}
