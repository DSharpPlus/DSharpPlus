using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Utility
{
    internal static class EpochConvert
    {
        public static int GetEpochSeconds(this DateTime date)
        {
            TimeSpan t = date - new DateTime(1970, 1, 1);
            return (int)t.TotalSeconds;
        }

        public static DateTime FromEpochSeconds(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
