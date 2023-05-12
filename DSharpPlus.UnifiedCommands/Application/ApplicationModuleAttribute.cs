namespace DSharpPlus.UnifiedCommands.Application;

[AttributeUsage(AttributeTargets.Class)]
public class ApplicationModuleAttribute : Attribute
{
    public string? Name { get; }
    public string? Description { get; }

    public ApplicationModuleAttribute()
    {
    }

    public ApplicationModuleAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
