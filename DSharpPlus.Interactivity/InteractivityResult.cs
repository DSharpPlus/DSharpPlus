using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity
{
    public readonly struct InteractivityResult<T>
    {
        public readonly bool TimedOut;
        public readonly T Result;

        internal InteractivityResult(bool timedout, T result)
        {
            this.TimedOut = timedout;
            this.Result = result;
        }
    }
}
