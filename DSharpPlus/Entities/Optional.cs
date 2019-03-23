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
            => new Optional<T>(value);

        /// <summary>
        /// Creates a new empty <see cref="Optional{T}"/> with no value and invalid state.
        /// </summary>
        /// <typeparam name="T">The type that the created instance is wrapping around.</typeparam>
        /// <returns>Created optional.</returns>
        public static Optional<T> FromNoValue<T>()
            => default;
    }
    
    /// <summary>
    /// Represents a wrapper which may or may not have a value.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    [JsonConverter(typeof(OptionalJsonConverter))]
    public readonly struct Optional<T> : IEquatable<Optional<T>>, IEquatable<T>
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
        public override string ToString()
        {
            return $"Optional<{typeof(T)}> ({(this.HasValue ? this.Value.ToString() : "<no value>")})";
        }

        /// <summary>
        /// Checks whether this <see cref="Optional{T}"/> (or its value) are equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="Optional{T}"/> or its value.</returns>
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case T t:
                    return this.Equals(t);
                case Optional<T> opt:
                    return this.Equals(opt);
                default:
                    return false;
            }
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

            return this.HasValue == e.HasValue && this.Value.Equals(e.Value);
        }

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
        public Optional<TTarget> IfPresent<TTarget>(Func<T, TTarget> mapper)
        {
            return HasValue ? new Optional<TTarget>(mapper(Value)) : default;
        }
    }
    
    /// <seealso cref="DiscordJson.Serializer"/>
    internal sealed class OptionalJsonContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            var type = property.PropertyType;
            
            if (!type.GetTypeInfo().IsGenericType)
                return property;

            if (type.GetGenericTypeDefinition() != typeof(Optional<>))
                return property;
            
            // we cache the PropertyInfo object here (it's captured in closure). we don't have direct 
            // access to the property value so we have to reflect into it from the parent instance
            // we use UnderlyingName instead of PropertyName in case the C# name is different from the Json name.
            var optionalProp = property.DeclaringType.GetTypeInfo().GetDeclaredProperty(property.UnderlyingName);
            
            // DIRTY HACK to support both serializing both fields and properties, if you know a better way to do this
            // write me at cadrekucra@gmx.com!!!!
            var propPresent = optionalProp != null;
            var optionalField = !propPresent
                ? property.DeclaringType.GetTypeInfo().GetDeclaredField(property.UnderlyingName)
                : null;
            
            property.ShouldSerialize = instance => // instance here is the declaring (parent) type
            {
                // this is the Optional<T> object
                var optionalValue = propPresent ? optionalProp.GetValue(instance) : optionalField.GetValue(instance);
                // get the HasValue property of the Optional<T> object and cast it to a bool, and only serialize it if
                // it's present
                return (bool)optionalValue.GetType().GetTypeInfo().GetDeclaredProperty("HasValue").GetValue(optionalValue);
            };
    
            return property;
        }
    }

    internal sealed class OptionalJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var typeInfo = value.GetType().GetTypeInfo();
            // we don't check for HasValue here since it's checked above
            var val = typeInfo.GetDeclaredProperty("Value").GetValue(value);
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
                var t = JToken.FromObject(val);
                t.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var genericType = objectType.GenericTypeArguments[0];

            var constructor = objectType.GetTypeInfo().DeclaredConstructors
                .Single(e => e.GetParameters()[0].ParameterType == genericType);
            
            try
            {
                return constructor.Invoke(new[] { Convert.ChangeType(reader.Value, genericType)});
            }
            catch
            {
                return existingValue;
            }
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            if (!objectType.GetTypeInfo().IsGenericType) return false;

            return objectType.GetGenericTypeDefinition() == typeof(Optional<>);
        }
    }
}
