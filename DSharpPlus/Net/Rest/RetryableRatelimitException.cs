using System;
using System.Diagnostics.CodeAnalysis;

namespace DSharpPlus.Net;

internal sealed class RetryableRatelimitException : Exception
{
    public required TimeSpan ResetAfter { get; set; }

    [SetsRequiredMembers]
    public RetryableRatelimitException(TimeSpan resetAfter) 
        => this.ResetAfter = resetAfter;
}
