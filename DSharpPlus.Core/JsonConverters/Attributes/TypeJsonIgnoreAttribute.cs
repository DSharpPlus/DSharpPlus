using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.JsonConverters.Attributes
{
    /// <summary>
    /// Specifies the condition under which any property of this type should be ignored if using the <see cref="ReflectJsonConverter{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class TypeJsonIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeJsonIgnoreAttribute"/> class.
        /// </summary>
        /// <param name="condition">The condition under which to ignore properties.</param>
        public TypeJsonIgnoreAttribute(JsonIgnoreCondition condition) => Condition = condition;

        /// <summary>
        /// Gets the condition under which to ignore properties.
        /// </summary>
        public JsonIgnoreCondition Condition { get; }
    }
}
