using System;

namespace DSharpPlus
{
    public class ClientErrorEventArgs : EventArgs
    {
        public Exception Exception { get; internal set; }
        public string EventName { get; internal set; }
    }
}
