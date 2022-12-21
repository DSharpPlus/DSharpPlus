using System;
using System.Collections.Generic;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Thrown when a pre-execution check for a slash command fails.
    /// </summary>
    public sealed class ContextMenuExecutionChecksFailedException : Exception
    {
        /// <summary>
        /// The list of failed checks.
        /// </summary>
        public IReadOnlyList<ContextMenuCheckBaseAttribute> FailedChecks;
    }
}
