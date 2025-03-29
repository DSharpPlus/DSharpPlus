using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees.Metadata;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// A builder type for creating a single specific command overload.
/// </summary>
public sealed class CommandOverloadBuilder
{
    private string? name;
    private readonly List<ParameterNodeBuilder> parameters = [];
    private readonly List<INodeMetadataItem> metadataItems = [];
    private readonly List<ContextCheckAttribute> checkAttributes = [];

    /// <summary>
    /// The parameters of this command, excluding the command context.
    /// </summary>
    public IReadOnlyList<ParameterNodeBuilder> Parameters => this.parameters;

    /// <summary>
    /// The executing function for this command.
    /// </summary>
    public Func<CommandContext, object?[], IServiceProvider, ValueTask>? Execute { get; private set; }

    /// <summary>
    /// The check attributes applicable to this command. This does not necessarily correlate to the list of executed checks,
    /// which may vary based on context and registered check implementations.
    /// </summary>
    public IReadOnlyList<ContextCheckAttribute> CheckAttributes => this.checkAttributes;

    /// <summary>
    /// Additional metadata associated with this command.
    /// </summary>
    public NodeMetadataCollection Metadata => new([.. this.metadataItems.DistinctBy(x => x.GetType())]);

    /// <summary>
    /// Adds a parameter to this overload.
    /// </summary>
    public CommandOverloadBuilder AddParameter(ParameterNodeBuilder parameter)
    {
        this.parameters.Add(parameter);
        return this;
    }

    /// <summary>
    /// Adds the check attribute to this parameter. Note that check attributes will be deduplicated.
    /// </summary>
    public CommandOverloadBuilder AddCheckAttribute(ContextCheckAttribute attribute)
    {
        this.checkAttributes.Add(attribute);
        return this;
    }

    /// <summary>
    /// Adds the check attribute to this parameter. Note that check attributes will be deduplicated.
    /// </summary>
    public CommandOverloadBuilder AddCheckAttributes(params IEnumerable<ContextCheckAttribute> attributes)
    {
        this.checkAttributes.AddRange(attributes);
        return this;
    }

    /// <summary>
    /// Adds a new check attribute only if there is no preexisting check attribute of the same type present.
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public CommandOverloadBuilder AddNonDuplicateCheckAttribute(ContextCheckAttribute attribute)
    {
        if (!this.checkAttributes.Any(x => x.GetType() == attribute.GetType()))
        {
            this.checkAttributes.Add(attribute);
        }

        return this;
    }

    /// <summary>
    /// Adds the specified metadata item to this parameter. Note that node metadata is deduplicated by type.
    /// </summary>
    public CommandOverloadBuilder AddMetadataItem(INodeMetadataItem item)
    {
        this.metadataItems.Add(item);
        return this;
    }

    /// <summary>
    /// Adds the specified metadata item to this parameter. Note that node metadata is deduplicated by type.
    /// </summary>
    public CommandOverloadBuilder AddMetadataItems(params IEnumerable<INodeMetadataItem> items)
    {
        this.metadataItems.AddRange(items);
        return this;
    }
}
