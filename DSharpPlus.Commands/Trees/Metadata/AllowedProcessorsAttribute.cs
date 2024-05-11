using System;
using System.Linq;
using DSharpPlus.Commands.Processors;

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
    public AllowedProcessorsAttribute(params Type[] processors)
    {
        if (processors.Length < 1)
        {
            throw new ArgumentException("Provide atleast one processor", nameof(processors));
        }

        if (processors.Any(x => x.IsAssignableTo(typeof(ICommandProcessor)) is false))
        {
            throw new ArgumentException("All processors must implement ICommandProcessor.", nameof(processors));
        }

        this.Processors = processors;
    }

    public Type[] Processors { get; private set; }
}
