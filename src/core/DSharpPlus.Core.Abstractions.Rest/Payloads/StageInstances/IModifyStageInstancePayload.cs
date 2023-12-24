// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /stage-instances/:channel-id</c>.
/// </summary>
public interface IModifyStageInstancePayload
{
    /// <summary>
    /// The new topic for this stage instance.
    /// </summary>
    public Optional<string> Topic { get; }

    /// <summary>
    /// The new privacy level of the current stage.
    /// </summary>
    public Optional<DiscordStagePrivacyLevel> PrivacyLevel { get; }
}
