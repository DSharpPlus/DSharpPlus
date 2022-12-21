namespace DSharpPlus.Entities;

/// <remarks>
/// Once set to <see cref="Completed"/> or <see cref="Canceled"/>, the status can no longer be updated.
/// </remarks>
public enum DiscordGuildScheduledEventStatus
{
    Scheduled = 1,
    Active = 2,
    Completed = 3,
    Canceled = 4
}
