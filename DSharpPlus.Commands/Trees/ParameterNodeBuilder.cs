using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Commands.Trees.Metadata;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// A builder type for creating a parameter node.
/// </summary>
public sealed class ParameterNodeBuilder
{
    private readonly List<INodeMetadataItem> metadataItems = [];
    private readonly List<ParameterCheckAttribute> checkAttributes = [];

    /// <summary>
    /// The unaltered name of this parameter.
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// The lowercased name of this parameter.
    /// </summary>
    public string? LowercasedName { get; private set; }

    /// <summary>
    /// The description of this parameter.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// The type of this parameter.
    /// </summary>
    public Type? ParameterType { get; private set; }

    /// <summary>
    /// Parameter check data attributes found on this parameter. This does not control which checks will be executed, which is handled by the extension.
    /// </summary>
    public IReadOnlyList<ParameterCheckAttribute>? CheckAttributes => this.checkAttributes;

    /// <summary>
    /// Indicates whether this parameter must be specified.
    /// </summary>
    public bool? IsRequired { get; private set; }

    /// <summary>
    /// The default value of this parameter, if it was not required and not specified at invocation.
    /// </summary>
    /// <remarks>
    /// This value is undefined for required parameters and must not be relied upon.
    /// </remarks>
    public object? DefaultValue { get; private set; }

    /// <summary>
    /// Additional metadata associated with this command.
    /// </summary>
    public NodeMetadataCollection Metadata => new([.. this.metadataItems.DistinctBy(x => x.GetType())]);

    /// <summary>
    /// Sets the name of this parameter.
    /// </summary>
    public ParameterNodeBuilder WithName(string name)
    {
        this.Name = name;
        return this;
    }

    /// <summary>
    /// Sets the lowercased name of this parameter.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the name was, in fact, not all-lowercase.</exception>
    public ParameterNodeBuilder WithLowercasedName(string lowercased)
    {
        if (lowercased.Any(char.IsHighSurrogate))
        {
            // slow path that can handle surrogate pairs

            foreach (Rune rune in lowercased.AsSpan().EnumerateRunes())
            {
                if (Rune.IsUpper(rune))
                {
                    throw new ArgumentException("The provided string must be entirely lowercase.");
                }
            }
        }
        else if (lowercased.Any(char.IsUpper))
        {
            throw new ArgumentException("The provided string must be entirely lowercase.");
        }

        this.LowercasedName = lowercased;
        return this;
    }

    /// <summary>
    /// Sets the description of this parameter.
    /// </summary>
    public ParameterNodeBuilder WithDescription(string description)
    {
        this.Description = description;
        return this;
    }

    /// <summary>
    /// Sets the type of this parameter.
    /// </summary>
    public ParameterNodeBuilder WithType(Type type)
    {
        this.ParameterType = type;
        return this;
    }

    /// <summary>
    /// Adds the check attribute to this parameter. Note that check attributes will be deduplicated.
    /// </summary>
    public ParameterNodeBuilder AddCheckAttribute(ParameterCheckAttribute attribute)
    {
        this.checkAttributes.Add(attribute);
        return this;
    }

    /// <summary>
    /// Adds the check attribute to this parameter. Note that check attributes will be deduplicated.
    /// </summary>
    public ParameterNodeBuilder AddCheckAttributes(params IEnumerable<ParameterCheckAttribute> attributes)
    {
        this.checkAttributes.AddRange(attributes);
        return this;
    }

    /// <summary>
    /// Sets the default value for this parameter and marks it as optional.
    /// </summary>
    public ParameterNodeBuilder WithDefaultValue(object? defaultValue)
    {
        this.IsRequired = false;
        this.DefaultValue = defaultValue;
        return this;
    }

    /// <summary>
    /// adds the specified metadata item to this parameter. Note that node metadata is deduplicated by type.
    /// </summary>
    public ParameterNodeBuilder AddMetadataItem(INodeMetadataItem item)
    {
        this.metadataItems.Add(item);
        return this;
    }

    /// <summary>
    /// adds the specified metadata item to this parameter. Note that node metadata is deduplicated by type.
    /// </summary>
    public ParameterNodeBuilder AddMetadataItems(params IEnumerable<INodeMetadataItem> items)
    {
        this.metadataItems.AddRange(items);
        return this;
    }

    /// <summary>
    /// Sets the minimum and maximum value for this parameter.
    /// </summary>
    public ParameterNodeBuilder WithMinMaxValue(object minValue, object maxValue)
    {
        Debug.Assert(minValue.GetType() == maxValue.GetType());

        MinMaxValueMetadata metadataItem = new()
        {
            MinValue = minValue,
            MaxValue = maxValue
        };

        return AddMetadataItem(metadataItem);
    }

    /// <summary>
    /// Sets the minimum and maximum length for this parameter;
    /// </summary>
    public ParameterNodeBuilder WithMinMaxLength(int minLength, int maxLength)
    {
        MinMaxLengthMetadata metadataItem = new()
        {
            MinLength = minLength,
            MaxLength = maxLength
        };

        return AddMetadataItem(metadataItem);
    }
}
