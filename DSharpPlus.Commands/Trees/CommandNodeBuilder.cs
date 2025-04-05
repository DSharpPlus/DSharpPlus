using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// A builder type for creating a named command tree node, both branch and executable nodes.
/// </summary>
public sealed class CommandNodeBuilder
{
    private readonly List<string> aliases;
    private readonly List<CommandNodeBuilder> children;
    private readonly List<CommandOverloadBuilder> overloads;
    private readonly List<INodeMetadataItem> metadataItems = [];
    private readonly List<ContextCheckAttribute> checkAttributes = [];
    private readonly List<Type> allowedHandlers = [];

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
        CommandPermissionsMetadata metadataItem = new()
        {
            BotPermissions = botPermissions,
            UserPermissions = userPermissions
        };

        AddMetadataItem(metadataItem);
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
        }

        foreach (CommandNodeBuilder nodeBuilder in this.Children)
        {
            nodeBuilder.AddCheckAttributes(this.checkAttributes);
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

        foreach (CommandNodeBuilder nodeBuilder in this.Children)
        {
            nodeBuilder.AddCheckAttributes(this.checkAttributes);
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
}
