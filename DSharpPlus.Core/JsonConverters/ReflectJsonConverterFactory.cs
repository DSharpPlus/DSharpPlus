using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.JsonConverters
{
    /// <summary>
    /// A <see cref="JsonConverterFactory"/> for instances of <see cref="ReflectJsonConverter{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This converter should only be applied on the <see cref="JsonSerializerOptions"/> level.
    /// To apply the converter on type-level, use <see cref="ReflectJsonConverter{T}"/> directly.
    /// </para>
    /// <para>
    /// This type will automatically cover any type that meets the following criteria if added to <see cref="JsonSerializerOptions"/>:
    /// </para>
    /// <list type="bullet">
    /// <item>Is a class (not a struct or interface)</item>
    /// <item>Is not a type in the <c>System</c> namespace</item>
    /// <item>Has no custom type-level converter</item>
    /// <item>Does not implement <see cref="IEnumerable"/></item>
    /// </list>
    /// </remarks>
    public sealed class ReflectJsonConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
            => !typeToConvert.IsInterface
            && !typeToConvert.IsValueType
            && !typeToConvert.IsDefined(typeof(JsonConverterAttribute))
            && !typeToConvert.Namespace!.StartsWith("System", false, CultureInfo.InvariantCulture)
            && !typeof(IEnumerable).IsAssignableFrom(typeToConvert);

        /// <inheritdoc/>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter)typeof(ReflectJsonConverter<>).MakeGenericType(typeToConvert).GetConstructor(Type.EmptyTypes)!.Invoke(null)!;
    }
}
