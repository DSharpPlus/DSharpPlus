#pragma warning disable IDE0040 // this shouldn't even be reported on a partial like this

using System;
using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.Entities;

partial class DiscordApplicationCommand
{
    /// <summary>
    /// Implements a weak-equality check for application commands, treating null and default values the same
    /// and only comparing fields known locally.
    /// </summary>
    public bool WeakEquals(DiscordApplicationCommand other)
    {
        return this.Name == other.Name
            && this.Description == other.Description
            && (this.DefaultMemberPermissions ?? DiscordPermissions.None) 
                == (other.DefaultMemberPermissions ?? DiscordPermissions.None)
            && (this.NSFW ?? false) == (other.NSFW ?? false)
            && this.Type == other.Type
            && IntegrationTypesMatch(this.IntegrationTypes, other.IntegrationTypes)
            && EnumListsMatch(this.Contexts, other.Contexts)
            && LocalizationsMatch(this.NameLocalizations, other.NameLocalizations)
            && LocalizationsMatch(this.DescriptionLocalizations, other.DescriptionLocalizations)
            && OptionsMatch(this.Options, other.Options);
    }

    private static bool LocalizationsMatch
    (
        IReadOnlyDictionary<string, string> a,
        IReadOnlyDictionary<string, string> b
    )
    {
        // if both are null or empty, they are equivalent
        if (a is null or { Count: 0 } && b is null or { Count: 0 })
        {
            return true;
        }

        // if one of the two is null, but not both (as per above), they are not equivalent
        if (a is null || b is null)
        {
            return false;
        }

        // they are both not-null, so let's go check
        // if they aren't evenly long, they by necessity cannot be equivalent
        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (KeyValuePair<string, string> val in a)
        {
            if (!b.TryGetValue(val.Key, out string? remoteValue) || val.Value != remoteValue)
            {
                return false;
            }
        }

        return true;
    }

    private static bool OptionsMatch
    (
        IReadOnlyList<DiscordApplicationCommandOption>? a,
        IReadOnlyList<DiscordApplicationCommandOption>? b
    )
    {
        // if both are null or empty, they are equivalent
        if (a is null or { Count: 0 } && b is null or { Count: 0 })
        {
            return true;
        }

        // if one of the two is null, but not both (as per above), they are not equivalent
        if (a is null || b is null)
        {
            return false;
        }

        // they are both not-null, so let's go check
        // if they aren't evenly long, they by necessity cannot be equivalent
        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (DiscordApplicationCommandOption option in a)
        {
            DiscordApplicationCommandOption other;

            if ((other = b.SingleOrDefault(x => x.Name == option.Name)) is not null)
            {
                // we have a remote record, check whether its surface matches
                bool match = option.Description == other.Description
                    && option.Type == other.Type
                    && (option.AutoComplete ?? false) == (other.AutoComplete ?? false)
                    && (option.MaxLength ?? 4000) == (other.MaxLength ?? 4000)
                    && (option.MinLength ?? 0) == (other.MinLength ?? 0)
                    && (option.Required ?? true) == (other.Required ?? true)
                    && (option.AutoComplete ?? false) == (other.AutoComplete ?? false)
                    && BoxedIntegersMatch(option.MinValue, other.MinValue)
                    && BoxedIntegersMatch(option.MaxValue, other.MaxValue)
                    && EnumListsMatch(option.ChannelTypes, other.ChannelTypes)
                    && ChoicesMatch(option.Choices, other.Choices)
                    && LocalizationsMatch(option.NameLocalizations, other.NameLocalizations)
                    && LocalizationsMatch(option.DescriptionLocalizations, other.DescriptionLocalizations)
                    && OptionsMatch(option.Options, other.Options);

                if (!match)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private static bool ChoicesMatch
    (
        IReadOnlyList<DiscordApplicationCommandOptionChoice>? a,
        IReadOnlyList<DiscordApplicationCommandOptionChoice>? b
    )
    {
        if (a is null or { Count: 0 } && b is null or { Count: 0 })
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (DiscordApplicationCommandOptionChoice choice in a)
        {
            if (!b.Any(c =>
                {
                    return c.Name == choice.Name 
                        && ChoiceValuesMatch(c, choice)
                        && LocalizationsMatch(c.NameLocalizations, choice.NameLocalizations);
                })
            )
            {
                return false;
            }
        }

        return true;
    }

    private static bool ChoiceValuesMatch(object a, object b)
    {
        return a switch
        {
            int value => b is int other && value == other,
            long value => b is long other && value == other,
            double value => b is double other && value == other,
            string value => b is string other && value == other,
            _ => false
        };
    }

    private static bool IntegrationTypesMatch
    (
        IReadOnlyList<DiscordApplicationIntegrationType>? a,
        IReadOnlyList<DiscordApplicationIntegrationType>? b
    )
    {
        if 
        (
            a is null or { Count: 0 } or [DiscordApplicationIntegrationType.GuildInstall]
            && b is null or { Count: 0 } or [DiscordApplicationIntegrationType.GuildInstall]
        )
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (DiscordApplicationIntegrationType type in a)
        {
            if (!b.Contains(type))
            {
                return false;
            }
        }

        return true;
    }

    private static bool EnumListsMatch<TEnum>
    (
        IReadOnlyList<TEnum>? a,
        IReadOnlyList<TEnum>? b
    )
        where TEnum : Enum
    {
        if (a is null or { Count: 0 } && b is null or { Count: 0 })
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (TEnum type in a)
        {
            if (!b.Contains(type))
            {
                return false;
            }
        }

        return true;
    }

    private static bool BoxedIntegersMatch(object? a, object? b)
    {
        return a switch
        {
            sbyte value => b is sbyte other && value == other,
            byte value => b is byte other && value == other,
            short value => b is short other && value == other,
            ushort value => b is ushort other && value == other,
            int value => b is int other && value == other,
            uint value => b is uint other && value == other,
            long value => b is long other && value == other,
            ulong value => b is ulong other && value == other,
            float value => b is float other && value == other,
            double value => b is double other && value == other,
            null => b is null || IsZero(b),
            _ => false
        };
    }

    private static bool IsZero(object boxed)
    {
        return boxed switch
        {
            sbyte value when value is 0 => true,
            byte value when value is 0 => true,
            short value when value is 0 => true,
            ushort value when value is 0 => true,
            int value when value is 0 => true,
            uint value when value is 0 => true,
            long value when value is 0 => true,
            ulong value when value is 0 => true,
            float value when value is 0.0f => true,
            double value when value is 0.0 => true,
            _ => false
        };
    }
}
