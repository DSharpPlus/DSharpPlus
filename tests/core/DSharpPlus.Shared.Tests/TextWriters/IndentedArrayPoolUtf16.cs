// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;

using DSharpPlus.RuntimeServices.TextWriters;

namespace DSharpPlus.Shared.Tests.TextWriters;

public class IndentedArrayPoolUtf16
{
    [Test]
    public async Task TestIndentationIncrementDecrement()
    {
        using IndentedArrayPoolUtf16TextWriter writer = new();

        writer.WriteLine("{");
        writer.IncreaseIndentation();
        writer.WriteLine();
        writer.DecreaseIndentation();
        writer.WriteLine("}");

        await Assert.That(writer.ToString().Trim(' ', '\r', '\n')).IsEqualTo
        (
            """
            {

            }
            """
        );
    }

    [Test]
    public async Task TestIndentingWrite()
    {
        using IndentedArrayPoolUtf16TextWriter writer = new();

        writer.WriteLine("{");
        writer.IncreaseIndentation();
        writer.WriteLine("\"stuff\":\r\n1", false);
        writer.DecreaseIndentation();
        writer.WriteLine("}");

        await Assert.That(writer.ToString().Trim(' ', '\r', '\n')).IsEqualTo
        (
            """
            {
                "stuff":
                1
            }
            """
        );
    }

    [Test]
    public async Task TestLiteralPreservingWrite()
    {
        using IndentedArrayPoolUtf16TextWriter writer = new();

        writer.WriteLine("{");
        writer.IncreaseIndentation();
        writer.WriteLine("\"stuff\":\r\n1", true);
        writer.DecreaseIndentation();
        writer.WriteLine("}");

        await Assert.That(writer.ToString().Trim(' ', '\r', '\n')).IsEqualTo
        (
            """
            {
                "stuff":
            1
            }
            """
        );
    }
}
