namespace DSharpPlus.Entities;

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
