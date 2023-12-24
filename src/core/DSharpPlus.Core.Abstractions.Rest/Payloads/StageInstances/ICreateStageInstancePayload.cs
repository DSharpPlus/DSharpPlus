// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /stage-instances</c>.
/// </summary>
public interface ICreateStageInstancePayload
{
    /// <summary>
    /// The snowflake identifier of the parent stage channel.
    /// </summary>
    public Snowflake ChannelId { get; }

    /// <summary>
    /// The topic of the stage instance.
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// The privacy level of the stage instance.
    /// </summary>
    public Optional<DiscordStagePrivacyLevel> PrivacyLevel { get; }

    /// <summary>
    /// Indicates whether @everyone should be notified that a stage instance has started.
    /// </summary>
    public Optional<bool> SendStartNotification { get; }

    /// <summary>
    /// The snowflake identifier of the scheduled event associated with this instance.
    /// </summary>
    public Optional<Snowflake> GuildScheduledEventId { get; }
}
