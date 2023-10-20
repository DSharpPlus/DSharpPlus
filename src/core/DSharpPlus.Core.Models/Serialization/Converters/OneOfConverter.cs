// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA1810

using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using OneOf;

namespace DSharpPlus.Core.Models.Serialization.Converters;

/// <summary>
/// Provides a mechanicsm for serializing and deserializing <see cref="OneOf"/> objects.
/// </summary>
public sealed class OneOfConverter<TUnion> : JsonConverter<TUnion>
    where TUnion : IOneOf
{    
    // using type handles turns out to be marginally faster than Type, but it doesn't fundamentally matter
    // and this might well change in future .NET versions, in which case this code should use Type for ease
    // of reading
    private static readonly FrozenDictionary<long, Func<object, IOneOf>> constructionMethods;

    // we cache ordered types by the jsontokentype
    private static readonly FrozenDictionary<JsonTokenType, FrozenSet<Type>> orderedUnion;

    // this cctor is *extremely* expensive and creates quite a lot of cached data. we might want to break this
    // up a bit more, or use specialized converters for smaller OneOfs that don't need all this elaborate
    // ordering ceremony because they only have a select few valid orders anyways.
    static OneOfConverter()
    {
        Type[] unionTypes = typeof(TUnion).GetGenericArguments();

        // order of type priority:
        // 1. snowflake
        // 2. integer primitives (is assignable to INumber but not to IFloatingPoint, is struct)
        // 3. float primitives (assignable to INumber and IFloatingPoint, is struct)
        // 4. other types that aren't models
        // 5. models
        FrozenSet<Type> baselineOrderedUnionTypes = unionTypes
            .OrderByDescending(t => t == typeof(Snowflake) || t == typeof(Snowflake?))
            .ThenByDescending(t => t.IsAssignableTo(typeof(INumber<>).MakeGenericType(t)) && t.IsValueType)
            .ThenBy(t => t.IsAssignableTo(typeof(IFloatingPoint<>).MakeGenericType(t)))
            .ThenBy(t => t.FullName!.StartsWith("DSharpPlus", StringComparison.InvariantCulture))
            .ToFrozenSet();

        // construction methods
        Dictionary<long, Func<object, IOneOf>> methods = [];

        for (int i = 0; i < unionTypes.Length; i++)
        {
            MethodInfo? method = typeof(TUnion).GetMethod($"FromT{i}")!;

            Func<object, IOneOf> @delegate = Unsafe.As<Func<object, IOneOf>>(method.CreateDelegate(unionTypes[i]));
            methods.Add(unionTypes[i].TypeHandle.Value, @delegate);
        }

        constructionMethods = methods.ToFrozenDictionary();

        // priority
        Dictionary<JsonTokenType, FrozenSet<Type>> priorities = new()
        {
            // our baseline is already optimized for numbers
            [JsonTokenType.Number] = baselineOrderedUnionTypes
        };

        // nullability
        priorities.Add
        (
            JsonTokenType.Null,
            baselineOrderedUnionTypes.OrderByDescending
            (
                type => type.GetGenericTypeDefinition() == typeof(Nullable<>) || !type.IsValueType
            )
            .ToFrozenSet()
        );

        // booleans
        FrozenSet<Type> booleanPriority = baselineOrderedUnionTypes.OrderByDescending
        (
            type => type == typeof(bool) || type == typeof(bool?)
        )
        .ToFrozenSet();

        priorities.Add(JsonTokenType.True, booleanPriority);
        priorities.Add(JsonTokenType.False, booleanPriority);

        // string
        priorities.Add
        (
            JsonTokenType.String,
            baselineOrderedUnionTypes.OrderByDescending
            (
                type => type == typeof(string)
            )
            .ToFrozenSet()
        );

        // collections
        priorities.Add
        (
            JsonTokenType.StartArray,
            // we can make our life easier here by seeing whether we're assignable to non-generic IEnumerable
            baselineOrderedUnionTypes.OrderByDescending(type => type.IsAssignableTo(typeof(IEnumerable)))
                .ToFrozenSet()
        );

        // start object
        priorities.Add
        (
            JsonTokenType.StartObject,
            baselineOrderedUnionTypes.OrderByDescending(type => !type.IsPrimitive)
                .ThenByDescending(type => type != typeof(Snowflake) && type != typeof(Snowflake?))
                .ThenByDescending(type => type.GetGenericTypeDefinition() != typeof(Nullable<>))
                .ToFrozenSet()
        );

        orderedUnion = priorities.ToFrozenDictionary();
    }

    public override TUnion? Read
    (
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options
    )
    {
        foreach (Type type in orderedUnion[reader.TokenType])
        {
            object value;

            try
            {
                value = JsonSerializer.Deserialize(ref reader, type, options)!;
            }
            // it's tragic we have to eat an exception here, but, try again
            catch (JsonException)
            {
                continue;
            }

            return (TUnion)constructionMethods[type.TypeHandle.Value](value);
        }

        throw new JsonException("The value could not be parsed into the given union.");
    }

    public override void Write(Utf8JsonWriter writer, TUnion value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.Value, typeof(TUnion).GetGenericArguments()[value.Index], options);
}
