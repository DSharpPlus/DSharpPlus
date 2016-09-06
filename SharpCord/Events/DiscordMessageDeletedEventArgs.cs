using Newtonsoft.Json.Linq;
using System;
using SharpCord.Objects;
namespace SharpCord.Events
{
    public class DiscordMessageDeletedEventArgs : EventArgs
    {
        public DiscordServer Server => Channel.Parent;
        public DiscordMessage DeletedMessage { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public JObject RawJson { get; internal set; }
    }

    public class DiscordPrivateMessageDeletedEventArgs : DiscordMessageDeletedEventArgs
    {
        public new DiscordPrivateChannel Channel { get; internal set; }
    }
}
