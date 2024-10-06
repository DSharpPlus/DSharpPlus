using System;
using System.Linq;
using DSharpPlus.Commands.Processors;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.UserCommands;

namespace DSharpPlus.Commands.Trees.Metadata;

/// <summary>
/// Allows to restrict commands to certain processors.
/// </summary>
/// <remarks>
/// This attribute only works on top-level commands.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowedProcessorsAttribute : Attribute
{
    /// <summary>
    /// Specifies which processors are allowed to execute this command.
    /// </summary>
    /// <param name="processors">Types of processors that are allowed to execute this command.</param>
    public AllowedProcessorsAttribute(params Type[] processors)
    {
        if (processors.Length < 1)
        {
            throw new ArgumentException("Provide atleast one processor", nameof(processors));
        }

        if (!processors.All(x => x.IsAssignableTo(typeof(ICommandProcessor))))
        {
            throw new ArgumentException(
                "All processors must implement ICommandProcessor.",
                nameof(processors)
            );
        }

        this.Processors = (processors.Contains(typeof(MessageCommandProcessor))
            || processors.Contains(typeof(UserCommandProcessor))) && !processors.Contains(typeof(SlashCommandProcessor))
                ? [.. processors, typeof(SlashCommandProcessor)]
                : processors;
    }

    /// <summary>
    /// Types of allowed processors
    /// </summary>
    public Type[] Processors { get; private set; }
}

/// <inheritdoc />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowedProcessorsAttribute<T> : AllowedProcessorsAttribute where T : ICommandProcessor
{
    /// <inheritdoc />
    public AllowedProcessorsAttribute() : base(typeof(T)) { }
}

/// <inheritdoc />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowedProcessorsAttribute<T1, T2> : AllowedProcessorsAttribute
    where T1 : ICommandProcessor
    where T2 : ICommandProcessor
{
    /// <inheritdoc />
    public AllowedProcessorsAttribute() : base(typeof(T1), typeof(T2)) { }
}

/// <inheritdoc />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowedProcessorsAttribute<T1, T2, T3> : AllowedProcessorsAttribute
    where T1 : ICommandProcessor
    where T2 : ICommandProcessor
    where T3 : ICommandProcessor
{
    /// <inheritdoc />
    public AllowedProcessorsAttribute() : base(typeof(T1), typeof(T2), typeof(T3)) { }
}

/// <inheritdoc />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowedProcessorsAttribute<T1, T2, T3, T4> : AllowedProcessorsAttribute
    where T1 : ICommandProcessor
    where T2 : ICommandProcessor
    where T3 : ICommandProcessor
    where T4 : ICommandProcessor
{
    /// <inheritdoc />
    public AllowedProcessorsAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4)) { }
}

/// <inheritdoc />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowedProcessorsAttribute<T1, T2, T3, T4, T5> : AllowedProcessorsAttribute
    where T1 : ICommandProcessor
    where T2 : ICommandProcessor
    where T3 : ICommandProcessor
    where T4 : ICommandProcessor
    where T5 : ICommandProcessor
{
    /// <inheritdoc />
    public AllowedProcessorsAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)) { }
}
