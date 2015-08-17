using System;

namespace DiscordSharp
{
    public class DiscordKeepAliveSentEventArgs
    {
        public DateTime SentAt { get; set; }
        public string JsonSent { get; set; }
    }
}