// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;

using DSharpPlus.Etf.Serialization;

using Xunit;

namespace DSharpPlus.Etf.Tests.Serialization.EtfReaderTests;

/// <summary>
/// Contains tests to see whether we complain correctly upon invalid data.
/// </summary>
public class ValidationTests
{
    private static readonly byte[] compressedPayload = [0x83, 0x50];

    /// <summary>
    /// Verifies whether the EtfReader correctly rejects a compressed term payload.
    /// </summary>
    [Fact]
    public void TestFailingToReadCompressedPayload()
    {
        try
        {
            ReadOnlySpan<byte> span = [.. compressedPayload];
            EtfReader reader = new(span);

            // this should be unreachable
            Assert.True(false);
        }
        catch (InvalidDataException)
        {
            // correct exception thrown
            Assert.True(true);
        }
        catch
        {
            // specifically only that exception should be thrown
            Assert.True(false);
        }
    }
}
