// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.RuntimeServices.TextWriters;

using Xunit;

namespace DSharpPlus.Shared.Tests.TextWriters;

public class IndentedArrayPoolUtf16
{
    [Fact]
    public void TestIndentationIncrementDecrement()
    {
        using IndentedArrayPoolUtf16TextWriter writer = new();

        writer.WriteLine("{");
        writer.IncreaseIndentation();
        writer.WriteLine();
        writer.DecreaseIndentation();
        writer.WriteLine("}");

        Assert.Equal
        (
            """
            {

            }
            """,
            writer.ToString().Trim(' ', '\r', 'n')
        );
    }

    [Fact]
    public void TestIndentingWrite()
    {
        using IndentedArrayPoolUtf16TextWriter writer = new();

        writer.WriteLine("{");
        writer.IncreaseIndentation();
        writer.WriteLine("\"stuff\":\r\n1", false);
        writer.DecreaseIndentation();
        writer.WriteLine("}");

        Assert.Equal
        (
            """
            {
                "stuff":
                1
            }
            """,
            writer.ToString().Trim(' ', '\r', 'n')
        );
    }

    [Fact]
    public void TestLiteralPreservingWrite()
    {
        using IndentedArrayPoolUtf16TextWriter writer = new();

        writer.WriteLine("{");
        writer.IncreaseIndentation();
        writer.WriteLine("\"stuff\":\r\n1", true);
        writer.DecreaseIndentation();
        writer.WriteLine("}");

        Assert.Equal
        (
            """
            {
                "stuff":
            1
            }
            """,
            writer.ToString().Trim(' ', '\r', 'n')
        );
    }
}
