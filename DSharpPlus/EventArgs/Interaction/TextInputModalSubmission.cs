using System;
using System.Globalization;
using DSharpPlus.Entities;

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
    /// Parse <see cref="Value"/> as supplied data type.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="Value"/> to.</typeparam>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public T GetValueAs<T>() where T : IParsable<T>
    {
        return TryGetValueAs<T>(out T? result)
            ? result
            : throw new FormatException(
            $"Value \"{this.Value}\" cannot be parsed as {typeof(T).Name}.");
    }

    /// <summary>
    /// Try Parse <see cref="Value"/> as supplied data type.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to try convert the <see cref="Value"/> to.</typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValueAs<T>(out T value) where T : IParsable<T> => T.TryParse(this.Value, CultureInfo.InvariantCulture, out value);

    /// <summary>
    /// Parse <see cref="Value"/> as supplied data type, else <![CDATA[default(T)]]>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetValueAsOrDefault<T>() where T : IParsable<T>
    {
        return TryGetValueAs<T>(out T? result)
            ? result
            : default!; // default(T) -> null/0/false etc.
    }

    /// <summary>
    /// Parse <see cref="Value"/> as supplied data type, else return your supplied <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public T GetValueAsOrDefault<T>(T defaultValue) where T : IParsable<T>
    {
        return TryGetValueAs<T>(out T? result)
            ? result
            : defaultValue;
    }
}
