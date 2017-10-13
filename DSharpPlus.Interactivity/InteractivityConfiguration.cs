using System;

namespace DSharpPlus.Interactivity
{
    public sealed class InteractivityConfiguration
    {
        public TimeSpan Timeout { get; set; }

        public TimeSpan PaginationTimeout { get; set; }

        public TimeoutBehaviour PaginationBehaviour { get; set; }
    }
}
