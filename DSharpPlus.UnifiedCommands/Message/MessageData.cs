using System.Reflection;

namespace DSharpPlus.UnifiedCommands.Message;

public class MessageData
{
    private MethodInfo _method;

    public string Name { get; private set; }
    // public x Options { get; set; }
    // public x Arguments { get; set; } 
    // Implement above later.

    internal MessageData(string name, MethodInfo method)
    {
        _method = method;
        Name = name;
    }

    public T? GetMetadata<T>() where T : Attribute => _method.GetCustomAttribute<T>();
}
