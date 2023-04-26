using System.Reflection;

namespace DSharpPlus.CH.Message;

public class MessageCommandData
{
    private MethodInfo _method;

    public string Name { get; set; }
    // public x Options { get; set; }
    // public x Arguments { get; set; } 
    // Implement above later.

    internal MessageCommandData(string name, MethodInfo method)
    {
        _method = method;
        Name = name;
    }

    public T? GetMetadata<T>() where T : Attribute => _method.GetCustomAttribute<T>();
}
