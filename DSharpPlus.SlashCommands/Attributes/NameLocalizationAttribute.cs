namespace DSharpPlus.SlashCommands;

using System;

/// <summary>
/// Specifies a locale for a slash command name. The longest name is the name that counts toward character limits.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class NameLocalizationAttribute : Attribute
{
    public string Locale { get; }

    public string Name { get; }

    public NameLocalizationAttribute(Localization locale, string name)
    {
        Name = name;
        Locale = LocaleHelper.LocaleToStrings[locale];
    }
}
