using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp.Objects;
namespace DiscordSharp.Events
{
    public class DiscordMessageEventArgs : EventArgs
    {
        public string MessageText { get; internal set; }

        public DiscordMember Author { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordMessage Message { get; internal set; }
        public DiscordMessageType MessageType { get; internal set; }

        public JObject RawJson { get; internal set; }
    }

    public class DiscordMessageEditedEventArgs : EventArgs
    {
        public DiscordMessage MessageEdited { get; internal set; }

        public DiscordMember Author { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public string MessageText { get; internal set; }
        public DiscordMessageType MessageType { get; internal set; }
        public DateTime EditedTimestamp { get; internal set; }

        public JObject RawJson { get; internal set; }

        public DiscordMessageEditedEventArgs(JObject message)
        {
        }
        public DiscordMessageEditedEventArgs() { }
    }
}
