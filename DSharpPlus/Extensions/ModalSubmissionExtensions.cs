using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Extensions;

/// <summary>
/// Provides convenience extensions for extracting information from modal submissions.
/// </summary>
public static class ModalSubmissionExtensions
{
    #region IModalSubmission Extensions

    /// <summary>
    /// Gets the component submission associated with a particular custom ID, returns null if either the ID didn't exist or pointed to a different type.
    /// </summary>
    /// <typeparam name="T">The type of the component submission to extract.</typeparam>
    /// <param name="submissions">The modal submission dictionary to extract from.</param>
    /// <param name="customId">The custom ID of the component.</param>
    public static T? GetComponentSubmission<T>(this IReadOnlyDictionary<string, IModalSubmission> submissions, string customId)
        where T : IModalSubmission
    {
        _ = TryGetComponentSubmission<T>(submissions, customId, out T? candidate);
        return candidate;
    }

    /// <summary>
    /// Gets the component submission associated with a particular custom ID, returns false if either the ID didn't exist or pointed to a different type.
    /// </summary>
    /// <typeparam name="T">The type of the component submission to extract.</typeparam>
    /// <param name="submissions">The modal submission dictionary to extract from.</param>
    /// <param name="customId">The custom ID of the component.</param>
    /// <param name="value">The value of the submission, or null if it couldn't be found</param>
    public static bool TryGetComponentSubmission<T>
    (
        this IReadOnlyDictionary<string, IModalSubmission> submissions,
        string customId,

        [NotNullWhen(true)]
        out T? value
    )
        where T : IModalSubmission
    {
        if (submissions.TryGetValue(customId, out IModalSubmission? candidate) && candidate is T submission)
        {
            value = submission;
            return true;
        }

        value = default;
        return false;
    }

    #endregion IModalSubmission Extensions

    #region TextInputModalSubmission Extensions

    /// <summary>
    /// Parse <see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="TextInputModalSubmission.Value"/> to.</typeparam>
    /// <param name="submission">The <see cref="TextInputModalSubmission"/> to extract value from.</param>
    /// <returns><see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/></returns>
    /// <exception cref="FormatException"></exception>
    public static T GetValueAs<T>
    (
        this TextInputModalSubmission submission
    ) 
        where T : IParsable<T>
    {
        return submission.TryGetValueAs<T>(out T? result)
            ? result
            : throw new FormatException(
            $"Value \"{submission.Value}\" cannot be parsed as {typeof(T).Name}.");
    }

    /// <summary>
    /// Try Parse <see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="TextInputModalSubmission.Value"/> to.</typeparam>
    /// <param name="submission">The <see cref="TextInputModalSubmission"/> to extract value from.</param>
    /// <param name="value">The converted value when able to parse, else <![CDATA[null]]>.</param>
    /// <returns><![CDATA[true]]> when able to parse.</returns>
    public static bool TryGetValueAs<T>
    (
        this TextInputModalSubmission submission, 
        
        [NotNullWhen(true)]
        out T? value
    ) 
        where T : IParsable<T> 
        => submission.Value.TryGetValueAs(out value);

    /// <summary>
    /// Parse <see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>, else <![CDATA[default(T)]]>.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="TextInputModalSubmission.Value"/> to.</typeparam>
    /// <param name="submission"></param>
    /// <returns><see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>, else <![CDATA[default(T)]]>.</returns>
    public static T GetValueAsOrDefault<T>
    (
        this TextInputModalSubmission submission
    ) 
        where T : IParsable<T>
    {
        return submission.TryGetValueAs<T>(out T? result)
            ? result
            : default!;
    }

    /// <summary>
    /// Parse <see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>, else return your supplied <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="TextInputModalSubmission.Value"/> to.</typeparam>
    /// <param name="submission">The <see cref="TextInputModalSubmission"/> to extract value from.</param>
    /// <param name="defaultValue">Will return this if cannot parse <see cref="TextInputModalSubmission.Value"/> to <typeparamref name="T"/></param>.
    /// <returns><see cref="TextInputModalSubmission.Value"/> as <typeparamref name="T"/>, else return your supplied <paramref name="defaultValue"/></returns>
    public static T GetValueAsOrDefault<T>
    (
        this TextInputModalSubmission submission, 
        T defaultValue
    )
        where T : IParsable<T>
    {
        return submission.TryGetValueAs<T>(out T? result)
            ? result
            : defaultValue;
    }

    #endregion TextInputModalSubmission Extensions

    #region SelectMenuModalSubmission Extensions

    /// <summary>
    /// Parse all <see cref="SelectMenuModalSubmission.Values"/> as <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="SelectMenuModalSubmission.Values"/> to.</typeparam>
    /// <param name="submission">The <see cref="SelectMenuModalSubmission"/> to extract the values from.</param>
    /// <returns>All parsable <see cref="SelectMenuModalSubmission.Values"/></returns>
    public static IEnumerable<T> GetValuesAs<T>
    (
        this SelectMenuModalSubmission submission
    ) 
        where T : IParsable<T>
    {
        foreach (string i in submission.Values)
        {
            if (i.TryGetValueAs<T>(out T? value))
            {
                yield return value; 
            }
        }
    }

    /// <summary>
    /// Parse all <see cref="SelectMenuModalSubmission.Values"/> as <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="SelectMenuModalSubmission.Values"/> to.</typeparam>
    /// <param name="submission">The <see cref="SelectMenuModalSubmission"/> to extract the values from.</param>
    /// <returns>All parsable <see cref="SelectMenuModalSubmission.Values"/>, where each are <![CDATA[default(T)]]> if not parsable.</returns>
    public static IEnumerable<T> GetValuesAsOrDefault<T>
    (
        this SelectMenuModalSubmission submission
    )
        where T : IParsable<T>
    {
        foreach (string i in submission.Values)
        {
            yield return i.TryGetValueAs<T>(out T? value) ? value : default!;
        }
    }

    /// <summary>
    /// Parse all <see cref="SelectMenuModalSubmission.Values"/> as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Supplied data type you wish to convert the <see cref="SelectMenuModalSubmission.Values"/> to.</typeparam>
    /// <param name="submission">The <see cref="SelectMenuModalSubmission"/> to extract the values from.</param>
    /// <param name="defaultValue">Will return this if cannot parse <see cref="SelectMenuModalSubmission.Values"/> to <typeparamref name="T"/></param>
    /// <returns><see cref="SelectMenuModalSubmission.Values"/> as <typeparamref name="T"/>, else return your supplied <paramref name="defaultValue"/></returns>
    public static IEnumerable<T> GetValueAsOrDefault<T>
    (
        this SelectMenuModalSubmission submission, 
        T defaultValue
    ) 
        where T : IParsable<T>
    {
        foreach (string i in submission.Values)
        {
            yield return i.TryGetValueAs<T>(out T? value) ? value : defaultValue;
        }
    }

    #endregion SelectMenuModalSubmission Extensions


    #region StringConversionBS

    /// <summary>
    /// Private extension method for trying to convert a string to a caller provided data type that implements <see cref="IParsable{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T">The destination data type</typeparam>
    /// <param name="value"></param>
    /// <param name="parsedValue"></param>
    /// <returns></returns>
    private static bool TryGetValueAs<T>
    (
        this string value,

        [NotNullWhen(true)] 
        out T? parsedValue
    ) 
        where T : IParsable<T> => 
        T.TryParse(value, CultureInfo.InvariantCulture, out parsedValue);

    #endregion StringConversionBS
}
