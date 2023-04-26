namespace DSharpPlus.CH.Message.Internals.Exceptions;

public class ConvertionFailedException : Exception
{
    public InvalidMessageConvertionType Type { get; private set; }
    public string Value { get; private set; }

    public ConvertionFailedException(string value, InvalidMessageConvertionType type)
    {
        Value = value;
        Type = type;
    }
}
