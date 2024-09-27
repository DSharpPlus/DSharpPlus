using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands;

/// <summary>
/// Provides a cached list of choices for the <typeparamref name="T"/> enum type.
/// </summary>
/// <typeparam name="T">The enum type to provide choices for.</typeparam>
public class EnumAutoCompleteProvider<T> : IAutoCompleteProvider where T : struct, Enum
{
    private static readonly DiscordAutoCompleteChoice[] choices;

    static EnumAutoCompleteProvider()
    {
        List<DiscordAutoCompleteChoice> choiceList = [];
        foreach (FieldInfo fieldInfo in typeof(T).GetFields())
        {
            if (fieldInfo.IsSpecialName || !fieldInfo.IsStatic)
            {
                continue;
            }

            // Add support for ChoiceDisplayNameAttribute
            string displayName = fieldInfo.GetCustomAttribute<ChoiceDisplayNameAttribute>() is ChoiceDisplayNameAttribute displayNameAttribute ? displayNameAttribute.DisplayName : fieldInfo.Name;
            object? obj = fieldInfo.GetValue(null);
            if (obj is not T enumValue)
            {
                // Hey what the fuck
                continue;
            }

            // Put ulong as a string, bool, byte, short and int as int, uint and long as long.
            choiceList.Add(Convert.ChangeType(obj, Enum.GetUnderlyingType(typeof(T)), CultureInfo.InvariantCulture) switch
            {
                bool value => new DiscordAutoCompleteChoice(displayName, value ? 1 : 0),
                byte value => new DiscordAutoCompleteChoice(displayName, value),
                sbyte value => new DiscordAutoCompleteChoice(displayName, value),
                short value => new DiscordAutoCompleteChoice(displayName, value),
                ushort value => new DiscordAutoCompleteChoice(displayName, value),
                int value => new DiscordAutoCompleteChoice(displayName, value),
                uint value => new DiscordAutoCompleteChoice(displayName, value),
                long value => new DiscordAutoCompleteChoice(displayName, value),
                ulong value => new DiscordAutoCompleteChoice(displayName, value),
                double value => new DiscordAutoCompleteChoice(displayName, value),
                float value => new DiscordAutoCompleteChoice(displayName, value),
                _ => throw new UnreachableException($"Unknown enum base type encountered: {obj.GetType()}")
            });
        }

        choices = [.. choiceList];
    }

    /// <inheritdoc />
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        List<DiscordAutoCompleteChoice> results = [];
        foreach (DiscordAutoCompleteChoice choice in choices)
        {
            if (choice.Name.Contains(context.UserInput ?? "", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(choice);
                if (results.Count == 25)
                {
                    break;
                }
            }
        }

        return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(results);
    }
}
