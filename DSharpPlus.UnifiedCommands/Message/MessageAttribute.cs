namespace DSharpPlus.UnifiedCommands.Message;

[AttributeUsage(AttributeTargets.Method)]
public class MessageAttribute : Attribute
{
    public string Name { get; set; }

    public MessageAttribute(string name) => Name = name;
}
