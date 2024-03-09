using System;

namespace DSharpPlus.Commands.ContextChecks;

[Flags]
public enum CodeType
{
    Inline = 1 << 0,
    Codeblock = 1 << 1,
    All = Inline | Codeblock
}
