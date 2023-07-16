// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Buffers;
using System.IO;
using System.IO.Hashing;
using System.IO.Pipelines;

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

        using FileStream file = new(path, FileMode.Open, FileAccess.Read);
        PipeReader pipe = PipeReader.Create(file);

        while (true)
        {
            if (!pipe.TryRead(out ReadResult result))
            {
                break;
            }

            ReadOnlySequence<byte> buffer = result.Buffer;
            SequenceReader<byte> reader = new(buffer);

            // read line by line, because that guarantees that the returned sequence is in one piece.
            // if we called .FirstSpan on the buffer we might risk missing data for large files
            while (reader.TryReadTo(out ReadOnlySequence<byte> sequence, 0x0a))
            {
                hash.Append(sequence.FirstSpan);
            }
        }

        this.Hash = hash.GetCurrentHashAsUInt64();
    }
}
