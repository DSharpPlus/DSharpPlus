using System;

namespace DSharpPlus.CommandAll.Converters.Meta
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate)]
    public class ConverterAttribute : Attribute
    {
        public Type ParameterType { get; init; }
        public ConverterAttribute(Type parameterType) => ParameterType = parameterType;
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate)]
    public class ConverterAttribute<T> : ConverterAttribute
    {
        public ConverterAttribute() : base(typeof(T)) { }
    }
}
