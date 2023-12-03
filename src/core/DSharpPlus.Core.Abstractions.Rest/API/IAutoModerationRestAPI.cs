// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Core.Abstractions.Models;

using Remora.Results;

namespace DSharpPlus.Core.Abstractions.Rest.API;

/// <summary>
/// Provides access to managing the built-in auto moderator via the API.
/// </summary>
public interface IAutoModerationRestAPI
{
    /// <summary>
    /// Fetches the auto moderation rules in the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IAutoModerationRule>>> ListAutoModerationRulesAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Fetches the specified auto moderation rule belonging to the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild owning the rule.</param>
    /// <param name="ruleId">The snowflake identifier of the rule to fetch.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IAutoModerationRule>> GetAutoModerationRuleAsync
    (
        Snowflake guildId,
        Snowflake ruleId,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
