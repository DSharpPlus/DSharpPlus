using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the status information of job processing the update of invite target users.
/// </summary>
public sealed class DiscordInviteTargetUsersJobStatus
{
    /// <summary>
    /// Gets the current status of the job.
    /// </summary>
    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordInviteTargetUserStatus Status { get; internal set; }

    /// <summary>
    /// Gets the total number of users being processed in this job.
    /// </summary>
    [JsonProperty("total_users", NullValueHandling = NullValueHandling.Ignore)]
    public int TotalUsers { get; internal set; }

    /// <summary>
    /// Gets the amount of users already processed.
    /// </summary>
    [JsonProperty("processed_users", NullValueHandling = NullValueHandling.Ignore)]
    public int ProcessedUsers { get; internal set; }

    /// <summary>
    /// Gets when the job was created.
    /// </summary>
    [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset CreatedAt { get; internal set; }

    /// <summary>
    /// Gets when the job was completed.
    /// </summary>
    [JsonProperty("completed_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? CompletedAt { get; internal set; }

    /// <summary>
    /// Gets the error message if the job failed.
    /// </summary>
    [JsonProperty("error_message", NullValueHandling = NullValueHandling.Ignore)]
    public string? ErrorMessage { get; internal set; }
}
