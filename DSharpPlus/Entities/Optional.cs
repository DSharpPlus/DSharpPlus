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
using System.Linq;
using System.Reflection;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Helper methods for instantiating an <see cref="Optional{T}"/>.
    /// </summary>
    /// <remarks>
    /// This class only serves to allow type parameter inference on calls to <see cref="FromValue{T}"/> or
    /// <see cref="FromNoValue{T}"/>.
    /// </remarks>
    public static class Optional
    {
        /// <summary>
        /// Creates a new <see cref="Optional{T}"/> with specified value and valid state.
        /// </summary>
        /// <param name="value">Value to populate the optional with.</param>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <returns>Created optional.</returns>
        public static Optional<T> FromValue<T>(T value)
            => new(value);

        /// <summary>
        /// Creates a new empty <see cref="Optional{T}"/> with no value and invalid state.
        /// </summary>
        /// <typeparam name="T">The type that the created instance is wrapping around.</typeparam>
        /// <returns>Created optional.</returns>
        public static Optional<T> FromNoValue<T>()
            => default;
    }

    // used internally to make serialization more convenient, do NOT change this, do NOT implement this yourself
    internal interface IOptional
    {
        bool HasValue { get; }
        object RawValue { get; } // must NOT throw InvalidOperationException
    }

    /// <summary>
    /// Represents a wrapper which may or may not have a value.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    [JsonConverter(typeof(OptionalJsonConverter))]
    public readonly struct Optional<T> : IEquatable<Optional<T>>, IEquatable<T>, IOptional
    {
        /// <summary>
        /// Gets whether this <see cref="Optional{T}"/> has a value.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// Gets the value of this <see cref="Optional{T}"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this <see cref="Optional{T}"/> has no value.</exception>
        public T Value => this.HasValue ? this._val : throw new InvalidOperationException("Value is not set.");
        object IOptional.RawValue => this._val;

        private readonly T _val;

        /// <summary>
        /// Creates a new <see cref="Optional{T}"/> with specified value.
        /// </summary>
        /// <param name="value">Value of this option.</param>
        public Optional(T value)
        {
            this._val = value;
            this.HasValue = true;
        }

        /// <summary>
        /// Returns a string representation of this optional value.
        /// </summary>
        /// <returns>String representation of this optional value.</returns>
        public override string ToString() => $"Optional<{typeof(T)}> ({(this.HasValue ? this.Value.ToString() : "<no value>")})";

        /// <summary>
        /// Checks whether this <see cref="Optional{T}"/> (or its value) are equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="Optional{T}"/> or its value.</returns>
        public override bool Equals(object obj)
        {
            return obj switch
            {
                T t => this.Equals(t),
                Optional<T> opt => this.Equals(opt),
                _ => false,
            };
        }

        /// <summary>
        /// Checks whether this <see cref="Optional{T}"/> is equal to another <see cref="Optional{T}"/>.
        /// </summary>
        /// <param name="e"><see cref="Optional{T}"/> to compare to.</param>
        /// <returns>Whether the <see cref="Optional{T}"/> is equal to this <see cref="Optional{T}"/>.</returns>
        public bool Equals(Optional<T> e)
            => (!this.HasValue && !e.HasValue) || (this.HasValue == e.HasValue && this.Value.Equals(e.Value));

        /// <summary>
        /// Checks whether the value of this <see cref="Optional{T}"/> is equal to specified object.
        /// </summary>
        /// <param name="e">Object to compare to.</param>
        /// <returns>Whether the object is equal to the value of this <see cref="Optional{T}"/>.</returns>
        public bool Equals(T e)
            => this.HasValue && ReferenceEquals(this.Value, e);

        /// <summary>
        /// Gets the hash code for this <see cref="Optional{T}"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="Optional{T}"/>.</returns>
        public override int GetHashCode()
            => this.HasValue ? this.Value.GetHashCode() : 0;

        public static implicit operator Optional<T>(T val)
            => new(val);

        public static explicit operator T(Optional<T> opt)
            => opt.Value;

        public static bool operator ==(Optional<T> opt1, Optional<T> opt2)
            => opt1.Equals(opt2);

        public static bool operator !=(Optional<T> opt1, Optional<T> opt2)
            => !opt1.Equals(opt2);

        public static bool operator ==(Optional<T> opt, T t)
            => opt.Equals(t);

        public static bool operator !=(Optional<T> opt, T t)
            => !opt.Equals(t);

        /// <summary>
        /// Performs a mapping operation on the current <see cref="Optional{T}"/>, turning it into an Optional holding a
        /// <typeparamref name="TTarget"/> instance if the source optional contains a value; otherwise, returns an
        /// <see cref="Optional{T}"/> of that same type with no value.
        /// </summary>
        /// <param name="mapper">The mapping function to apply on the current value if it exists</param>
        /// <typeparam name="TTarget">The type of the target value returned by <paramref name="mapper"/></typeparam>
        /// <returns>
        /// An <see cref="Optional{T}"/> containing a value denoted by calling <paramref name="mapper"/> if the current
        /// <see cref="Optional{T}"/> contains a value; otherwise, an empty <see cref="Optional{T}"/> of the target
        /// type.
        /// </returns>
        public Optional<TTarget> IfPresent<TTarget>(Func<T, TTarget> mapper) => this.HasValue ? new Optional<TTarget>(mapper(this.Value)) : default;
    }

    /// <seealso cref="DiscordJson._serializer"/>
    internal sealed class OptionalJsonContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            var type = property.PropertyType;

            if (!type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IOptional)))
                return property;

            // we cache the PropertyInfo object here (it's captured in closure). we don't have direct 
            // access to the property value so we have to reflect into it from the parent instance
            // we use UnderlyingName instead of PropertyName in case the C# name is different from the Json name.
            var declaringMember = property.DeclaringType.GetTypeInfo().DeclaredMembers
                .FirstOrDefault(e => e.Name == property.UnderlyingName);

            switch (declaringMember)
            {
                case PropertyInfo declaringProp:
                    property.ShouldSerialize = instance => // instance here is the declaring (parent) type
                    {
                        var optionalValue = declaringProp.GetValue(instance);
                        return (optionalValue as IOptional).HasValue;
                    };
                    return property;
                case FieldInfo declaringField:
                    property.ShouldSerialize = instance => // instance here is the declaring (parent) type
                    {
                        var optionalValue = declaringField.GetValue(instance);
                        return (optionalValue as IOptional).HasValue;
                    };
                    return property;
                default:
                    throw new InvalidOperationException(
                        "Can only serialize Optional<T> members that are fields or properties");
            }
        }
    }

    internal sealed class OptionalJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // we don't check for HasValue here since it's checked in OptionalJsonContractResolver
            var val = (value as IOptional).RawValue;
            // JToken.FromObject will throw if `null` so we manually write a null value.
            if (val == null)
            {
                // you can read serializer.NullValueHandling here, but unfortunately you can **not** skip serialization
                // here, or else you will get a nasty JsonWriterException, so we just ignore its value and manually
                // write the null.
                writer.WriteToken(JsonToken.Null);
            }
            else
            {
                // convert the value to a JSON object and write it to the property value.
                JToken.FromObject(val).WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var genericType = objectType.GenericTypeArguments[0];

            var constructor = objectType.GetTypeInfo().DeclaredConstructors
                .FirstOrDefault(e => e.GetParameters()[0].ParameterType == genericType);

            return constructor.Invoke(new[] { serializer.Deserialize(reader, genericType) });
        }

        public override bool CanConvert(Type objectType) => objectType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IOptional));
    }
}
