using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity
{
    public sealed class InteractivityConfiguration
    {
        public TimeSpan Timeout { get; set; }

        public TimeSpan PaginationTimeout { get; set; }

        public TimeoutBehaviour PaginationBehaviour { get; set; }
    }
}
