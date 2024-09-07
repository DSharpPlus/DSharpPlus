// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA1001 // yes, that type absolutely is disposable.

using System;
using System.Buffers;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.RuntimeServices.TextWriters;

/// <summary>
/// Provides a simple and efficient way to write indented UTF-16 text using pooled buffers.
/// </summary>
public record struct IndentedArrayPoolUtf16TextWriter : IDisposable
{
    private static ReadOnlySpan<char> SingleIndentation => "    ";
    private static ReadOnlySpan<char> DoubleIndentation => "        ";
    private static ReadOnlySpan<char> TripleIndentation => "            ";
    private static ReadOnlySpan<char> QuadrupleIndentation => "                ";
    private static ReadOnlySpan<char> DefaultNewline => "\r\n";

    private int currentIndentation;

    private readonly ArrayPoolBufferWriter<char> writer;

    public IndentedArrayPoolUtf16TextWriter()
        => this.writer = new();

    /// <summary>
    /// Increases the indentation of this text writer by one level.
    /// </summary>
    public void IncreaseIndentation() => this.currentIndentation++;

    /// <summary>
    /// Decreases the indentation of this text writer by one level.
    /// </summary>
    public void DecreaseIndentation() => this.currentIndentation--;

    /// <summary>
    /// Writes the provided text to the writer, without appending a newline.
    /// </summary>
    /// <param name="text">The text to append to this text writer.</param>
    /// <param name="literal">
    /// Indicates whether this is literal text and thus whether each individual line should be properly indented. Defaults
    /// to non-literal text where each line will be indented relative to the current writer indentation.
    /// </param>
    public readonly void Write(ReadOnlySpan<char> text, bool literal = false)
    {
        if (literal)
        {
            this.WriteCurrentIndentation();
            this.writer.Write(text);
            return;
        }

        while (text.Length > 0)
        {
            int newlineIndex = text.IndexOf(DefaultNewline);

            if (newlineIndex < 0)
            {
                this.WriteCurrentIndentation();
                this.writer.Write(text);
                break;
            }
            else
            {
                ReadOnlySpan<char> line = text[..newlineIndex];

                if (!line.IsEmpty)
                {
                    this.WriteCurrentIndentation();
                    this.writer.Write(line);
                }

                this.WriteLine();
                text = text[newlineIndex..];
                text = text[text.IndexOfAnyExcept("\r\n")..];
            }
        }
    }

    /// <summary>
    /// Writes the provided text to the writer, appending a newline at the end.
    /// </summary>
    /// <param name="text">The text to append to this text writer.</param>
    /// <param name="literal">
    /// Indicates whether this is literal text and thus whether each individual line should be properly indented. Defaults
    /// to non-literal text where each line will be indented relative to the current writer indentation.
    /// </param>
    public readonly void WriteLine(ReadOnlySpan<char> text, bool literal = false)
    {
        this.Write(text, literal);
        this.writer.Write(DefaultNewline);
    }

    /// <summary>
    /// Writes a newline to the writer.
    /// </summary>
    public readonly void WriteLine()
        => this.writer.Write(DefaultNewline);

    /// <summary>
    /// Ascertains that the writer ends on a newline and not any other character.
    /// </summary>
    public readonly void EnsureEndsInNewline()
    {
        if (this.writer.WrittenSpan is [.., '\n'])
        {
            return;
        }

        this.writer.Write(DefaultNewline);
    }

    /// <summary>
    /// Writes the current indentation to the writer using the cached indentation values.
    /// </summary>
    private readonly void WriteCurrentIndentation()
    {
        int quadrupleIndentationLevels = this.currentIndentation / 4;

        for (int i = 0; i < quadrupleIndentationLevels; i++)
        {
            this.writer.Write(QuadrupleIndentation);
        }

        switch (this.currentIndentation % 4)
        {
            case 3:
                this.writer.Write(TripleIndentation);
                break;

            case 2:
                this.writer.Write(DoubleIndentation);
                break;

            case 1:
                this.writer.Write(SingleIndentation);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Builds the current text writer into a string. Note that this operation is computationally expensive.
    /// </summary>
    /// <returns></returns>
    public override readonly string ToString()
        => new(this.writer.WrittenSpan);

    /// <inheritdoc/>
    public readonly void Dispose() => this.writer.Dispose();
}
