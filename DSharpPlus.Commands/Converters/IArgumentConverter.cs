using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public interface IArgumentConverter
{
    public string ReadableName { get; }

    /// <summary>
    /// Finds the base type to use for converter registration.
    /// </summary>
    /// <remarks>
    /// More specifically, this methods returns the base type that can be found from <see cref="Nullable{T}"/>, <see cref="Enum"/>, or <see cref="Array"/>'s.
    /// </remarks>
    /// <param name="type">The type to find the base type for.</param>
    /// <returns>The base type to use for converter registration.</returns>
    public static Type GetConverterFriendlyBaseType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        Type effectiveType = Nullable.GetUnderlyingType(type) ?? type;
        if (effectiveType.IsArray)
        {
            // The type could be an array of enums or nullable
            // objects or worse: an array of arrays.
            return GetConverterFriendlyBaseType(effectiveType.GetElementType()!);
        }

        return effectiveType;
    }
}

/// <summary>
/// Converts an argument to a desired type.
/// </summary>
/// <typeparam name="TOutput">The type to convert the argument to.</typeparam>
public interface IArgumentConverter<TOutput> : IArgumentConverter
{
    /// <summary>
    /// Converts the argument to the desired type.
    /// </summary>
    /// <param name="context">The context for this conversion.</param>
    /// <returns>An optional containing the converted value, or an empty optional if the conversion failed.</returns>
    public Task<Optional<TOutput>> ConvertAsync(ConverterContext context);
}
