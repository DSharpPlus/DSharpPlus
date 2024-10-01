using System;
using System.Collections;
using System.Text;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Processors.SlashCommands.NamingPolicies;

/// <summary>
/// Represents a policy for naming parameters. This is used to determine the
/// name of the parameter when registering or receiving interaction data.
/// </summary>
public interface IInteractionNamingPolicy
{
    /// <summary>
    /// Transforms the command name into the name that should be used for the interaction data.
    /// </summary>
    /// <param name="command">The command being transformed.</param>
    /// <returns>The name that should be used for the interaction data.</returns>
    string GetCommandName(Command command);

    /// <summary>
    /// Transforms the parameter name into the name that should be used for the interaction data.
    /// </summary>
    /// <param name="parameter">The parameter being transformed.</param>
    /// <param name="arrayIndex">
    /// If this parameter is part of an <see cref="IEnumerable" />, the index of the parameter.
    /// The value will be -1 if this parameter is not part of an <see cref="IEnumerable" />.
    /// </param>
    /// <returns>The name that should be used for the interaction data.</returns>
    string GetParameterName(CommandParameter parameter, int arrayIndex);

    /// <summary>
    /// Transforms the text into it's new case.
    /// </summary>
    /// <param name="text">The text to transform.</param>
    /// <returns>The transformed text.</returns>
    StringBuilder TransformText(ReadOnlySpan<char> text);
}
