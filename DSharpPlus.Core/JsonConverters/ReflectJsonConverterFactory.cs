// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
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
            && !typeToConvert.Namespace!.StartsWith("System")
            && !typeof(IEnumerable).IsAssignableFrom(typeToConvert);

        /// <inheritdoc/>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter)typeof(ReflectJsonConverter<>).MakeGenericType(typeToConvert).GetConstructor(Type.EmptyTypes)!.Invoke(null)!;
    }
}
