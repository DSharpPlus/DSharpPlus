#pragma warning disable IDE0022 // that's a multiline body. we do not want that to be expression bodied.

namespace DSharpPlus.Net;

internal record struct RestMetricCollection
{
    // these are fields so that we can use Interlocked.Increment directly
    public int requests;
    public int successful;
    public int ratelimits;
    public int globalRatelimits;
    public int bucketRatelimits;
    public int badRequests;
    public int forbidden;
    public int notFound;
    public int tooLarge;
    public int serverError;

    /// <summary>
    /// Returns a human-readable string representation of these metrics.
    /// </summary>
    public override readonly string ToString()
    {
        return $"""
            Total Requests: {this.requests}
            Successful Requests: {this.successful} ({Percentage(this.requests, this.successful)})
            Failed Requests: {this.requests - this.successful} ({Percentage(this.requests, this.requests - this.successful)})
              - Ratelimits hit: {this.ratelimits} ({Percentage(this.requests, this.ratelimits)})
                - Global ratelimits hit: {this.globalRatelimits} ({Percentage(this.requests, this.globalRatelimits)})
                - Bucket ratelimits hit: {this.bucketRatelimits} ({Percentage(this.requests, this.bucketRatelimits)})
              - Bad requests executed: {this.badRequests} ({Percentage(this.requests, this.badRequests)})
              - Forbidden/Unauthorized requests executed: {this.forbidden} ({Percentage(this.requests, this.forbidden)})
              - Requests not found: {this.notFound} ({Percentage(this.requests, this.notFound)})
              - Requests too large: {this.tooLarge} ({Percentage(this.requests, this.tooLarge)})
              - Server Errors: {this.serverError} ({Percentage(this.requests, this.serverError)})
            """;
    }

    private static string Percentage(int total, int part)
    {
        double ratio = (double)part / total;
        ratio *= 100;
        return $"{ratio:N4}%";
    }
}
