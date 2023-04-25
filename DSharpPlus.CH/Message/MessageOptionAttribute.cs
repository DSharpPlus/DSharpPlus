namespace DSharpPlus.CH.Message;

[AttributeUsage(AttributeTargets.Parameter)]
public class MessageOptionAttribute : Attribute
{
    public string Option { get; set; }
    public string? ShorthandOption { get; set; }

    public MessageOptionAttribute(string option, string? shorthandOption = null)
    {
        Option = option;
        ShorthandOption = shorthandOption;
    }
}