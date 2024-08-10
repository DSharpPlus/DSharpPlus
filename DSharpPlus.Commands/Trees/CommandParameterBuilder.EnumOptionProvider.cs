using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace DSharpPlus.Commands.Trees;

public partial class CommandParameterBuilder
{
    public class EnumOptionProvider : IChoiceProvider
    {
        public ValueTask<IReadOnlyDictionary<string, object>> ProvideAsync(CommandParameter parameter)
        {
            List<string> enumNames = [];
            Type effectiveType = Nullable.GetUnderlyingType(parameter.Type) ?? parameter.Type;

            foreach (FieldInfo fieldInfo in effectiveType.GetFields())
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
            Array enumValues = Enum.GetValuesAsUnderlyingType(effectiveType);
            for (int i = 0; i < enumNames.Count; i++)
            {
                string? value = enumValues.GetValue(i)?.ToString() ?? throw new InvalidOperationException($"Failed to get the value of the enum {parameter.Type.Name} for element {enumNames[i]}");
                choices.Add(enumNames[i], value.ToString());
            }

            return ValueTask.FromResult<IReadOnlyDictionary<string, object>>(choices);
        }
    }
}
