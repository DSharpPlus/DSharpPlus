using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    /// <summary>
    /// This is Voltana's class for Unix time that I've borrowed for testing.
    /// </summary>
    internal class EpochTime
    {
        private const long epoch = 621355968000000000L;

        public static long GetMilliseconds() => (DateTime.UtcNow.Ticks - epoch) / TimeSpan.TicksPerMillisecond;
    }
}
