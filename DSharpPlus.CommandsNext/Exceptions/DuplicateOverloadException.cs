namespace DSharpPlus.CommandsNext.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Indicates that given argument set already exists as an overload for specified command.
/// </summary>
public class DuplicateOverloadException : Exception
{
    /// <summary>
    /// Gets the name of the command that already has the overload.
    /// </summary>
    public string CommandName { get; }

    /// <summary>
    /// Gets the ordered collection of argument types for the specified overload.
    /// </summary>
    public IReadOnlyList<Type> ArgumentTypes { get; }

    private string ArgumentSetKey { get; }

    /// <summary>
    /// Creates a new exception indicating given argument set already exists as an overload for specified command.
    /// </summary>
    /// <param name="name">Name of the command with duplicated argument sets.</param>
    /// <param name="argumentTypes">Collection of ordered argument types for the command.</param>
    /// <param name="argumentSetKey">Overload identifier.</param>
    internal DuplicateOverloadException(string name, IList<Type> argumentTypes, string argumentSetKey)
        : base("An overload with specified argument types exists.")
    {
        CommandName = name;
        ArgumentTypes = new ReadOnlyCollection<Type>(argumentTypes);
        ArgumentSetKey = argumentSetKey;
    }

    /// <summary>
    /// Returns a string representation of this <see cref="DuplicateOverloadException"/>.
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString() => $"{GetType()}: {Message}\nCommand name: {CommandName}\nArgument types: {ArgumentSetKey}"; // much like System.ArgumentException works
}
