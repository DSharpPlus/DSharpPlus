namespace DSharpPlus.CH.Application;

[AttributeUsage(AttributeTargets.Parameter)]
public class ApplicationNameAttribute : Attribute
{
    public string Name { get; }
    // public Type Type { get; }
    
    public ApplicationNameAttribute(string name)
        => Name = name;
}
