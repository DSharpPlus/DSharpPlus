using System;

namespace DSharpPlus.Commands.ContextChecks;

/// <summary>
/// The types of code-formatted text to accept.
/// </summary>
[Flags]
public enum CodeType
{
    /// <summary>
    /// Accept inline code blocks - codeblocks that will not contain any newlines.
    /// </summary>
    Inline = 1 << 0,

    /// <summary>
    /// Accept codeblocks - codeblocks that will contain possibly multiple newlines.
    /// </summary>
    Codeblock = 1 << 1,

    /// <summary>
    /// Accept any type of code block.
    /// </summary>
    All = Inline | Codeblock
}
