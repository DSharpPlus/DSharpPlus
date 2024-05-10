using System;
using System.Linq;
using DSharpPlus.Commands.Processors;

namespace DSharpPlus.Commands.Trees.Metadata;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowedProcessorsAttribute : Attribute
{
    public AllowedProcessorsAttribute(params Type[] processors)
    {
        if (processors.Any(x => x.IsAssignableTo(typeof(ICommandProcessor)) is false))
        {
            throw new ArgumentException("All processors must implement ICommandProcessor.", nameof(processors));
        }
        
        this.Processors = processors;
    } 

    public Type[] Processors { get; private set; }
}
