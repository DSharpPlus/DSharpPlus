using Newtonsoft.Json.Linq;
using System;

namespace DSharpPlus.Events
{
    public class UnknownMessageEventArgs : EventArgs
    {
        public JObject RawJson { get; internal set; }
    }
}
