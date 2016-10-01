using System;

namespace DSharpPlus.Objects
{
    public class DiscordLoginException : Exception
    {
        public DiscordLoginException() : base("A generic exception occurred while trying to login to Discord.")
        {}
        public DiscordLoginException(string message) : base(message)
        {}
        public DiscordLoginException(string message, Exception inner) : base(message, inner)
        {}
    }
}
