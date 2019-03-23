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
    /// Represents a value which may or may not have a value.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    [JsonConverter(typeof(OptionalJsonConverter))]
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
            return $"Optional<{typeof(T)}> ({(this.HasValue ? this.Value.ToString() : "<no value>")})";
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
