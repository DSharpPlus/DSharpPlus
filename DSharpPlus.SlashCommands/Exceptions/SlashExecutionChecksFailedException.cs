using System;
using System.Collections.Generic;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Thrown when a pre-execution check for a slash command fails.
    /// </summary>
    [Serializable]
    public sealed class SlashExecutionChecksFailedException : Exception
    {
        /// <summary>
        /// The list of failed checks.
        /// </summary>
        public IReadOnlyList<SlashCheckBaseAttribute> FailedChecks;
    }
}
