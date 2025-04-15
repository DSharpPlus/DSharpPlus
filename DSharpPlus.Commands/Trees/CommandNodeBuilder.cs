using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Extensions;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

using DspDescriptionAttribute = DSharpPlus.Commands.DescriptionAttribute;
using BclDescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// A builder type for creating a named command tree node, both branch and executable nodes.
/// </summary>
public sealed class CommandNodeBuilder
{
    private readonly List<string> aliases = [];
    private readonly List<CommandNodeBuilder> children = [];
    private readonly List<CommandOverloadBuilder> overloads = [];
    private readonly List<INodeMetadataItem> metadataItems = [];
    private readonly List<ContextCheckAttribute> checkAttributes = [];
    private readonly List<Type> allowedHandlers = [];

    private DiscordPermissions permissions;
    private DiscordPermissions botPermissions;

    private bool allowGuilds = true;
    private bool allowBotDms = false;
    private bool allowUserDms = false;

    /// <inheritdoc/>
    public string? Name { get; internal set; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Aliases => this.aliases;

    /// <inheritdoc/>
    public string? LowercasedName { get; internal set; }

    /// <inheritdoc/>
    public string? Description { get; internal set; }

    /// <summary>
    /// The children of this command node.
    /// </summary>
    public IReadOnlyList<CommandNodeBuilder> Children => this.children;

    /// <summary>
    /// The overloads of this command node.
    /// </summary>
    public IReadOnlyList<CommandOverloadBuilder> Overloads => this.overloads;

    /// <summary>
    /// The check attributes applicable to this command. This does not necessarily correlate to the list of executed checks,
    /// which may vary based on context and registered check implementations.
    /// </summary>
    // this is not actually respected on command nodes, but we'll collect it here and apply to the actual overload nodes at build time
    public IReadOnlyList<ContextCheckAttribute> CheckAttributes => this.checkAttributes;

    /// <summary>
    /// Additional metadata associated with this command.
    /// </summary>
    public NodeMetadataCollection Metadata => new([.. this.metadataItems.DistinctBy(x => x.GetType())]);

    /// <summary>
    /// Sets the name of this node.
    /// </summary>
    [MemberNotNull(nameof(Name))]
    public CommandNodeBuilder WithName(string name)
    {
        this.Name = name;
        return this;
    }

    /// <summary>
    /// Sets the lowercased name of this node.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the name was, in fact, not all-lowercase.</exception>
    [MemberNotNull(nameof(LowercasedName))]
    public CommandNodeBuilder WithLowercasedName(string lowercased)
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
    /// Adds an alias for this node. Aliases are considered secondarily in command resolution.
    /// </summary>
    public CommandNodeBuilder AddAlias(string alias)
    {
        this.aliases.Add(alias);
        return this;
    }

    /// <summary>
    /// Adds aliases for this node. Aliases are considered secondarily in command resolution.
    /// </summary>
    public CommandNodeBuilder AddAliases(params IEnumerable<string> aliases)
    {
        this.aliases.AddRange(aliases);
        return this;
    }

    /// <summary>
    /// Sets the description of this node.
    /// </summary>
    [MemberNotNull(nameof(Description))]
    public CommandNodeBuilder WithDescription(string description)
    {
        this.Description = description;
        return this;
    }

    /// <summary>
    /// Sets the current overload to require being executed in a NSFW channel.
    /// </summary>
    public CommandNodeBuilder RequireNsfwChannel()
    {
        AddNonDuplicateCheckAttribute(new RequireNsfwAttribute());
        AddMetadataItem(new NsfwChannelMetadata());

        return this;
    }

    /// <summary>
    /// Specifies which permissions this overload needs to be allowed to execute.
    /// </summary>
    public CommandNodeBuilder WithRequiredPermissions(DiscordPermissions userPermissions, DiscordPermissions botPermissions)
    {
        this.permissions = userPermissions;
        this.botPermissions = botPermissions;

        return this;
    }

    /// <summary>
    /// Adds permissions to the set of requirements for this node.
    /// </summary>
    public CommandNodeBuilder AddPermissions(DiscordPermissions userPermissions, DiscordPermissions botPermissions)
    {
        this.permissions |= userPermissions;
        this.botPermissions |= botPermissions;

        return this;
    }

    /// <summary>
    /// Sets whether this overload can be executed in guilds.
    /// </summary>
    public CommandNodeBuilder SetExecutableInGuilds(bool executableInGuilds = true)
    {
        this.allowGuilds = executableInGuilds;
        return this;
    }

    /// <summary>
    /// Sets whether this overload can be executed in DMs with the bot.
    /// </summary>
    public CommandNodeBuilder SetExecutableInBotDms(bool executableInBotDms = true)
    {
        this.allowBotDms = executableInBotDms;
        return this;
    }

    /// <summary>
    /// Sets whether this overload can be executed in DMs between users as an user app.
    /// </summary>
    public CommandNodeBuilder SetExecutableInUserDms(bool executableInUserDms = true)
    {
        this.allowUserDms = executableInUserDms;
        return this;
    }

    /// <summary>
    /// Adds the check attribute to this parameter.
    /// </summary>
    public CommandNodeBuilder AddCheckAttribute(ContextCheckAttribute attribute)
    {
        this.checkAttributes.Add(attribute);
        return this;
    }

    /// <summary>
    /// Adds the check attribute to this parameter.
    /// </summary>
    public CommandNodeBuilder AddCheckAttributes(params IEnumerable<ContextCheckAttribute> attributes)
    {
        this.checkAttributes.AddRange(attributes);
        return this;
    }

    /// <summary>
    /// Adds a new check attribute only if there is no preexisting check attribute of the same type present.
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public CommandNodeBuilder AddNonDuplicateCheckAttribute(ContextCheckAttribute attribute)
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
    public CommandNodeBuilder AddMetadataItem(INodeMetadataItem item)
    {
        this.metadataItems.Add(item);
        return this;
    }

    /// <summary>
    /// Adds the specified metadata item to this parameter. Note that node metadata is deduplicated by type.
    /// </summary>
    public CommandNodeBuilder AddMetadataItems(params IEnumerable<INodeMetadataItem> items)
    {
        this.metadataItems.AddRange(items);
        return this;
    }

    /// <summary>
    /// Adds the specified handler to the allow-list for this command.
    /// </summary>
    public CommandNodeBuilder AddAllowedHandler(Type handler)
    {
        this.allowedHandlers.Add(handler);
        return this;
    }

    /// <summary>
    /// Adds the specified handlers to the allow-list for this command.
    /// </summary>
    public CommandNodeBuilder AddAllowedHandlers(params IEnumerable<Type> handlers)
    {
        this.allowedHandlers.AddRange(handlers);
        return this;
    }

    /// <summary>
    /// Adds a child node to this node. This has no bearing on the type of this node.
    /// </summary>
    public CommandNodeBuilder AddChild(CommandNodeBuilder child)
    {
        this.children.Add(child);
        return this;
    }

    /// <summary>
    /// Adds child nodes to this node. This has no bearing on the type of this node.
    /// </summary>
    public CommandNodeBuilder AddChildren(params IEnumerable<CommandNodeBuilder> children)
    {
        this.children.AddRange(children);
        return this;
    }

    /// <summary>
    /// Adds an overload to this node.
    /// </summary>
    public CommandNodeBuilder AddOverload(CommandOverloadBuilder overload)
    {
        this.overloads.Add(overload);
        return this;
    }

    /// <summary>
    /// Adds overloads to this node. 
    /// </summary>
    public CommandNodeBuilder AddOverloads(params IEnumerable<CommandOverloadBuilder> overloads)
    {
        this.overloads.AddRange(overloads);
        return this;
    }

    /// <summary>
    /// Builds a command node from this builder capable of representing the 
    /// </summary>
    /// <param name="namingPolicy"></param>
    /// <returns></returns>
    public ICommandNode Build(IInteractionNamingPolicy namingPolicy)
        => this.overloads is [] ? BuildBranchNode(namingPolicy) : BuildExecutableNode(namingPolicy);

    // this one is kind of difficult, since this is where we decide which overload is to reign supreme
    private ExecutableCommandNode BuildExecutableNode(IInteractionNamingPolicy namingPolicy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(this.Name);

        if (this.LowercasedName is null)
        {
            WithLowercasedName(namingPolicy.TransformText(this.Name, CultureInfo.InvariantCulture));
        }

        if (this.Description is null)
        {
            WithDescription("No description provided.");
        }

        foreach (CommandOverloadBuilder overloadBuilder in this.Overloads)
        {
            overloadBuilder.AddCheckAttributes(this.checkAttributes);
            overloadBuilder.AddPermissions(this.permissions, this.botPermissions);
        }

        foreach (CommandNodeBuilder nodeBuilder in this.Children)
        {
            nodeBuilder.AddCheckAttributes(this.checkAttributes);
            nodeBuilder.AddPermissions(this.permissions, this.botPermissions);
        }

        int canonicalOverloads = this.overloads.Count(x => x.IsCanonicalOverload);

        // TODO: once we add overload resolution support, update this to choose the most applicable overload and/or by attribute
        if (canonicalOverloads == 0)
        {
            CommandOverloadBuilder canonical = this.overloads.OrderByDescending(x => x.Parameters.Count).First();
            canonical.SetAsCanonicalOverload();
        }
        else if (canonicalOverloads > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(this.overloads), $"Building command {this.Name}: encountered two marked canonical overloads, only one is permitted.");
        }

        LocationMetadata location = new()
        {
            IsAllowedInBotDms = this.allowBotDms,
            IsAllowedInOtherDms = this.allowUserDms,
            IsAllowedInGuilds = this.allowGuilds
        };

        AddMetadataItem(location);

        CommandOverload[] builtOverloads = [.. this.Overloads.Select(x => x.Build(namingPolicy))];

        return new ExecutableCommandNode
        {
            Aliases = this.Aliases,
            Description = this.Description,
            Children = [..this.Children.Select(x => x.Build(namingPolicy))],
            LowercasedName = this.LowercasedName,
            Metadata = this.Metadata,
            Name = this.Name,
            Overloads = builtOverloads,
            CanonicalApplicationCommandOverload = builtOverloads.First(x => x.IsCanonicalOverload)
        };
    }

    // this one, on the other hand, is fairly easy - we just assemble a bit of structural information
    private CommandBranchNode BuildBranchNode(IInteractionNamingPolicy namingPolicy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(this.Name);
        ArgumentOutOfRangeException.ThrowIfEqual(this.Children.Count, 0);

        if (this.LowercasedName is null)
        {
            WithLowercasedName(namingPolicy.TransformText(this.Name, CultureInfo.InvariantCulture));
        }

        if (this.Description is null)
        {
            WithDescription("No description provided.");
        }

        LocationMetadata location = new()
        {
            IsAllowedInBotDms = this.allowBotDms,
            IsAllowedInOtherDms = this.allowUserDms,
            IsAllowedInGuilds = this.allowGuilds
        };

        CommandPermissionsMetadata permissions = new()
        {
            UserPermissions = this.permissions,
            BotPermissions = this.botPermissions
        };

        AddMetadataItem(location);
        AddMetadataItem(permissions);

        foreach (CommandNodeBuilder nodeBuilder in this.Children)
        {
            nodeBuilder.AddCheckAttributes(this.checkAttributes);
            nodeBuilder.AddPermissions(this.permissions, this.botPermissions);
        }

        return new CommandBranchNode
        {
            Aliases = this.Aliases,
            Description = this.Description,
            Children = [.. this.Children.Select(x => x.Build(namingPolicy))],
            LowercasedName = this.LowercasedName,
            Metadata = this.Metadata,
            Name = this.Name
        };
    }

    /// <summary>
    /// Creates a new command node builder from a Type.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the type was not a valid command and/or did not contain any commands.</exception>
    public static CommandNodeBuilder FromType(Type type)
    {
        CommandNodeBuilder root = new();
        CommandNodeBuilder builder = root;
        IEnumerable<Attribute> attributes = type.GetCustomAttributes();

        CommandAttribute commandAttribute = attributes.SingleOrDefaultOfType<CommandAttribute, Attribute>()
            ?? throw new ArgumentException($"The type {type} does not have a CommandAttribute applied to it.", nameof(type));

        NestCommandGroups(ref builder, commandAttribute.Names);
        ApplyAttributesToNode(builder, attributes);

        foreach (Type nested in type.GetNestedTypes())
        {
            if (nested.GetCustomAttribute<CommandAttribute>() is not null)
            {
                builder.AddChild(FromType(nested));
            }
        }

        foreach (MethodInfo method in type.GetMethods())
        {
            if (method.GetCustomAttribute<CommandAttribute>() is not null)
            {
                builder.AddChild(FromMethodInfo(method));
            }

            if (method.GetCustomAttribute<DefaultGroupCommandAttribute>() is not null)
            {
                builder.AddOverload(CommandOverloadBuilder.FromMethodInfo(method));
            }
        }

        return root;
    }

    /// <summary>
    /// Creates a new command node builder from a MethodInfo.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the method was not a valid command.</exception>
    public static CommandNodeBuilder FromMethodInfo(MethodInfo method)
    {
        CommandNodeBuilder builder = new();
        IEnumerable<Attribute> attributes = method.GetCustomAttributes();

        CommandAttribute commandAttribute = attributes.SingleOrDefaultOfType<CommandAttribute, Attribute>()
            ?? throw new ArgumentException($"The method {method} does not have a CommandAttribute applied to it.", nameof(method));

        NestCommandGroups(ref builder, commandAttribute.Names);
        ApplyAttributesToNode(builder, attributes);
        builder.AddOverload(CommandOverloadBuilder.FromMethodInfo(method));        

        return builder;
    }

    private static void ApplyAttributesToNode(CommandNodeBuilder builder, IEnumerable<Attribute> attributes)
    {
        builder.AddCheckAttributes(attributes.Where(x => x.GetType().IsAssignableTo(typeof(ContextCheckAttribute))).Cast<ContextCheckAttribute>());

        DspDescriptionAttribute? dspDescriptionAttribute = attributes.SingleOrDefaultOfType<DspDescriptionAttribute, Attribute>();
        BclDescriptionAttribute? bclDescriptionAttribute = attributes.SingleOrDefaultOfType<BclDescriptionAttribute, Attribute>();

        if (dspDescriptionAttribute is not null)
        {
            builder.WithDescription(dspDescriptionAttribute.Description);
        }
        else if (bclDescriptionAttribute is not null)
        {
            builder.WithDescription(bclDescriptionAttribute.Description);
        }

        foreach (Attribute attribute in attributes)
        {
            switch (attribute)
            {
                case AllowedCommandHandlersAttribute allowedHandlers:
                    builder.AddAllowedHandlers(allowedHandlers.Handlers);
                    break;

                case RequireNsfwAttribute:
                    builder.RequireNsfwChannel();
                    break;

                case RequirePermissionsAttribute requiredPermissions:
                    builder.WithRequiredPermissions(requiredPermissions.UserPermissions, requiredPermissions.BotPermissions);
                    break;

                case AllowDmsAttribute allowDms:
                    builder.allowBotDms = allowDms.Usage.HasFlag(DmUsageRule.AllowBotDms);
                    builder.allowUserDms = allowDms.Usage.HasFlag(DmUsageRule.AllowUserDms);
                    break;

                case DenyGuildsAttribute denyGuilds:
                    builder.allowGuilds = false;
                    break;
            }
        }
    }

    private static void NestCommandGroups(ref CommandNodeBuilder builder, string[] attributeData)
    {
        if (attributeData is [string soleName])
        {
            builder.WithName(soleName);
        }
        else
        {
            builder.WithName(attributeData[0]);

            for (int i = 1; i < attributeData.Length; i++)
            {
                CommandNodeBuilder tempBuilder = new();

                tempBuilder.WithName(attributeData[i]);

                builder.AddChild(tempBuilder);
                builder = tempBuilder;
            }
        }
    }
}
