using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the status information of job processing the update of invite target users.
/// </summary>
public readonly struct DiscordTargetUsersJobStatus
{
    /// <summary>
    /// Gets the current status of the job.
    /// </summary>
    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
    public JobStatus Status { get; init; }

    /// <summary>
    /// Gets the total number of users being processed in this job.
    /// </summary>
    [JsonProperty("total_users", NullValueHandling = NullValueHandling.Ignore)]
    public int TotalUsers { get; init; }

    /// <summary>
    /// Gets the amount of users already processed.
    /// </summary>
    [JsonProperty("processed_users", NullValueHandling = NullValueHandling.Ignore)]
    public int ProcessedUsers { get; init; }

    /// <summary>
    /// Gets when the job was created.
    /// </summary>
    [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets when the job was completed.
    /// </summary>
    [JsonProperty("completed_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>
    /// Gets the error message if the job failed.
    /// </summary>
    [JsonProperty("error_message", NullValueHandling = NullValueHandling.Ignore)]
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Represents the status of a job.
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// The default value.
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// The job is still being processed.
        /// </summary>
        Processing = 1,
        /// <summary>
        /// The job has been completed successfully.
        /// </summary>
        Completed = 2,
        /// <summary>
        /// The job has failed.
        /// </summary>
        Failed = 3
    }
}
