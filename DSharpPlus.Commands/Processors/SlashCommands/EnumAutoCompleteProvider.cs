using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace DSharpPlus.Commands.Processors.SlashCommands;

/// <summary>
/// Provides a cached list of choices for the <typeparamref name="T"/> enum type.
/// </summary>
/// <typeparam name="T">The enum type to provide choices for.</typeparam>
public class EnumAutoCompleteProvider<T> : IAutoCompleteProvider where T : struct, Enum
{
    private static readonly FrozenDictionary<string, object> choices;

    static EnumAutoCompleteProvider()
    {
        Dictionary<string, object> choiceDictionary = [];
        foreach (FieldInfo fieldInfo in typeof(T).GetFields())
        {
            if (fieldInfo.IsSpecialName || !fieldInfo.IsStatic)
            {
                continue;
            }

            object value = fieldInfo.GetValue(null) ?? throw new InvalidOperationException($"Enum '{typeof(T).Name}' field '{fieldInfo.Name}' returned a null value.");
            choiceDictionary.Add(fieldInfo.GetCustomAttribute<ChoiceDisplayNameAttribute>() is ChoiceDisplayNameAttribute displayNameAttribute ? displayNameAttribute.DisplayName : fieldInfo.Name, value);
        }

        choices = choiceDictionary.ToFrozenDictionary();
    }

    /// <inheritdoc />
    public ValueTask<IReadOnlyDictionary<string, object>> AutoCompleteAsync(AutoCompleteContext context)
    {
        if (string.IsNullOrWhiteSpace(context.UserInput))
        {
            return ValueTask.FromResult<IReadOnlyDictionary<string, object>>(choices);
        }

        // Find the choices that start with the provided input
        Dictionary<string, object> matchingChoices = [];
        foreach (KeyValuePair<string, object> choice in choices)
        {
            // Use InvariantCultureIgnoreCase so characters from other languages (such as German's ß) are treated as equal to their base characters (ss)
            if (choice.Key.StartsWith(context.UserInput, StringComparison.InvariantCultureIgnoreCase))
            {
                matchingChoices.Add(choice.Key, choice.Value);
            }
        }

        return ValueTask.FromResult<IReadOnlyDictionary<string, object>>(matchingChoices);
    }
}
