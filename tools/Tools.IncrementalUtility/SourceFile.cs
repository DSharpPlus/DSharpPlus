// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.IO;
using System.IO.Hashing;

namespace Tools.IncrementalUtility;

/// <summary>
/// Represents source file information and a hash indicating whether it has changed.
/// </summary>
/// <param name="path">The path to this file.</param>
public sealed class SourceFile
(
    string path
)
{
    /// <summary>
    /// Gets the current hash of this file.
    /// </summary>
    public ulong Hash { get; private set; } = 0;

    /// <summary>
    /// Loads the file and calculates its current hash. This may be called multiple times.
    /// </summary>
    public void Load()
    {
        XxHash3 hash = new();

        hash.Append(File.ReadAllBytes(path));

        this.Hash = hash.GetCurrentHashAsUInt64();
    }
}
