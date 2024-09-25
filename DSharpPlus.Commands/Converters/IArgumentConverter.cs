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
    /// <param name="flags">The flags to use for the search.</param>
    /// <returns>The base type to use for converter registration.</returns>
    public static Type GetConverterFriendlyBaseType(Type type, ConverterFriendlySearchFlags flags = ConverterFriendlySearchFlags.None)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        Type effectiveType = Nullable.GetUnderlyingType(type) ?? type;
        if (effectiveType.IsEnum)
        {
            return flags.HasFlag(ConverterFriendlySearchFlags.ReturnEnumType) ? effectiveType : typeof(Enum);
        }
        else if (effectiveType.IsArray)
        {
            // The type could be an array of enums or nullable
            // objects or worse: an array of arrays.
            return GetConverterFriendlyBaseType(effectiveType.GetElementType()!, flags);
        }

        return effectiveType;
    }
}

public interface IArgumentConverter<TOutput> : IArgumentConverter
{
    public Task<Optional<TOutput>> ConvertAsync(ConverterContext context);
}

/// <summary>
/// Any search parameters to use when finding the base type for converter mapping.
/// </summary>
[Flags]
public enum ConverterFriendlySearchFlags
{
    /// <summary>
    /// No flags.
    /// </summary>
    None = 0,

    /// <summary>
    /// Returns the type if the type is an enum.
    /// By default, the base type of <see cref="Enum"/> is returned
    /// if the type is an enum.
    /// </summary>
    ReturnEnumType = 1 << 0,
}
