namespace DSharpPlus.UnifiedCommands.Message.Internals.Exceptions;

public class ConversionFailedException : Exception
{
    public InvalidMessageConversionType Type { get; private set; }
    public string Value { get; private set; }
    public bool IsPositionalArgument { get; private set; }
    public string Name { get; private set; }

    public ConversionFailedException(string value, InvalidMessageConversionType type, bool isPositionalArgument, string name)
    {
        Value = value;
        Type = type;
        IsPositionalArgument = isPositionalArgument;
        Name = name;
    }
}
