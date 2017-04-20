using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class HeartBeatEventArgs : EventArgs
    {
        public int Ping { get; internal set; }
        public DateTimeOffset Timestamp { get; internal set; }
        public int IntegrityChecksum { get; internal set; }
    }
}
