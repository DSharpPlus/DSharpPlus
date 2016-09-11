using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus
{
    class RateLimiter
    {
        Thread limiter = new Thread(Limiter);
        public static void Limiter()
        {
            while (true)
            {
                Thread.Sleep(5000);
            }
        }
    }
}
