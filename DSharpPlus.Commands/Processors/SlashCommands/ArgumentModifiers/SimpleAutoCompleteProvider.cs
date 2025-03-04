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
    ///   If <c>false</c>, when multiple choices have the same
    ///   <see cref="DiscordAutoCompleteChoice.Value">Value</see>, only
    ///   the first such choice is presented to the user. Otherwise (if
    ///   <c>true</c>), has no effect.
    /// </summary>
    protected virtual bool AllowDuplicateValues { get; } = true;

    /// <summary>
    ///   If <c>true</c>, user input can be found anywhere in a matched
    ///   choice's <see cref="DiscordAutoCompleteChoice.Name">Name</see>.
    ///   Otherwise (if <c>false</c>), the Name must start with the user
    ///   input.
    /// </summary>
    protected virtual bool AllowInternalMatches { get; } = true;

    /// <inheritdoc/>
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        IEnumerable<DiscordAutoCompleteChoice> results;

        if (this.AllowInternalMatches)
        {
            results = this.Choices
                .Select(c => (Choice: c, Index: c.Name.IndexOf(context.UserInput ?? "", this.Comparison)))
                .Where(ci => ci.Index != -1)
                .OrderBy(ci => ci.Index)
                .Select(c => c.Choice);
        }
        else
        {
            results = this.Choices
                .Where(c => c.Name.StartsWith(context.UserInput ?? "", this.Comparison));
        }

        if (!this.AllowDuplicateValues)
        {
            results = results.DistinctBy(c => c.Value);
        }

        return ValueTask.FromResult(results.Take(25));
    }

    /// <summary>
    ///   Converts a sequence of strings into autocomplete choices.
    /// </summary>
    /// <param name="options">The sequence of strings.</param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert(IEnumerable<string> options)
      => options.Select(s => new DiscordAutoCompleteChoice(s, s));

    /// <summary>
    ///   Converts a sequence of floats into autocomplete choices.
    /// </summary>
    /// <param name="options">The sequence of floats.</param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert(IEnumerable<float> options)
      => options.Select(f => new DiscordAutoCompleteChoice(f.ToString(), f));

    /// <summary>
    ///   Converts a sequence of doubles into autocomplete choices.
    /// </summary>
    /// <param name="options">The sequence of doubles.</param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert(IEnumerable<double> options)
      => options.Select(d => new DiscordAutoCompleteChoice(d.ToString(), d));

    /// <summary>
    ///   Converts a sequence of ints into autocomplete choices.
    /// </summary>
    /// <param name="options">The sequence of ints.</param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert(IEnumerable<int> options)
      => options.Select(i => new DiscordAutoCompleteChoice(i.ToString(), i));

    /// <summary>
    ///   Converts a sequence of longs into autocomplete choices.
    /// </summary>
    /// <param name="options">The sequence of longs.</param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert(IEnumerable<long> options)
      => options.Select(l => new DiscordAutoCompleteChoice(l.ToString(), l));

    /// <summary>
    ///   Converts a sequence of string-string key-value pairs into
    ///   autocomplete choices.
    /// </summary>
    /// <param name="options">
    ///   The sequence of string-string key-value pairs.
    /// </param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert(IEnumerable<KeyValuePair<string, string>> options)
      => options.Select(kvp => new DiscordAutoCompleteChoice(kvp.Key, kvp.Value));

    /// <summary>
    ///   Converts a sequence of string-float key-value pairs into
    ///   autocomplete choices.
    /// </summary>
    /// <param name="options">
    ///   The sequence of string-float key-value pairs.
    /// </param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert(IEnumerable<KeyValuePair<string, float>> options)
      => options.Select(kvp => new DiscordAutoCompleteChoice(kvp.Key, kvp.Value));

    /// <summary>
    ///   Converts a sequence of string-double key-value pairs into
    ///   autocomplete choices.
    /// </summary>
    /// <param name="options">
    ///   The sequence of string-double key-value pairs.
    /// </param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert(IEnumerable<KeyValuePair<string, double>> options)
      => options.Select(kvp => new DiscordAutoCompleteChoice(kvp.Key, kvp.Value));

    /// <summary>
    ///   Converts a sequence of string-int key-value pairs into
    ///   autocomplete choices.
    /// </summary>
    /// <param name="options">
    ///   The sequence of string-int key-value pairs.
    /// </param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert(IEnumerable<KeyValuePair<string, int>> options)
      => options.Select(kvp => new DiscordAutoCompleteChoice(kvp.Key, kvp.Value));

    /// <summary>
    ///   Converts a sequence of string-long key-value pairs into
    ///   autocomplete choices.
    /// </summary>
    /// <param name="options">
    ///   The sequence of string-long key-value pairs.
    /// </param>
    /// <returns>The sequence of autocomplete choices.</returns>
    public static IEnumerable<DiscordAutoCompleteChoice> Convert(IEnumerable<KeyValuePair<string, long>> options)
      => options.Select(kvp => new DiscordAutoCompleteChoice(kvp.Key, kvp.Value));
}
