using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using DSharpPlus.EventArgs;

namespace DSharpPlus.Extensions;

/// <summary>
/// Provides convenience extensions for extracting information from modal submissions.
/// </summary>
public static class ModalSubmissionExtensions
{
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
}
