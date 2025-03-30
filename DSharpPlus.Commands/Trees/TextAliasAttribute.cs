using System;

namespace DSharpPlus.Commands.Trees;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public sealed class TextAliasAttribute(params string[] aliases) : Attribute
{
    public string[] Aliases { get; init; } = aliases;
}
