
using System;
using System.Globalization;
using System.Text;

namespace DSharpPlus.Metrics;
/// <summary>
/// Represents an immutable snapshot of request metrics.
/// </summary>
public readonly record struct RequestMetricsCollection
{
    /// <summary>
    /// The total amount of requests made during the specified duration.
    /// </summary>
    public int TotalRequests { get; init; }

    /// <summary>
    /// The successful requests made during the specified duration.
    /// </summary>
    public int SuccessfulRequests { get; init; }

    /// <summary>
    /// The failed requests made during the specified duration.
    /// </summary>
    public int FailedRequests => TotalRequests - SuccessfulRequests;

    /// <summary>
    /// The amount of ratelimits hit during the specified duration.
    /// </summary>
    public int RatelimitsHit { get; init; }

    /// <summary>
    /// The amount of global ratelimits hit during the specified duration.
    /// </summary>
    public int GlobalRatelimitsHit { get; init; }

    /// <summary>
    /// The amount of bucket ratelimits hit during the specified duration.
    /// </summary>
    public int BucketRatelimitsHit { get; init; }

    /// <summary>
    /// The amount of bad requests made during the specified duration.
    /// </summary>
    public int BadRequests { get; init; }

    /// <summary>
    /// The amount of forbidden or unauthorized requests made during the specified duration.
    /// </summary>
    public int Forbidden { get; init; }

    /// <summary>
    /// The amount of requests whose target could not be found made during the specified duration.
    /// </summary>
    public int NotFound { get; init; }

    /// <summary>
    /// The amount of requests whose payload was too large during the specified duration.
    /// </summary>
    public int TooLarge { get; init; }

    /// <summary>
    /// The amount of server errors hit during the specified duration.
    /// </summary>
    public int ServerErrors { get; init; }

    /// <summary>
    /// The duration covered by these metrics.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Returns a human-readable string representation of these metrics.
    /// </summary>
    public override readonly string ToString()
    {
        StringBuilder builder = new($"Total Requests: {TotalRequests} during {Duration}\n");

        if (SuccessfulRequests > 0)
        {
            builder.AppendLine
            (
                CultureInfo.CurrentCulture,
                $"Successful Requests: {SuccessfulRequests} ({Percentage(TotalRequests, SuccessfulRequests)})"
            );
        }

        if (FailedRequests > 0)
        {
            builder.AppendLine
            (
                CultureInfo.CurrentCulture,
                $"Failed Requests: {FailedRequests} ({Percentage(TotalRequests, FailedRequests)})"
            );

            if (RatelimitsHit > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Ratelimits hit: {RatelimitsHit} ({Percentage(TotalRequests, RatelimitsHit)})"
                );

                if (GlobalRatelimitsHit > 0)
                {
                    builder.AppendLine
                    (
                        CultureInfo.CurrentCulture,
                        $"    - Global ratelimits hit: {GlobalRatelimitsHit} ({Percentage(TotalRequests, GlobalRatelimitsHit)})"
                    );
                }

                if (BucketRatelimitsHit > 0)
                {
                    builder.AppendLine
                    (
                        CultureInfo.CurrentCulture,
                        $"    - Bucket ratelimits hit: {BucketRatelimitsHit} ({Percentage(TotalRequests, BucketRatelimitsHit)})"
                    );
                }
            }

            if (BadRequests > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Bad requests executed: {BadRequests} ({Percentage(TotalRequests, BadRequests)})"
                );
            }

            if (Forbidden > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Forbidden/Unauthorized requests executed: {Forbidden} ({Percentage(TotalRequests, Forbidden)})"
                );
            }

            if (NotFound > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Requests not found: {NotFound} ({Percentage(TotalRequests, NotFound)})"
                );
            }

            if (TooLarge > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Requests too large: {TooLarge} ({Percentage(TotalRequests, TooLarge)})"
                );
            }

            if (ServerErrors > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Server errors: {ServerErrors} ({Percentage(TotalRequests, ServerErrors)})"
                );
            }
        }

        return builder.ToString();
    }

    private static string Percentage(int total, int part)
    {
        double ratio = (double)part / total;
        ratio *= 100;
        return $"{ratio:N4}%";
    }
}
