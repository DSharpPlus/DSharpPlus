using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands.ParameterNamingPolicies;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands;

/// <summary>
/// Represents a context for interaction-based argument converters.
/// </summary>
public record InteractionConverterContext : ConverterContext
{
    /// <summary>
    /// The parameter naming policy to use when mapping parameters to interaction data.
    /// </summary>
    public required IInteractionNamingPolicy ParameterNamePolicy { get; init; }

    /// <summary>
    /// The underlying interaction.
    /// </summary>
    public required DiscordInteraction Interaction { get; init; }

    /// <summary>
    /// The options passed to this command.
    /// </summary>
    public required IReadOnlyList<DiscordInteractionDataOption> Options { get; init; }

    /// <summary>
    /// The current argument to convert.
    /// </summary>
    public new DiscordInteractionDataOption? Argument { get; protected set; }

    /// <summary>
    /// The current index of the argument for enumerable parameters.
    /// </summary>
    public int ArgumentEnumerableIndex { get; private set; } = -1;

    /// <inheritdoc/>
    public override bool NextParameter() =>
        this.Interaction.Data.Options is not null && base.NextParameter();

    /// <inheritdoc/>
    public override bool NextArgument()
    {
        // Support for multi-argument parameters
        if (
            this.Parameter.Attributes.SingleOrDefault(attribute =>
                attribute is MultiArgumentAttribute
            )
            is MultiArgumentAttribute multiArgumentAttribute
        )
        {
            // If we've reached the maximum argument count, return false
            // This means we'll move on to the next parameter without consuming the next argument
            if (this.ArgumentEnumerableIndex >= multiArgumentAttribute.MaximumArgumentCount)
            {
                // TODO: Find a way to allow the user to test
                //       if there were too many arguments passed
                //       and return an error message
                return false;
            }

            this.ArgumentEnumerableIndex++;
        }
        else
        {
            // If we're not a multi-argument parameter, reset the index
            this.ArgumentEnumerableIndex = -1;
        }

        // Convert the parameter into it's interaction-friendly name
        string parameterPolicyName = this.ParameterNamePolicy.GetParameterName(
            this.Parameter,
            this.ArgumentEnumerableIndex
        );
        DiscordInteractionDataOption? argument = this.Options.SingleOrDefault(argument =>
            argument.Name == parameterPolicyName
        );
        if (argument is null)
        {
            return false;
        }

        this.Argument = argument;
        base.Argument = argument.Value;
        return true;
    }
}
