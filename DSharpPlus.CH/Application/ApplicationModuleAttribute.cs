namespace DSharpPlus.CH.Application;

[AttributeUsage(AttributeTargets.Class)]
public class ApplicationModuleAttribute : Attribute
{
    public string Name { get; }

    public ApplicationModuleAttribute(string name)
        => Name = name;
}
