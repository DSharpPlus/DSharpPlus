namespace DSharpPlus.Entities;

using System.Collections.Generic;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

/// <summary>
/// Represents a user presence.
/// </summary>
public sealed class DiscordPresence
{
    [JsonIgnore]
    internal DiscordClient Discord { get; set; }

    // "The user object within this event can be partial, the only field which must be sent is the id field, everything else is optional."
    [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
    internal TransportUser InternalUser { get; set; }

    /// <summary>
    /// Gets the user that owns this presence.
    /// </summary>
    [JsonIgnore]
    public DiscordUser User
        => Discord.GetCachedOrEmptyUserInternal(InternalUser.Id);

    /// <summary>
    /// Gets the user's current activity.
    /// </summary>
    [JsonIgnore]
    public DiscordActivity Activity { get; internal set; }

    internal TransportActivity RawActivity { get; set; }

    /// <summary>
    /// Gets the user's current activities.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordActivity> Activities => _internalActivities;

    [JsonIgnore]
    internal DiscordActivity[] _internalActivities;

    [JsonProperty("activities", NullValueHandling = NullValueHandling.Ignore)]
    internal TransportActivity[] RawActivities { get; set; }

    /// <summary>
    /// Gets this user's status.
    /// </summary>
    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUserStatus Status { get; internal set; }

    [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong GuildId { get; set; }

    /// <summary>
    /// Gets the guild for which this presence was set.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild Guild
        => GuildId != 0 ? Discord._guilds[GuildId] : null;

    /// <summary>
    /// Gets this user's platform-dependent status.
    /// </summary>
    [JsonProperty("client_status", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordClientStatus ClientStatus { get; internal set; }

    internal DiscordPresence() { }

    internal DiscordPresence(DiscordPresence other)
    {
        Discord = other.Discord;
        if (other.Activity != null)
        {
            Activity = new DiscordActivity(other.Activity);
        }

        if (other.Activity != null)
        {
            RawActivity = new TransportActivity(Activity);
        }

        _internalActivities = (DiscordActivity[])other._internalActivities?.Clone();
        RawActivities = (TransportActivity[])other.RawActivities?.Clone();
        Status = other.Status;
        InternalUser = new TransportUser(other.InternalUser);
    }
}

public sealed class DiscordClientStatus
{
    /// <summary>
    /// Gets the user's status set for an active desktop (Windows, Linux, Mac) application session.
    /// </summary>
    [JsonProperty("desktop", NullValueHandling = NullValueHandling.Ignore)]
    public Optional<DiscordUserStatus> Desktop { get; internal set; }

    /// <summary>
    /// Gets the user's status set for an active mobile (iOS, Android) application session.
    /// </summary>
    [JsonProperty("mobile", NullValueHandling = NullValueHandling.Ignore)]
    public Optional<DiscordUserStatus> Mobile { get; internal set; }

    /// <summary>
    /// Gets the user's status set for an active web (browser, bot account) application session.
    /// </summary>
    [JsonProperty("web", NullValueHandling = NullValueHandling.Ignore)]
    public Optional<DiscordUserStatus> Web { get; internal set; }
}
