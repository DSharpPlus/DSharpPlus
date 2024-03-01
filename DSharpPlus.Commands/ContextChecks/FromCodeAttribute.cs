namespace DSharpPlus.Commands.ContextChecks;

using System;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromCodeAttribute(CodeType codeType = CodeType.All) : Attribute
{
    public CodeType CodeType { get; init; } = codeType;
}

[Flags]
public enum CodeType
{
    Inline = 1 << 0,
    Codeblock = 1 << 1,
    All = Inline | Codeblock
}
