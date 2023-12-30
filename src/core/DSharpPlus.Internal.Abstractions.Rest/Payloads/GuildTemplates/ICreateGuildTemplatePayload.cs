// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /guilds/:guild-id/templates</c>
/// </summary>
public interface ICreateGuildTemplatePayload
{
    /// <summary>
    /// The name of this template, up to 100 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description of this template, up to 120 characters.
    /// </summary>
    public Optional<string?> Description { get; }
}
