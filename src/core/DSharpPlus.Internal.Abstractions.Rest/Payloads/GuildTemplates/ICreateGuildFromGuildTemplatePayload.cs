// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /guilds/templates/:template-code</c>.
/// </summary>
public interface ICreateGuildFromGuildTemplatePayload
{
    /// <summary>
    /// The name of the guild, 2 to 100 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The 128x128 icon for this guild.
    /// </summary>
    public Optional<InlineMediaData> Icon { get; }
}
