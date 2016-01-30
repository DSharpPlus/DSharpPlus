using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    internal class EpochTime
    {
        private const long epoch = 621355968000000000L;

        public static long GetMilliseconds() => (DateTime.UtcNow.Ticks - epoch) / TimeSpan.TicksPerMillisecond;
    }
}
