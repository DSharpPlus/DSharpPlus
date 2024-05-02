
using System;

namespace DSharpPlus.Net;
/// <summary>
/// A value-type variant of <seealso cref="RateLimitBucket"/> for extraction, in case we don't need the object.
/// </summary>
internal readonly record struct RateLimitCandidateBucket(int Maximum, int Remaining, DateTime Reset)
{
    public RateLimitBucket ToFullBucket()
        => new(Maximum, Remaining, Reset);
}
