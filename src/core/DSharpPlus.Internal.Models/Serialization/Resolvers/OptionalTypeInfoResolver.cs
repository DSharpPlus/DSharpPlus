// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace DSharpPlus.Internal.Models.Serialization.Resolvers;

/// <summary>
/// Provides a mechanism for resolving serialization of <see cref="Optional{T}"/>.
/// </summary>
public static class OptionalTypeInfoResolver
{
    public static IJsonTypeInfoResolver Default { get; } = new DefaultJsonTypeInfoResolver
    {
        Modifiers =
        {
            (type) =>
            {
                foreach (JsonPropertyInfo property in type.Properties)
                {
                    if (property.PropertyType.IsConstructedGenericType &&
                        property.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>))
                    {
                        property.ShouldSerialize = Unsafe.As<Func<object, object?, bool>>
                        (
                            typeof(OptionalTypeInfoResolver)
                            .GetMethod
                            (
                                nameof(ShouldIgnoreOptional),
                                BindingFlags.NonPublic | BindingFlags.Static
                            )!
                            .MakeGenericMethod
                            (
                                property.PropertyType.GetGenericArguments()[0]!
                            )
                            .CreateDelegate<Func<object, object?, bool>>()
                        );
                    }
                }
            }
        }
    };

    private static bool ShouldIgnoreOptional<T>(object _, object? value)
        => Unsafe.Unbox<Optional<T>>(value!).HasValue;
}
