using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Processors.SlashCommands;

public class EnumOptionChoiceProvider : IChoiceProvider
{
    public ValueTask<IReadOnlyDictionary<string, object>> ProvideAsync(CommandParameter parameter)
    {
        List<string> enumNames = [];
        Type baseType = IArgumentConverter.GetConverterFriendlyBaseType(parameter.Type);

        foreach (FieldInfo fieldInfo in baseType.GetFields())
        {
            if (fieldInfo.IsSpecialName || !fieldInfo.IsStatic)
            {
                continue;
            }
            else if (fieldInfo.GetCustomAttribute<ChoiceDisplayNameAttribute>() is ChoiceDisplayNameAttribute displayNameAttribute)
            {
                enumNames.Add(displayNameAttribute.DisplayName);
            }
            else
            {
                enumNames.Add(fieldInfo.Name);
            }
        }

        Dictionary<string, object> choices = [];
        Array enumValues = Enum.GetValuesAsUnderlyingType(baseType);
        for (int i = 0; i < enumNames.Count; i++)
        {
            string? value = enumValues.GetValue(i)?.ToString() ?? throw new InvalidOperationException($"Failed to get the value of the enum {parameter.Type.Name} for element {enumNames[i]}");
            choices.Add(enumNames[i], value.ToString());
        }

        return ValueTask.FromResult<IReadOnlyDictionary<string, object>>(choices);
    }
}
