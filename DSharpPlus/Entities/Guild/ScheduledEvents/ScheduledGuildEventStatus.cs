namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the status of a <see cref="DiscordScheduledGuildEvent"/>.
    /// </summary>
    public enum ScheduledGuildEventStatus
    {
        /// <summary>
        /// This event is scheduled.
        /// </summary>
        Scheduled = 1,
        /// <summary>
        /// This event is currently running.
        /// </summary>
        Active = 2,

        /// <summary>
        /// This event has finished running.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// This event has been cancelled.
        /// </summary>
        Cancelled = 4
    }
}
