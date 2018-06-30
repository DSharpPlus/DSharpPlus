using System;
using DSharpPlus.Net.Serialization;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a value which may or may not have a value.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public struct Optional<T> : IEquatable<Optional<T>>, IEquatable<T>
    {
        /// <summary>
        /// Gets whether this <see cref="Optional{T}"/> has a value.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// Gets the value of this <see cref="Optional{T}"/>.
        /// </summary>
        public T Value
        {
            get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException("Value is not set.");
                }

                return _val;
            }
        }
        private T _val;

        /// <summary>
        /// Creates a new <see cref="Optional{T}"/> with specified value.
        /// </summary>
        /// <param name="value">Value of this option.</param>
        public Optional(T value)
        {
            _val = value;
            HasValue = true;
        }
        
        /// <summary>
        /// Returns a string representation of this optional value.
        /// </summary>
        /// <returns>String representation of this optional value.</returns>
        public override string ToString()
        {
            return $"Optional<{typeof(T)}> ({(HasValue ? Value.ToString() : "<no value>")})";
        }

        /// <summary>
        /// Checks whether this <see cref="Optional{T}"/> (or its value) are equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="Optional{T}"/> or its value.</returns>
        public override bool Equals(object obj)
        {
            if (obj is T t)
            {
                return Equals(t);
            }

            if (obj is Optional<T> opt)
            {
                return Equals(opt);
            }

            return false;
        }

        /// <summary>
        /// Checks whether this <see cref="Optional{T}"/> is equal to another <see cref="Optional{T}"/>.
        /// </summary>
        /// <param name="e"><see cref="Optional{T}"/> to compare to.</param>
        /// <returns>Whether the <see cref="Optional{T}"/> is equal to this <see cref="Optional{T}"/>.</returns>
        public bool Equals(Optional<T> e)
        {
            if (!HasValue && !e.HasValue)
            {
                return true;
            }

            if (HasValue != e.HasValue)
            {
                return false;
            }

            return Value.Equals(e.Value);
        }

        /// <summary>
        /// Checks whether the value of this <see cref="Optional{T}"/> is equal to specified object.
        /// </summary>
        /// <param name="e">Object to compare to.</param>
        /// <returns>Whether the object is equal to the value of this <see cref="Optional{T}"/>.</returns>
        public bool Equals(T e)
        {
            if (!HasValue)
            {
                return false;
            }

            return ReferenceEquals(Value, e);
        }

        /// <summary>
        /// Gets the hash code for this <see cref="Optional{T}"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="Optional{T}"/>.</returns>
        public override int GetHashCode()
        {
            return HasValue ? Value.GetHashCode() : 0;
        }

        /// <summary>
        /// Creates a new <see cref="Optional{T}"/> with specified value and valid state.
        /// </summary>
        /// <param name="value">Value to populate the optional with.</param>
        /// <returns>Created optional.</returns>
        public static Optional<T> FromValue(T value)
            => new Optional<T>(value);

        /// <summary>
        /// Creates a new empty <see cref="Optional{T}"/> with no value and invalid state.
        /// </summary>
        /// <returns>Created optional.</returns>
        public static Optional<T> FromNoValue()
            => default;

        public static implicit operator Optional<T>(T val) 
            => new Optional<T>(val);

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

        public Optional<TTarget> IfPresent<TTarget>(Func<T, TTarget> mapper)
        {
            return HasValue ? new Optional<TTarget>(mapper(Value)) : default;
        }
    }
}
