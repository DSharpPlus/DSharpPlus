namespace DSharpPlus.UnifiedCommands.Message;

[AttributeUsage(AttributeTargets.Class)]
public class MessageModuleAttribute : Attribute
{
    public string? Name { get; set; } = null;

    public MessageModuleAttribute()
    {
    }

    public MessageModuleAttribute(string name)
        => Name = name;
}
