namespace DSharpPlus.CommandsNext.Converters
{
    /// <summary>
    /// Argument converter abstract.
    /// </summary>
    interface IArgumentConverter
    { }

    /// <summary>
    /// Represents a converter for specific argument type.
    /// </summary>
    /// <typeparam name="T">Type for which the converter is to be registered.</typeparam>
    interface IArgumentConverter<T> : IArgumentConverter
    {
        bool TryConvert(string value, CommandContext ctx, out T result);
    }
}
