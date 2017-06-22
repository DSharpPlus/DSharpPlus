namespace DSharpPlus.CommandsNext.Converters
{
    /// <summary>
    /// Argument converter abstract.
    /// </summary>
    public interface IArgumentConverter
    { }

    /// <summary>
    /// Represents a converter for specific argument type.
    /// </summary>
    /// <typeparam name="T">Type for which the converter is to be registered.</typeparam>
    public interface IArgumentConverter<T> : IArgumentConverter
    {
        bool TryConvert(string value, CommandContext ctx, out T result);
    }
}
