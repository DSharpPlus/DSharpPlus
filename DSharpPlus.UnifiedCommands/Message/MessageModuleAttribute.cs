namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// Defines that a module is here.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MessageModuleAttribute : Attribute
{
    public string? Name { get; init; }

    /// <summary>
    /// Constructor used for non group modules.
    /// </summary>
    public MessageModuleAttribute()
    {
    }

    /// <summary>
    /// Constructor used for
    /// </summary>
    /// <param name="name">The name for the module group. Can include spaces</param>
    public MessageModuleAttribute(string name)
        => Name = name;
}
