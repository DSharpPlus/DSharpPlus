using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands;

public class EnumOptionChoiceProvider : IChoiceProvider
{
    public ValueTask<IEnumerable<DiscordApplicationCommandOptionChoice>> ProvideAsync(CommandParameter parameter)
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

        // We can use `enumNames.Count` here since `SlashCommandProcessor.Registration.ConfigureCommands`
        // will use autocomplete for enums that have more than 25 values. If the user decides to use
        // this class manually, `IChoiceProvider.ProvideAsync` will be called and truncate the list
        // automatically, warning the user that the list is too long.
        List<DiscordApplicationCommandOptionChoice> choices = new(enumNames.Count);
        Array enumValues = Enum.GetValuesAsUnderlyingType(baseType);
        for (int i = 0; i < enumNames.Count; i++)
        {
            object? obj = enumValues.GetValue(i);
            choices.Add(obj switch
            {
                null => throw new InvalidOperationException($"Failed to get the value of the enum {parameter.Type.Name} for element {enumNames[i]}"),
                bool value => new DiscordApplicationCommandOptionChoice(enumNames[i], value ? 1 : 0),
                byte value => new DiscordApplicationCommandOptionChoice(enumNames[i], value),
                sbyte value => new DiscordApplicationCommandOptionChoice(enumNames[i], value),
                short value => new DiscordApplicationCommandOptionChoice(enumNames[i], value),
                ushort value => new DiscordApplicationCommandOptionChoice(enumNames[i], value),
                int value => new DiscordApplicationCommandOptionChoice(enumNames[i], value),
                uint value => new DiscordApplicationCommandOptionChoice(enumNames[i], value),
                long value => new DiscordApplicationCommandOptionChoice(enumNames[i], value),
                ulong value => new DiscordApplicationCommandOptionChoice(enumNames[i], value),
                float value => new DiscordApplicationCommandOptionChoice(enumNames[i], value),
                double value => new DiscordApplicationCommandOptionChoice(enumNames[i], value),
                _ => new DiscordApplicationCommandOptionChoice(enumNames[i], obj.ToString()!)
            });
        }

        return ValueTask.FromResult<IEnumerable<DiscordApplicationCommandOptionChoice>>(choices);
    }
}
