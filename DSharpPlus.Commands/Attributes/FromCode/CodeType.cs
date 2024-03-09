using System;

namespace DSharpPlus.Commands.Attributes;

[Flags]
public enum CodeType
{
    Inline = 1 << 0,
    Codeblock = 1 << 1,
    All = Inline | Codeblock
}
