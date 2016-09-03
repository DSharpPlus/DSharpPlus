using Newtonsoft.Json.Linq;
using System;
using SharpCord.Objects;
namespace SharpCord.Events
{
    public class UnknownMessageEventArgs : EventArgs
    {
        public JObject RawJson { get; internal set; }
    }
}
