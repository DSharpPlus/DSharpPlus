using System;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// Allows to restrict commands to certain handlers.
/// </summary>
/// <remarks>
/// This attribute only works on top-level commands.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowedCommandHandlersAttribute : Attribute
{
    /// <summary>
    /// Specifies which handlers are allowed to execute this command.
    /// </summary>
    /// <param name="handlers">Types of handlers that are allowed to execute this command.</param>
    public AllowedCommandHandlersAttribute(params Type[] handlers)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(handlers.Length, 0, nameof(handlers));
        this.Handlers = handlers;
    }

    /// <summary>
    /// Types of allowed handlers.
    /// </summary>
    public Type[] Handlers { get; private set; }
}
