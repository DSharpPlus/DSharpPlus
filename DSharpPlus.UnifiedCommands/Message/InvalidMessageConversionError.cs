namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// The primary class used when a invalid conversion happens.
/// </summary>
public class InvalidMessageConversionError
{
    /// <summary>
    /// The for the option/positional argument.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// The value for said positional argument/option.
    /// </summary>
    public required string Value { get; set; }
    /// <summary>
    /// If parameter is a argument or not.
    /// </summary>
    public required bool IsPositionalArgument { get; set; }
    /// <summary>
    /// The type of error that happened.
    /// </summary>
    public required InvalidMessageConversionType Type { get; set; }
}
