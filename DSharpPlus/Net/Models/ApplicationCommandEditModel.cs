using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying an application command.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class ApplicationCommandEditModel
{
    /// <summary>
    /// Sets the command's new name.
    /// </summary>
    public Optional<string> Name
    {
        get => this.name;
        set
        {
            if (value.Value.Length > 32)
            {
                throw new ArgumentException("Application command name cannot exceed 32 characters.", nameof(value));
            }

            this.name = value;
        }
    }

    private Optional<string> name;

    /// <summary>
    /// Sets the command's new description
    /// </summary>
    public Optional<string> Description
    {
        get => this.description;
        set
        {
            if (value.Value.Length > 100)
            {
                throw new ArgumentException("Application command description cannot exceed 100 characters.", nameof(value));
            }

            this.description = value;
        }
    }

    private Optional<string> description;

    /// <summary>
    /// Sets the command's new options.
    /// </summary>
    public Optional<List<DiscordApplicationCommandOption>> Options { get; set; }

    /// <summary>
    /// Sets whether the command is enabled by default when the application is added to a guild.
    /// </summary>
    public Optional<bool?> DefaultPermission { get; set; }

    /// <summary>
    /// Sets whether the command can be invoked in DMs.
    /// </summary>
    public Optional<bool> AllowDMUsage { get; set; }

    /// <summary>
    /// Sets the requisite permissions for the command.
    /// </summary>
    public Optional<DiscordPermissions?> DefaultMemberPermissions { get; set; }

    /// <summary>
    /// Sets whether this command is age restricted.
    /// </summary>
    public Optional<bool?> NSFW { get; set; }
}
