namespace DSharpPlus.UnifiedCommands.Application;

[AttributeUsage(AttributeTargets.Class)]
public class ApplicationModuleAttribute : Attribute
{
    public string? Name { get; }

    public ApplicationModuleAttribute(string? name = null)
        => Name = name;
}
