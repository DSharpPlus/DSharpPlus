using System;
using System.Collections;
using System.Globalization;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Processors.SlashCommands.NamingPolicies;

/// <summary>
/// Represents a policy for naming parameters. This is used to determine the
/// name of the parameter when registering or receiving interaction data.
/// </summary>
public interface IInteractionNamingPolicy
{
    /// <summary>
    /// Transforms the parameter name into the name that should be used for the interaction data.
    /// </summary>
    /// <param name="parameter">The parameter being transformed.</param>
    /// <param name="culture">The culture to use for the transformation.</param>
    /// <param name="arrayIndex">
    /// If this parameter is part of an <see cref="IEnumerable" />, the index of the parameter.
    /// The value will be -1 if this parameter is not part of an <see cref="IEnumerable" />.
    /// </param>
    /// <returns>The name that should be used for the interaction data.</returns>
    string GetParameterName(CommandParameter parameter, CultureInfo culture, int arrayIndex);

    /// <summary>
    /// Transforms the text into it's new case.
    /// </summary>
    /// <param name="text">The text to transform.</param>
    /// <param name="culture">The culture to use for the transformation.</param>
    /// <returns>The transformed text.</returns>
    string TransformText(ReadOnlySpan<char> text, CultureInfo culture);
}
