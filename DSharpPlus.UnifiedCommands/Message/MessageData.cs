using System.Reflection;

namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// Data related to the current execution.
/// </summary>
public class MessageData
{
    private MethodInfo _method;

    /// <summary>
    /// The name of the method/command.
    /// </summary>
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
