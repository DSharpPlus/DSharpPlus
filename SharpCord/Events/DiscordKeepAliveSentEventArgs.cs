using System;
using SharpCord.Objects;
namespace SharpCord
{
    public class DiscordKeepAliveSentEventArgs : EventArgs
    {
        public DateTime SentAt { get; set; }
        public string JsonSent { get; set; }
    }
}