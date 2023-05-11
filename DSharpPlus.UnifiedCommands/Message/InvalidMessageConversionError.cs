namespace DSharpPlus.UnifiedCommands.Message;

public class InvalidMessageConversionError
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public required bool IsPositionalArgument { get; set; }
    public required InvalidMessageConversionType Type { get; set; }
}
