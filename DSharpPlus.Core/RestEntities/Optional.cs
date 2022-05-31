using System;
using System.Collections.Generic;
using DSharpPlus.Core.JsonConverters;
using DSharpPlus.Core.JsonConverters.Attributes;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// Often used in Discord API responses and requests, this class is used to represent an optional field.
    /// </summary>
    /// <remarks>
    /// While a Json field can be null, Discord enforces that a field can be missing entirely. This means that a field can be missing and/or null at any time. This struct is used to represent that.
    /// </remarks>
    [JsonConverter(typeof(OptionalConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(OptionalJsonConverterFactory))]
    [TypeJsonIgnore(System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public struct Optional<T> : IEquatable<Optional<T>>
    {
        /// <summary>
        /// An <see cref="Optional{T}"/> without a value.
        /// </summary>
        public static Optional<T> Empty { get; }

        /// <summary>
        /// If the <see cref="Optional{T}"/> has a value. The value may be null.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// The value to be returned if the <see cref="Optional{T}"/> has a value.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this <see cref="Optional{T}"/> has no value.</exception>
        public T Value => HasValue ? _value! : throw new InvalidOperationException("Optional<T> has no value.");

        /// <summary>
        /// The internal value. If no value is provided, this will be initialized to the default value of <typeparamref name="T"/>.
        /// </summary>
        private readonly T? _value = default;

        /// <summary>
        /// Creates an empty instance of <see cref="Optional{T}"/>.
        /// </summary>
        public Optional() => HasValue = false;

        /// <summary>
        /// Creates an instance of <see cref="Optional{T}"/> with the specified value.
        /// </summary>
        public Optional(T value)
        {
            _value = value;
            HasValue = true;
        }

        /// <summary>
        /// Checks if the property has a value that isn't null.
        /// </summary>
        /// <returns>If the property has a value that isn't null.</returns>
        public bool IsDefined() => HasValue && _value != null;

        /// <summary>
        /// If a value is present, applies the provided function to it, and returns the result.
        /// </summary>
        /// <param name="ifPresentFunction">The lambda function to evaluate if there's a value present.</param>
        /// <typeparam name="TOutput">The type that the lambda function is expected to return.</typeparam>
        /// <returns>An <see cref="Optional{TOutput}"/>. Optional.HasValue will be false if the field wasn't present.</returns>
        /// <remarks>This checks if the <see cref="Optional{T}"/> has a value, not if the value is null.</remarks>
        public Optional<TOutput> IfPresent<TOutput>(Func<T, TOutput> ifPresentFunction) => HasValue ? new Optional<TOutput>(ifPresentFunction(_value!)) : Optional<TOutput>.Empty;

        /// <summary>
        /// Gets the hash code for this <see cref="Optional{T}"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="Optional{T}"/>.</returns>
        public override int GetHashCode() => HasValue ? (_value?.GetHashCode() ?? 0) : 0;

        /// <summary>
        /// Checks whether this <see cref="Optional{T}"/> is equal to another <see cref="Optional{T}"/>.
        /// </summary>
        /// <param name="other"><see cref="Optional{T}"/> to compare to.</param>
        /// <returns>Whether the <see cref="Optional{T}"/> is equal to this <see cref="Optional{T}"/>.</returns>
        public bool Equals(Optional<T> other) => HasValue
            ? (other.HasValue && EqualityComparer<T>.Default.Equals(_value, other._value))
            : !other.HasValue;

        /// <summary>
        /// Checks whether the value of this <see cref="Optional{T}"/> is equal to specified object.
        /// </summary>
        /// <param name="other">Object to compare to.</param>
        /// <returns>Whether the object is equal to the value of this <see cref="Optional{T}"/>.</returns>
        public bool Equals(T other) => HasValue && EqualityComparer<T>.Default.Equals(_value, other);

        /// <summary>
        /// Checks whether the value of this <see cref="Optional{T}"/> is equal to specified object.
        /// </summary>
        /// <param name="other">Object to compare to.</param>
        /// <returns>Whether the object is equal to the value of this <see cref="Optional{T}"/>.</returns>
        public override bool Equals(object? other) => other switch
        {
            T otherType => Equals(otherType),
            Optional<T> otherObject => Equals(otherObject),
            _ => false
        };

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString() => HasValue ? (_value?.ToString() ?? string.Empty) : "<Empty>";

        public static implicit operator Optional<T>(T value) => new(value);
        public static explicit operator T(Optional<T> optional) => optional.Value;
        public static bool operator ==(Optional<T> opt, T t) => opt.Equals(t);
        public static bool operator !=(Optional<T> opt, T t) => !opt.Equals(t);
    }
}
