using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DSharpPlus.Extensions;

internal static class StringExtentions
{
    /// <summary>
    /// Method for trying to convert a string to a caller provided data type that implements <see cref="IParsable{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T">The destination data type</typeparam>
    /// <param name="value"></param>
    /// <param name="parsedValue"></param>
    /// <returns></returns>
    internal static bool TryGetValueAs<T>
    (
        this string value,

        [NotNullWhen(true)]
        out T? parsedValue
    )
        where T : IParsable<T> 
        => T.TryParse(value, CultureInfo.InvariantCulture, out parsedValue);
}
