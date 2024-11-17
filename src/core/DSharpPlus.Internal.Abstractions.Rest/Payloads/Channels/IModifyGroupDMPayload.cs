// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /channels/:channel-id</c>.
/// </summary>
public interface IModifyGroupDMPayload
{
    /// <summary>
    /// The name of this group DM channel. 
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The icon of this group DM channel.
    /// </summary>
    public Optional<InlineMediaData> Icon { get; }
}
