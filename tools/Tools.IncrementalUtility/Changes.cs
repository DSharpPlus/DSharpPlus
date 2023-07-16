// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Tools.IncrementalUtility;

/// <summary>
/// Contains the changes made to files since the last hashing.
/// </summary>
public sealed record Changes
{
    public required IEnumerable<string> Modified { get; init; }

    public required IEnumerable<string> Added { get; init; }

    public required IEnumerable<string> Removed { get; init; }
}
