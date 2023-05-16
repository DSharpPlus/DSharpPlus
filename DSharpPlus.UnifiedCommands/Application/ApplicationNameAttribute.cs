namespace DSharpPlus.UnifiedCommands.Application;

[AttributeUsage(AttributeTargets.Method)]
public class ApplicationNameAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public ApplicationNameAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
