using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Trees.Metadata;

using BclDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using DspDescriptionAttribute = DSharpPlus.Commands.DescriptionAttribute;

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
    public IReadOnlyList<ParameterCheckAttribute> CheckAttributes => this.checkAttributes;

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
    /// Indicates whether this parameter will greedily match, that is, consume as much text as possible rather than as little as possible. If this is the final
    /// parameter, all remaining text will be considered either part of this parameter or treated as excess.
    /// </summary>
    public bool IsGreedy { get; private set; }

    /// <summary>
    /// Additional metadata associated with this command.
    /// </summary>
    public NodeMetadataCollection Metadata => new([.. this.metadataItems.DistinctBy(x => x.GetType())]);

    /// <summary>
    /// Sets the name of this parameter.
    /// </summary>
    [MemberNotNull(nameof(Name))]
    public ParameterNodeBuilder WithName(string name)
    {
        this.Name = name;
        return this;
    }

    /// <summary>
    /// Sets the lowercased name of this parameter.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the name was, in fact, not all-lowercase.</exception>
    [MemberNotNull(nameof(LowercasedName))]
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
    [MemberNotNull(nameof(Description))]
    public ParameterNodeBuilder WithDescription(string description)
    {
        this.Description = description;
        return this;
    }

    /// <summary>
    /// Sets the type of this parameter.
    /// </summary>
    [MemberNotNull(nameof(ParameterType))]
    public ParameterNodeBuilder WithType(Type type)
    {
        this.ParameterType = type;
        return this;
    }

    /// <summary>
    /// Sets this parameter to match greedily. If this is specified, the parameter will consider as much text as possible rather than as little text as possible.
    /// </summary>
    [MemberNotNull(nameof(IsGreedy))]
    public ParameterNodeBuilder AsGreedy()
    {
        this.IsGreedy = true;
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
    /// Adds a new check attribute only if there is no preexisting check attribute of the same type present.
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public ParameterNodeBuilder AddNonDuplicateCheckAttribute(ParameterCheckAttribute attribute)
    {
        if (!this.checkAttributes.Any(x => x.GetType() == attribute.GetType()))
        {
            this.checkAttributes.Add(attribute);
        }

        return this;
    }

    /// <summary>
    /// Sets the default value for this parameter and marks it as optional.
    /// </summary>
    [MemberNotNull(nameof(IsRequired))]
    public ParameterNodeBuilder WithDefaultValue(object? defaultValue)
    {
        this.IsRequired = false;
        this.DefaultValue = defaultValue;
        return this;
    }

    /// <summary>
    /// Adds the specified metadata item to this parameter. Note that node metadata is deduplicated by type.
    /// </summary>
    public ParameterNodeBuilder AddMetadataItem(INodeMetadataItem item)
    {
        this.metadataItems.Add(item);
        return this;
    }

    /// <summary>
    /// Adds the specified metadata item to this parameter. Note that node metadata is deduplicated by type.
    /// </summary>
    public ParameterNodeBuilder AddMetadataItems(params IEnumerable<INodeMetadataItem> items)
    {
        this.metadataItems.AddRange(items);
        return this;
    }

    /// <summary>
    /// Sets the minimum and maximum value for this parameter.
    /// </summary>
    public ParameterNodeBuilder WithMinMaxValue(object? minValue = null, object? maxValue = null)
    {
        if (minValue is not null && maxValue is not null)
        {
            Debug.Assert(minValue.GetType() == maxValue.GetType());
        }

        AddNonDuplicateCheckAttribute(new MinMaxValueAttribute(minValue, maxValue));

        MinMaxValueMetadata metadataItem = new()
        {
            MinValue = minValue,
            MaxValue = maxValue
        };

        return AddMetadataItem(metadataItem);
    }

    /// <summary>
    /// Sets the minimum and maximum length for this parameter.
    /// </summary>
    public ParameterNodeBuilder WithMinMaxLength(int minLength = 0, int maxLength = 6000)
    {
        AddNonDuplicateCheckAttribute(new MinMaxLengthAttribute(minLength, maxLength));

        MinMaxLengthMetadata metadataItem = new()
        {
            MinLength = minLength,
            MaxLength = maxLength
        };

        return AddMetadataItem(metadataItem);
    }

    /// <summary>
    /// Designates this parameter as a variadic parameter, meaning it can take between <paramref name="requiredArguments"/> and
    /// <paramref name="maxArguments"/> (both inclusive) arguments.
    /// </summary>
    public ParameterNodeBuilder AsVariadicParameter(int requiredArguments, int maxArguments)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(requiredArguments, 1, nameof(requiredArguments));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxArguments, 1, nameof(maxArguments));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxArguments, requiredArguments, nameof(maxArguments));

        VariadicParameterMetadata metadataItem = new()
        {
            MinVariadicArguments = requiredArguments,
            MaxVariadicArguments = maxArguments
        };

        return AddMetadataItem(metadataItem);
    }

    /// <summary>
    /// Specifies a choice provider for this parameter.
    /// </summary>
    public ParameterNodeBuilder WithChoiceProvider(Type choiceProvider)
    {
        ChoiceProviderMetadata metadataItem = new()
        {
            ChoiceProvider = choiceProvider
        };

        return AddMetadataItem(metadataItem);
    }

    /// <summary>
    /// Specifies an autocomplete provider for this parameter.
    /// </summary>
    public ParameterNodeBuilder WithAutocompleteProvider(Type autocompleteProvider)
    {
        AutocompleteProviderMetadata metadataItem = new()
        {
            AutocompleteProvider = autocompleteProvider
        };

        return AddMetadataItem(metadataItem);
    }

    /// <summary>
    /// Specifies that this parameter may be sourced from a reply.
    /// </summary>
    /// <param name="requireReply">Indicates whether it is required to be sourced from a reply.</param>
    public ParameterNodeBuilder AllowReply(bool requireReply)
    {
        ReplyMetadata metadataItem = new()
        {
            IsAllowed = true,
            IsRequired = requireReply
        };

        return AddMetadataItem(metadataItem);
    }

    /// <summary>
    /// Builds this parameter builder into a complete node.
    /// </summary>
    /// <param name="namingPolicy">The naming policy in use by the extension.</param>
    internal ParameterNode Build(IInteractionNamingPolicy namingPolicy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(this.Name);
        ArgumentNullException.ThrowIfNull(this.ParameterType);

        if (this.LowercasedName is null)
        {
            WithLowercasedName(namingPolicy.TransformText(this.Name, CultureInfo.InvariantCulture));
        }

        if (this.Description is null)
        {
            WithDescription("No description provided.");
        }

        return new ParameterNode()
        {
            Name = this.Name,
            LowercasedName = this.LowercasedName!,
            Description = this.Description!,
            ParameterType = this.ParameterType,
            CheckAttributes = this.CheckAttributes,
            IsRequired = this.IsRequired ?? true,
            DefaultValue = this.DefaultValue,
            IsGreedy = this.IsGreedy,
            Metadata = this.Metadata
        };
    }

    /// <summary>
    /// Creates a new parameter node builder from a given C# parameter.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the parameter was a context type.</exception>
    public static ParameterNodeBuilder FromParameterInfo(ParameterInfo info)
    {
        if (info.ParameterType.IsAssignableTo(typeof(AbstractContext)))
        {
            throw new ArgumentException("Command contexts are not valid command arguments.");
        }

        ParameterNodeBuilder builder = new();

        IEnumerable<Attribute> attributes = info.GetCustomAttributes();

        ParameterAttribute? parameterNameAttribute = (ParameterAttribute?)attributes.SingleOrDefault(x => x.GetType() == typeof(ParameterAttribute));

        if (parameterNameAttribute is null)
        {
            builder.WithName(info.Name);
        }
        else
        {
            builder.WithName(parameterNameAttribute.Name);
        }

        // these attributes are order-sensitive and can't be handled in the loop below
        DspDescriptionAttribute? dspDescriptionAttribute = (DspDescriptionAttribute?)attributes.SingleOrDefault(x => x.GetType() == typeof(DspDescriptionAttribute));
        BclDescriptionAttribute? bclDescriptionAttribute = (BclDescriptionAttribute?)attributes.SingleOrDefault(x => x.GetType() == typeof(BclDescriptionAttribute));

        if (dspDescriptionAttribute is not null)
        {
            builder.WithDescription(dspDescriptionAttribute.Description);
        }
        else if (bclDescriptionAttribute is not null)
        {
            builder.WithDescription(bclDescriptionAttribute.Description);
        }

        builder.WithType(info.ParameterType);

        if (info.HasDefaultValue)
        {
            builder.WithDefaultValue(info.DefaultValue);
        }

        builder.AddCheckAttributes(attributes.Where(x => x.GetType().IsAssignableTo(typeof(ParameterCheckAttribute))).Cast<ParameterCheckAttribute>());

        foreach (Attribute attribute in attributes)
        {
            switch (attribute)
            {
                case GreedyAttribute:
                    builder.AsGreedy();
                    break;

                case MinMaxLengthAttribute minMaxLength:
                    builder.WithMinMaxLength(minMaxLength.MinLength, minMaxLength.MaxLength);
                    break;

                case MinMaxValueAttribute minMaxValue:
                    builder.WithMinMaxValue(minMaxValue.MinValue, minMaxValue.MaxValue);
                    break;

                case VariadicParameterAttribute variadic:
                    builder.AsVariadicParameter(variadic.MinimumArgumentCount, variadic.MaximumArgumentCount);
                    break;

                case SlashAutoCompleteProviderAttribute autocomplete:
                    builder.WithAutocompleteProvider(autocomplete.AutoCompleteType);
                    break;

                case SlashChoiceProviderAttribute choice:
                    builder.WithChoiceProvider(choice.ProviderType);
                    break;
            }
        }

        return builder;
    }
}
