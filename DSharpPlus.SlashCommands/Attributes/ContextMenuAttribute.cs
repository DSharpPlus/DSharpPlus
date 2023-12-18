using System;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Marks this method as a context menu.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ContextMenuAttribute : Attribute
{
    /// <summary>
    /// Gets the name of this context menu.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the type of this context menu.
    /// </summary>
    public ApplicationCommandType Type { get; internal set; }

    /// <summary>
    /// Gets whether this command is enabled by default.
    /// </summary>
    public bool DefaultPermission { get; internal set; }

    /// <summary>
    /// Gets whether this command is age restricted.
    /// </summary>
    public bool NSFW { get; }

    /// <summary>
    /// Marks this method as a context menu.
    /// </summary>
    /// <param name="type">The type of the context menu.</param>
    /// <param name="name">The name of the context menu.</param>
    /// <param name="defaultPermission">Sets whether the command should be enabled by default.</param>
    /// <param name="nsfw">Sets whether the command is age restricted.</param>
    public ContextMenuAttribute(ApplicationCommandType type, string name, bool defaultPermission = true, bool nsfw = false)
    {
        if (type == ApplicationCommandType.SlashCommand)
        {
            throw new ArgumentException("Context menus cannot be of type SlashCommand.");
        }

        this.Type = type;
        this.Name = name;
        this.DefaultPermission = defaultPermission;
        this.NSFW = nsfw;
    }
}
