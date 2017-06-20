using System;

namespace DSharpPlus
{
    public class SocketMessageEventArgs : EventArgs
    {
        public string Message { get; internal set; }
    }
}
