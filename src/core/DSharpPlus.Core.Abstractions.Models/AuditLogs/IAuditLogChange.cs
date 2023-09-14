// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a single logged change made to an entity. Cast to a more specific type to access the changes made.
/// </summary>
public interface IAuditLogChange
{
    /// <summary>
    /// The new value of this field.
    /// </summary>
    public Optional<string> NewValue { get; }

    /// <summary>
    /// The old value of this field.
    /// </summary>
    public Optional<string> OldValue { get; }

    /// <summary>
    /// The name of the changed field, with a few exceptions: see
    /// <seealso href="https://discord.com/developers/docs/resources/audit-log#audit-log-change-object-audit-log-change-exceptions">
    /// the documentation</seealso>.
    /// </summary>
    public string Key { get; }
}
