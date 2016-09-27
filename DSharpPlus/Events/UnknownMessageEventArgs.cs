using Newtonsoft.Json.Linq;
using System;
using DSharpPlus.Objects;
namespace DSharpPlus.Events
{
    public class UnknownMessageEventArgs : EventArgs
    {
        public JObject RawJson { get; internal set; }
    }
}
