using System;

namespace DSharpPlus.CommandsNext.Exceptions;

/// <summary>
/// Thrown when the command service fails to find a command.
/// </summary>
public sealed class CommandNotFoundException : Exception
{
    /// <summary>
    /// Gets the name of the command that was not found.
    /// </summary>
    public string CommandName { get; set; }

    /// <summary>
    /// Creates a new <see cref="CommandNotFoundException"/>.
    /// </summary>
    /// <param name="command">Name of the command that was not found.</param>
    public CommandNotFoundException(string command)
        : base("Specified command was not found.") => this.CommandName = command;

    /// <summary>
    /// Returns a string representation of this <see cref="CommandNotFoundException"/>.
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString() => $"{this.GetType()}: {this.Message}\nCommand name: {this.CommandName}"; // much like System.ArgumentNullException works
}
