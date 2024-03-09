using System;

namespace DSharpPlus.Commands.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed partial class FromCodeAttribute : Attribute
{
    public CodeType CodeType { get; init; }

    public FromCodeAttribute(CodeType codeType = CodeType.All) => this.CodeType = codeType;
}
