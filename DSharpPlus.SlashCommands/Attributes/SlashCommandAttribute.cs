using System;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Marks this method as a slash command.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class SlashCommandAttribute : Attribute
{
    /// <summary>
    /// Gets the name of this command.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of this command.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets whether this command is enabled by default.
    /// </summary>
    public bool DefaultPermission { get; }

    /// <summary>
    /// Gets whether this command is age restricted.
    /// </summary>
    public bool NSFW { get; }

    /// <summary>
    /// Marks this method as a slash command.
    /// </summary>
    /// <param name="name">Sets the name of this slash command.</param>
    /// <param name="description">Sets the description of this slash command.</param>
    /// <param name="defaultPermission">Sets whether the command should be enabled by default.</param>
    /// <param name="nsfw">Sets whether the command is age restricted.</param>
    public SlashCommandAttribute(string name, string description, bool defaultPermission = true, bool nsfw = false)
    {
        Name = name.ToLower();
        Description = description;
        DefaultPermission = defaultPermission;
        NSFW = nsfw;
    }
}
