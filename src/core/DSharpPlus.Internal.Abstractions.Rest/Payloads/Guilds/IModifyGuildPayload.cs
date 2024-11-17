// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /guilds/:guild-id</c>.
/// </summary>
public interface IModifyGuildPayload
{
    /// <summary>
    /// The new name for this guild.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The new verification level for this guild.
    /// </summary>
    public Optional<DiscordVerificationLevel?> VerificationLevel { get; }

    /// <summary>
    /// The new default message notification level for this guild.
    /// </summary>
    public Optional<DiscordMessageNotificationLevel?> DefaultMessageNotifications { get; }

    /// <summary>
    /// The new explicit content filter level for this guild.
    /// </summary>
    public Optional<DiscordExplicitContentFilterLevel?> ExplicitContentFilter { get; }

    /// <summary>
    /// The new snowflake identifier of the AFK channel of this guild.
    /// </summary>
    public Optional<Snowflake?> AfkChannelId { get; }

    /// <summary>
    /// The new AFK timeout for this guild.
    /// </summary>
    public Optional<int> AfkTimeout { get; }

    /// <summary>
    /// The new icon for this guild.
    /// </summary>
    public Optional<InlineMediaData?> Icon { get; }

    /// <summary>
    /// The snowflake identifier of this guild's new owner. Used to transfer guild ownership.
    /// </summary>
    public Optional<Snowflake> OwnerId { get; }

    /// <summary>
    /// The new splash for this guild.
    /// </summary>
    public Optional<InlineMediaData?> Splash { get; }

    /// <summary>
    /// The new guild discovery splash for this guild.
    /// </summary>
    public Optional<InlineMediaData?> DiscoverySplash { get; }

    /// <summary>
    /// The new banner for this guild.
    /// </summary>
    public Optional<InlineMediaData?> Banner { get; }

    /// <summary>
    /// The snowflake identifier of the new system channel.
    /// </summary>
    public Optional<Snowflake?> SystemChannelId { get; }

    /// <summary>
    /// The new system channel flags for this guild.
    /// </summary>
    public Optional<DiscordSystemChannelFlags> SystemChannelFlags { get; }

    /// <summary>
    /// The snowflake identifier of the new rules channel.
    /// </summary>
    public Optional<Snowflake?> RulesChannelId { get; }

    /// <summary>
    /// The snowflake identifier of the new public update channel.
    /// </summary>
    public Optional<Snowflake?> PublicUpdatesChannelId { get; }

    /// <summary>
    /// The new preferred locale for this community guild.
    /// </summary>
    public Optional<string?> PreferredLocale { get; }

    /// <summary>
    /// The new enabled guild features for this guild.
    /// </summary>
    public Optional<IReadOnlyList<string>> Features { get; }

    /// <summary>
    /// The new description for this guild, if it is discoverable.
    /// </summary>
    public Optional<string?> Description { get; }

    /// <summary>
    /// Indicates whether the guild should have a boost progress bar.
    /// </summary>
    public Optional<bool> PremiumProgressBarEnabled { get; }

    /// <summary>
    /// The snowflake identifier of the new safety alerts channel.
    /// </summary>
    public Optional<Snowflake?> SafetyAlertsChannelId { get; }
}
