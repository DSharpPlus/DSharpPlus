using System;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Extensions;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides information about a string select menu submitted through a modal.
/// </summary>
public sealed class SelectMenuModalSubmission : IModalSubmission
{
    /// <inheritdoc/>
    public DiscordComponentType ComponentType => DiscordComponentType.StringSelect;

    /// <inheritdoc/>
    public string CustomId { get; internal set; }

    /// <summary>
    /// The values selected from the menu.
    /// </summary>
    public IReadOnlyList<string> Values { get; internal set; }

    internal SelectMenuModalSubmission(string customId, IReadOnlyList<string> values)
    {
        this.CustomId = customId;
        this.Values = values;
    }

    /// <summary>
    /// Parse all <see cref="SelectMenuModalSubmission.Values"/> as <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="SelectMenuModalSubmission.Values"/> to.</typeparam>
    /// <returns>All parsable <see cref="SelectMenuModalSubmission.Values"/></returns>
    public IEnumerable<T> GetValuesAs<T>()
        where T : IParsable<T>
    {
        List<FormatException> errors = [];

        foreach (string i in this.Values)
        {
            if (i.TryGetValueAs<T>(out T? value))
            {
                yield return value;
            }
            else
            {
                errors.Add(new FormatException($"Value \"{i}\" cannot be parsed as {typeof(T).Name}."));
            }                
        }

        if (errors.Count > 0)
        {
            throw new AggregateException($"One or more values cannot be parsed as {typeof(T).Name}.", errors);
        }
    }

    /// <summary>
    /// Parse all <see cref="SelectMenuModalSubmission.Values"/> as <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="SelectMenuModalSubmission.Values"/> to.</typeparam>
    /// <returns>All parsable <see cref="SelectMenuModalSubmission.Values"/>, where each are <![CDATA[default(T)]]> if not parsable.</returns>
    public IEnumerable<T> GetValuesAsOrDefault<T>()
        where T : IParsable<T>
    {
        foreach (string i in this.Values)
        {
            yield return i.TryGetValueAs<T>(out T? value) ? value : default!;
        }
    }

    /// <summary>
    /// Parse all <see cref="SelectMenuModalSubmission.Values"/> as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="SelectMenuModalSubmission.Values"/> to.</typeparam>
    /// <param name="defaultValue">Will return this if cannot parse <see cref="SelectMenuModalSubmission.Values"/> to <typeparamref name="T"/></param>
    /// <returns><see cref="SelectMenuModalSubmission.Values"/> as <typeparamref name="T"/>, else return your supplied <paramref name="defaultValue"/></returns>
    public IEnumerable<T> GetValueAsOrDefault<T>
    (
        T defaultValue
    )
        where T : IParsable<T>
    {
        foreach (string i in this.Values)
        {
            yield return i.TryGetValueAs<T>(out T? value) ? value : defaultValue;
        }
    }
}
