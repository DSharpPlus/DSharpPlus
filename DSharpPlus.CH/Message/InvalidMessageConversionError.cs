namespace DSharpPlus.CH.Message;

public class InvalidMessageConversionError
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public required bool IsPositionalArgument { get; set; }
    public required InvalidMessageConvertionType Type { get; set; }
}
