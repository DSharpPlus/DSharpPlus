using System;
using System.Diagnostics.CodeAnalysis;

using DSharpPlus.Entities;
using DSharpPlus.Extensions;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides information about the data submitted through a text input component in a modal.
/// </summary>
public sealed class TextInputModalSubmission : IModalSubmission
{
    /// <inheritdoc/>
    public DiscordComponentType ComponentType => DiscordComponentType.TextInput;

    /// <inheritdoc/>
    public string CustomId { get; internal set; }

    /// <inheritdoc/>
    public string Value { get; internal set; }

    internal TextInputModalSubmission(string customId, string value)
    {
        this.CustomId = customId;
        this.Value = value;
    }

    /// <summary>
    /// Parse <see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="TextInputModalSubmission.Value"/> to.</typeparam>
    /// <returns><see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/></returns>
    /// <exception cref="FormatException"></exception>
    public T GetValueAs<T>()
        where T : IParsable<T>
    {
        return TryGetValueAs<T>(out T? result)
            ? result
            : throw new FormatException(
            $"Value \"{this.Value}\" cannot be parsed as {typeof(T).Name}.");
    }

    /// <summary>
    /// Try Parse <see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="TextInputModalSubmission.Value"/> to.</typeparam>
    /// <param name="value">The converted value when able to parse, else <![CDATA[null]]>.</param>
    /// <returns><![CDATA[true]]> when able to parse.</returns>
    public bool TryGetValueAs<T>
    (
        [NotNullWhen(true)]
        out T? value
    )
        where T : IParsable<T>
        => this.Value.TryGetValueAs(out value);

    /// <summary>
    /// Parse <see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>, else <![CDATA[default(T)]]>.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="TextInputModalSubmission.Value"/> to.</typeparam>
    /// <returns><see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>, else <![CDATA[default(T)]]>.</returns>
    public T GetValueAsOrDefault<T>()
        where T : IParsable<T>
    {
        return TryGetValueAs<T>(out T? result)
            ? result
            : default!;
    }

    /// <summary>
    /// Parse <see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>, else return your supplied <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="TextInputModalSubmission.Value"/> to.</typeparam>
    /// <param name="defaultValue">Will return this if cannot parse <see cref="TextInputModalSubmission.Value"/> to <typeparamref name="T"/></param>.
    /// <returns><see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>, else return your supplied <paramref name="defaultValue"/></returns>
    public T GetValueAsOrDefault<T>
    (
        T defaultValue
    )
        where T : IParsable<T>
    {
        return TryGetValueAs<T>(out T? result)
            ? result
            : defaultValue;
    }
}
