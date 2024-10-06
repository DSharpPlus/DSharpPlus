using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands.NamingPolicies;
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

    /// <inheritdoc/>
    public override bool NextParameter() => this.Interaction.Data.Options is not null && base.NextParameter();

    /// <inheritdoc/>
    public override bool NextArgument()
    {
        // Support for variadic-argument parameters
        if (this.VariadicArgumentAttribute is not null && !NextVariadicArgument())
        {
            return false;
        }

        // Convert the parameter into it's interaction-friendly name
        string parameterPolicyName = this.ParameterNamePolicy.GetParameterName(this.Parameter, SlashCommandProcessor.ResolveCulture(this.Interaction), this.VariadicArgumentParameterIndex);
        DiscordInteractionDataOption? argument = this.Options.SingleOrDefault(argument => argument.Name == parameterPolicyName);
        if (argument is null)
        {
            return false;
        }

        this.Argument = argument;
        base.Argument = argument.Value;
        return true;
    }
}
