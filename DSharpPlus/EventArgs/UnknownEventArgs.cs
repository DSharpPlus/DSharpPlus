using System;

namespace DSharpPlus
{
    public class UnknownEventArgs : EventArgs
    {
        public string EventName { get; internal set; }
        public string Json { get; internal set; }
    }
}
