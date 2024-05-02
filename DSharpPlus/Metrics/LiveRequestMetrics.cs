namespace DSharpPlus.Metrics;


internal record struct LiveRequestMetrics
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
}
