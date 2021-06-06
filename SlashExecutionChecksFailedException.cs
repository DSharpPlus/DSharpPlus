using System;
using System.Collections.Generic;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Thrown when a pre-execution check for a slash command fails.
    /// </summary>
    public class SlashExecutionChecksFailedException : Exception
    {
        /// <summary>
        /// The list of failed checks.
        /// </summary>
        public IReadOnlyList<SlashCheckBaseAttribute> FailedChecks;

        internal SlashExecutionChecksFailedException(List<SlashCheckBaseAttribute> failedChecks)
        {
            FailedChecks = failedChecks;
        }
    }
}