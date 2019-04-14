using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity
{
    /// <summary>
    /// Interactivity result
    /// </summary>
    /// <typeparam name="T">Type of result</typeparam>
    public readonly struct InteractivityResult<T>
    {
        /// <summary>
        /// Whether interactivity was timed out
        /// </summary>
        public readonly bool TimedOut;
        /// <summary>
        /// Result
        /// </summary>
        public readonly T Result;

        internal InteractivityResult(bool timedout, T result)
        {
            this.TimedOut = timedout;
            this.Result = result;
        }
    }
}
