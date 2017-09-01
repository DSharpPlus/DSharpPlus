using System;

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
                if (!this.HasValue)
                    throw new InvalidOperationException("Value is not set.");
                return this._val;
            }
        }
        private T _val;

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
        public override string ToString()
        {
            return string.Concat("Optional<", typeof(T), "> (", this.HasValue ? this.Value.ToString() : "<no value>", ")");
        }

        /// <summary>
        /// Checks whether this <see cref="Optional{T}"/> (or its value) are equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="Optional{T}"/> or its value.</returns>
        public override bool Equals(object obj)
        {
            if (obj is T t)
                return this.Equals(t);

            if (obj is Optional<T> opt)
                return this.Equals(opt);

            return false;
        }

        /// <summary>
        /// Checks whether this <see cref="Optional{T}"/> is equal to another <see cref="Optional{T}"/>.
        /// </summary>
        /// <param name="e"><see cref="Optional{T}"/> to compare to.</param>
        /// <returns>Whether the <see cref="Optional{T}"/> is equal to this <see cref="Optional{T}"/>.</returns>
        public bool Equals(Optional<T> e)
        {
            if (!this.HasValue && !e.HasValue)
                return true;

            if (this.HasValue != e.HasValue)
                return false;

            return this.Value.Equals(e.Value);
        }

        /// <summary>
        /// Checks whether the value of this <see cref="Optional{T}"/> is equal to specified object.
        /// </summary>
        /// <param name="e">Object to compare to.</param>
        /// <returns>Whether the object is equal to the value of this <see cref="Optional{T}"/>.</returns>
        public bool Equals(T e)
        {
            if (!this.HasValue)
                return false;

            return ReferenceEquals(this.Value, e);
        }

        /// <summary>
        /// Gets the hash code for this <see cref="Optional{T}"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="Optional{T}"/>.</returns>
        public override int GetHashCode()
        {
            return this.HasValue ? this.Value.GetHashCode() : 0;
        }

        public static implicit operator Optional<T>(T val) =>
            new Optional<T>(val);

        public static explicit operator T(Optional<T> opt) =>
            opt.Value;

        public static bool operator ==(Optional<T> opt1, Optional<T> opt2) =>
            opt1.Equals(opt2);

        public static bool operator !=(Optional<T> opt1, Optional<T> opt2) =>
            !opt1.Equals(opt2);

        public static bool operator ==(Optional<T> opt, T t) =>
            opt.Equals(t);

        public static bool operator !=(Optional<T> opt, T t) =>
            !opt.Equals(t);
    }
}
