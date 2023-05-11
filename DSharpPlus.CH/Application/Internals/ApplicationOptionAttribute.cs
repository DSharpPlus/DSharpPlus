namespace DSharpPlus.CH.Application.Internals;

[AttributeUsage(AttributeTargets.Parameter)]
public class ApplicationOptionAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    
    public ApplicationOptionAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
