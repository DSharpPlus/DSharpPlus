using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Processors.SlashCommands.NamingPolicies;

/// <summary>
/// Transforms parameter names into snake_case.
/// </summary>
public class SnakeCaseNamingPolicy : IInteractionNamingPolicy
{
    /// <summary>
    /// Transforms the command name into it's snake_case equivalent.
    /// </summary>
    /// <inheritdoc />
    public string GetCommandName(Command command, CultureInfo culture)
    {
        string commandName = command.Attributes.FirstOrDefault(attribute => attribute is DisplayNameAttribute) is DisplayNameAttribute displayNameAttribute
            ? displayNameAttribute.DisplayName
            : command.Name;

        return string.IsNullOrWhiteSpace(commandName)
            ? throw new InvalidOperationException("Command name cannot be null or empty.")
            : TransformText(commandName, culture).ToString();
    }

    /// <summary>
    /// Transforms the parameter name into it's snake_case equivalent.
    /// </summary>
    /// <inheritdoc />
    public string GetParameterName(CommandParameter parameter, CultureInfo culture, int arrayIndex)
    {
        if (string.IsNullOrWhiteSpace(parameter.Name))
        {
            throw new InvalidOperationException("Parameter name cannot be null or empty.");
        }

        StringBuilder stringBuilder = TransformText(parameter.Name, culture);
        if (arrayIndex > -1)
        {
            stringBuilder.Append('_');
            stringBuilder.Append(arrayIndex);
        }

        return stringBuilder.ToString();
    }

    /// <inheritdoc />
    public StringBuilder TransformText(ReadOnlySpan<char> text, CultureInfo culture)
    {
        StringBuilder stringBuilder = new();
        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];

            // camelCase, PascalCase
            if (i != 0 && char.IsUpper(character))
            {
                stringBuilder.Append('_');
            }
            else if (character == '-')
            {
                stringBuilder.Append('_');
                continue;
            }

            stringBuilder.Append(char.ToLower(character, culture));
        }

        return stringBuilder;
    }
}
