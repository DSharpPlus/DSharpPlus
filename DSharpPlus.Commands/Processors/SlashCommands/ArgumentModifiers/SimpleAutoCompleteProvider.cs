using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

/// <summary>
///   An abstract class that may be derived to provide a simple
///   autocomplete implementation that filters from a list of options
///   based on user input.
/// </summary>
public class SimpleAutoCompleteProvider : IAutoCompleteProvider
{
    /// <summary>
    ///   The list of available choices for this autocomplete provider,
    ///   without any filtering applied.
    /// </summary>
    protected virtual IEnumerable<DiscordAutoCompleteChoice> Choices { get; } = [];

    /// <summary>
    ///   The string comparison used between user input and option names.
    /// </summary>
    protected virtual StringComparison Comparison { get; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    ///   If <see langword="false"/>, when multiple choices have the same
    ///   <see cref="DiscordAutoCompleteChoice.Value">Value</see>, only
    ///   the first such choice is presented to the user. Otherwise (if
    ///   <see langword="true"/>), has no effect.
    /// </summary>
    protected virtual bool AllowDuplicateValues { get; } = true;

    /// <summary>
    ///   The method by which choices are matched to the user input.
    /// </summary>
    protected virtual SimpleAutoCompleteStringMatchingMethod MatchingMethod { get; } = SimpleAutoCompleteStringMatchingMethod.Contains;

    /// <inheritdoc/>
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        IEnumerable<DiscordAutoCompleteChoice> results;

        switch (this.MatchingMethod)
        {
            case SimpleAutoCompleteStringMatchingMethod.Contains:
                results = this.Choices
                    .Select(c => (Choice: c, Index: c.Name.IndexOf(context.UserInput ?? "", this.Comparison)))
                    .Where(ci => ci.Index != -1)
                    .OrderBy(ci => ci.Index)
                    .Select(c => c.Choice);
                break;
            case SimpleAutoCompleteStringMatchingMethod.StartsWith:
                results = this.Choices
                    .Where(c => c.Name.StartsWith(context.UserInput ?? "", this.Comparison));
                break;
            // case MatchingMethod.Fuzzy:
            //     results = this.Choices
            //         .Select(c => Choice: c, Score: FuzzyMatch(c.Name, context.UserInput, this.Comparison))
            //         .Where(cs => cs.Score > 0)
            //         .OrderByDescending(cs => cs.Score)
            //         .Select(c => c.Choice);
            //     break;
            default:
                throw new InvalidOperationException($"The value {this.MatchingMethod} is not valid.");
        }

        if (!this.AllowDuplicateValues)
        {
            results = results.DistinctBy(c => c.Value);
        }

        return ValueTask.FromResult(results.Take(25));
    }

    /// <summary>
    ///   Converts a sequence into autocomplete choices.
    /// </summary>
    /// <param name="options">The input sequence.</param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert<T>(IEnumerable<T> options)
    {
        return options.Select(o => o switch
            {
                string s => new DiscordAutoCompleteChoice(s, s),
                float f => new DiscordAutoCompleteChoice(f.ToString(), f),
                double d => new DiscordAutoCompleteChoice(d.ToString(), d),
                int i => new DiscordAutoCompleteChoice(i.ToString(), i),
                long l => new DiscordAutoCompleteChoice(l.ToString(), l),
                KeyValuePair<string, string> kvp => new DiscordAutoCompleteChoice(kvp.Key, kvp.Value),
                KeyValuePair<string, float> kvp => new DiscordAutoCompleteChoice(kvp.Key, kvp.Value),
                KeyValuePair<string, double> kvp => new DiscordAutoCompleteChoice(kvp.Key, kvp.Value),
                KeyValuePair<string, int> kvp => new DiscordAutoCompleteChoice(kvp.Key, kvp.Value),
                KeyValuePair<string, long> kvp => new DiscordAutoCompleteChoice(kvp.Key, kvp.Value),
                _ => throw new InvalidCastException($"Cannot use {o?.GetType()?.Name ?? "null"} as the value of a DiscordAutoCompleteChoice.")
            });
    }
}
