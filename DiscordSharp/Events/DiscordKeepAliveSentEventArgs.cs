using System;

namespace DiscordSharp
{
    public class DiscordKeepAliveSentEventArgs : EventArgs
    {
        public DateTime SentAt { get; set; }
        public string JsonSent { get; set; }
    }
}