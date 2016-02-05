using Newtonsoft.Json.Linq;
using System;
using DiscordSharp.Objects;
namespace DiscordSharp.Events
{
    public class UnknownMessageEventArgs : EventArgs
    {
        public JObject RawJson { get; internal set; }
    }
}
