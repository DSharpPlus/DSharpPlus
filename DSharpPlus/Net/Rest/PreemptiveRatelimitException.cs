using System;
using System.Diagnostics.CodeAnalysis;

namespace DSharpPlus.Net;

internal class PreemptiveRatelimitException : Exception
{
    public required string Scope { get; set; }

    public required TimeSpan ResetAfter { get; set; }

    [SetsRequiredMembers]
    public PreemptiveRatelimitException(string scope, TimeSpan resetAfter)
    {
        this.Scope = scope;
        this.ResetAfter = resetAfter;
    }
}
