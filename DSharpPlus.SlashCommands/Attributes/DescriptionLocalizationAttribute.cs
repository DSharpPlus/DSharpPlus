using System;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Specifies a locale for a slash command description. The longest description is the one that counts toward character limits.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class DescriptionLocalizationAttribute : Attribute
{
    public string Locale { get; }

    public string Description { get; }

    public DescriptionLocalizationAttribute(Localization locale, string description)
    {
        this.Description = description;
        this.Locale = LocaleHelper.LocaleToStrings[locale];
    }
}
