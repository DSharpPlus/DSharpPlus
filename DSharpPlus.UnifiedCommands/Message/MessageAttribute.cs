namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// Attribute used to set command names for methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MessageAttribute : Attribute
{
    public string Name { get; set; }

    /// <summary>
    /// The default constructor.
    /// </summary>
    /// <param name="name">The default name.</param>
    /// <param name="alias">Aliases to the command.</param>
    public MessageAttribute(string name, params string[] alias) => Name = name;
}
