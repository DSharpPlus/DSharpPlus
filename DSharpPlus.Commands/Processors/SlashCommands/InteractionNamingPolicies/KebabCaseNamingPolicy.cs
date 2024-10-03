using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Processors.SlashCommands.ParameterNamingPolicies;

/// <summary>
/// Transforms parameter names into kebab-case.
/// </summary>
public sealed class KebabCaseNamingPolicy : IInteractionNamingPolicy
{
    /// <summary>
    /// Transforms the command name into it's kebab-case equivalent.
    /// </summary>
    /// <inheritdoc />
    public string GetCommandName(Command command)
    {
        string commandName = command.Attributes.FirstOrDefault(attribute => attribute is DisplayNameAttribute) is DisplayNameAttribute displayNameAttribute
            ? displayNameAttribute.DisplayName
            : command.Name;

        return string.IsNullOrWhiteSpace(commandName)
            ? throw new InvalidOperationException("Command name cannot be null or empty.")
            : TransformText(commandName).ToString();
    }

    /// <summary>
    /// Transforms the parameter name into it's kebab-case equivalent.
    /// </summary>
    /// <inheritdoc />
    public string GetParameterName(CommandParameter parameter, int arrayIndex)
    {
        if (string.IsNullOrWhiteSpace(parameter.Name))
        {
            throw new InvalidOperationException("Parameter name cannot be null or empty.");
        }

        StringBuilder stringBuilder = TransformText(parameter.Name);
        if (arrayIndex > -1)
        {
            stringBuilder.Append('-');
            stringBuilder.Append(arrayIndex);
        }

        return stringBuilder.ToString();
    }

    /// <inheritdoc />
    public StringBuilder TransformText(ReadOnlySpan<char> text)
    {
        StringBuilder stringBuilder = new();
        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];

            // camelCase, PascalCase
            if (i != 0 && char.IsUpper(character))
            {
                stringBuilder.Append('-');
            }
            else if (character == '_')
            {
                stringBuilder.Append('-');
                continue;
            }

            stringBuilder.Append(char.ToLowerInvariant(character));
        }

        return stringBuilder;
    }
}
