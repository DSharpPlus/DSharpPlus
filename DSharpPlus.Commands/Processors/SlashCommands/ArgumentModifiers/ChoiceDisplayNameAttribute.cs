using System;

namespace DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

/// <summary>
/// Used to annotate enum members with a display name for the built-in choice provider.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ChoiceDisplayNameAttribute(string name) : Attribute
{
    public string DisplayName { get; set; } = name;
}