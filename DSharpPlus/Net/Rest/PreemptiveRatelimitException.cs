namespace DSharpPlus.Net;

using System;
using System.Diagnostics.CodeAnalysis;

internal class PreemptiveRatelimitException : Exception
{
    public required string Scope { get; set; }

    public required TimeSpan ResetAfter { get; set; }

    [SetsRequiredMembers]
    public PreemptiveRatelimitException(string scope, TimeSpan resetAfter)
    {
        Scope = scope;
        ResetAfter = resetAfter;
    }
}
