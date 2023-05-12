namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// The option attribute. Decides what name a option has.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class MessageOptionAttribute : Attribute
{
    public string Option { get; set; }
    public string? ShorthandOption { get; set; }

    /// <summary>
    /// The primary constructor for MessageOptionAttribute
    /// </summary>
    /// <param name="option">The primary option name. Example: --help</param>
    /// <param name="shorthandOption">The shorthand option name. Example: -h</param>
    public MessageOptionAttribute(string option, string? shorthandOption = null)
    {
        Option = option;
        ShorthandOption = shorthandOption;
    }
}
