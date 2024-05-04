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
    public int FailedRequests => this.TotalRequests - this.SuccessfulRequests;

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
        StringBuilder builder = new($"Total Requests: {this.TotalRequests} during {this.Duration}\n");

        if (this.SuccessfulRequests > 0)
        {
            builder.AppendLine
            (
                CultureInfo.CurrentCulture,
                $"Successful Requests: {this.SuccessfulRequests} ({Percentage(this.TotalRequests, this.SuccessfulRequests)})"
            );
        }

        if (this.FailedRequests > 0)
        {
            builder.AppendLine
            (
                CultureInfo.CurrentCulture,
                $"Failed Requests: {this.FailedRequests} ({Percentage(this.TotalRequests, this.FailedRequests)})"
            );

            if (this.RatelimitsHit > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Ratelimits hit: {this.RatelimitsHit} ({Percentage(this.TotalRequests, this.RatelimitsHit)})"
                );

                if (this.GlobalRatelimitsHit > 0)
                {
                    builder.AppendLine
                    (
                        CultureInfo.CurrentCulture,
                        $"    - Global ratelimits hit: {this.GlobalRatelimitsHit} ({Percentage(this.TotalRequests, this.GlobalRatelimitsHit)})"
                    );
                }

                if (this.BucketRatelimitsHit > 0)
                {
                    builder.AppendLine
                    (
                        CultureInfo.CurrentCulture,
                        $"    - Bucket ratelimits hit: {this.BucketRatelimitsHit} ({Percentage(this.TotalRequests, this.BucketRatelimitsHit)})"
                    );
                }
            }

            if (this.BadRequests > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Bad requests executed: {this.BadRequests} ({Percentage(this.TotalRequests, this.BadRequests)})"
                );
            }

            if (this.Forbidden > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Forbidden/Unauthorized requests executed: {this.Forbidden} ({Percentage(this.TotalRequests, this.Forbidden)})"
                );
            }

            if (this.NotFound > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Requests not found: {this.NotFound} ({Percentage(this.TotalRequests, this.NotFound)})"
                );
            }

            if (this.TooLarge > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Requests too large: {this.TooLarge} ({Percentage(this.TotalRequests, this.TooLarge)})"
                );
            }

            if (this.ServerErrors > 0)
            {
                builder.AppendLine
                (
                    CultureInfo.CurrentCulture,
                    $"  - Server errors: {this.ServerErrors} ({Percentage(this.TotalRequests, this.ServerErrors)})"
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
