using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Invocation;
using DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

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
    private readonly List<Type> allowedHandlers = [];

    private bool allowGuilds = true;
    private bool allowBotDms = false;
    private bool allowUserDms = false;

    /// <summary>
    /// The parameters of this command, excluding the command context.
    /// </summary>
    public IReadOnlyList<ParameterNodeBuilder> Parameters => this.parameters;

    /// <summary>
    /// The context type of this command. This is a valid overloading distinction.
    /// </summary>
    public Type? ContextType { get; private set; }

    /// <summary>
    /// Specifies this overload as the canonical overload. Only one overload is allowed to be designated as such.
    /// </summary>
    public bool IsCanonicalOverload { get; private set; }

    /// <summary>
    /// The executing function for this command.
    /// </summary>
    public Func<CommandContext, object?[], IServiceProvider, ValueTask>? Execute { get; private set; }

    /// <summary>
    /// A list of types of handlers allowed to handle this command. If this list is empty, any handler may decide on its own.
    /// </summary>
    public IReadOnlyList<Type> AllowedHandlers => this.allowedHandlers;

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
    /// Enregisters everything needed to execute this overload.
    /// </summary>
    /// <param name="method">A MethodInfo that will be wrapped in an execution thunk.</param>
    /// <param name="commandName">An identifier for this command to use in debug info.</param>
    [MemberNotNull(nameof(Execute), nameof(name), nameof(ContextType))]
    public CommandOverloadBuilder WithMethod(MethodInfo method, string commandName)
    {
        this.Execute = CommandCanonicalization.GetCommandInvocationFunc(method, commandName);

        StringBuilder mangledNameBuilder = new("Overload(");

        // the first parameter has to be a CommandContext. we encode CommandContext as 'generic', everything else by a ToLower-ed, cut off name
        ParameterInfo[] parameters = method.GetParameters();
        ParameterInfo context = parameters[0];
        Debug.Assert(context.ParameterType.IsAssignableTo(typeof(CommandContext)));

        this.ContextType = context.ParameterType;

        if (context.ParameterType == typeof(CommandContext))
        {
            mangledNameBuilder.Append("generic");
        }
        else
        {
            string contextTypeName = context.ParameterType.Name;
            contextTypeName = contextTypeName[..^nameof(CommandContext).Length];
            mangledNameBuilder.Append(contextTypeName.ToLowerInvariant());
        }

        // now, we build the parameter list. for primitives, we ToLower their framework name (so i.e. ulong -> uint64, int -> int32), for structs
        // we write 'valuetype Namespace.Struct' and for classes we write 'class Namespace.Class'. for example, a command
        // 'WarnAsync(CommandContext context, ulong user, string reason)' would get formatted as 'overload(generic, uint64, string)' and
        // 'MuteAsync(SlashCommandContext context, DiscordMember user, TimeSpan duration, string reason) would get formatted as
        // 'overload(slash, class DSharpPlus.Entities.DiscordMember, valuetype System.TimeSpan, string)'.
        for (int i = 1; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];

            if (parameter.ParameterType.IsValueType)
            {
                if (parameter.ParameterType.IsPrimitive)
                {
                    mangledNameBuilder.Append($", {parameter.ParameterType.Name.ToLowerInvariant()}");
                }
                else
                {
                    mangledNameBuilder.Append($", valuetype {parameter.ParameterType.FullName}");
                }
            }
            else
            {
                if (parameter.ParameterType == typeof(string))
                {
                    mangledNameBuilder.Append(", string");
                }
                else
                {
                    mangledNameBuilder.Append($", class {parameter.ParameterType.FullName}");
                }
            }
        }

        mangledNameBuilder.Append(')');

        this.name = mangledNameBuilder.ToString();
        return this;
    }

    /// <summary>
    /// Sets execution info for this command. This is a manual replacement for <see cref="WithMethod"/>, intended for customized execution thunks or
    /// name mangling patterns. You should not generally use this.
    /// </summary>
    /// <param name="thunk">
    /// The execution thunk for this overload, of the following shape:
    ///     <code> 
    ///     <![CDATA[Func<CommandContext context, object?[] orderedArgs, IServiceProvider services, ValueTask returnValue>]]>
    ///     </code>
    /// </param>
    /// <param name="mangledName">The mangled name of this overload.</param>
    /// <param name="contextType">
    /// The context type, which in and of itself is a valid overloading and filtering target. Since the execution thunk's canonicalized form does not
    /// encode the command context type, this is how the library determines what type can be passed to your command. Misreporting the type will not
    /// throw any apparent errors, but instead cryptically crash at runtime if the real context's vtable mismatches the reported context's vtable.
    /// </param>
    public CommandOverloadBuilder WithExecutionInfo(Func<CommandContext, object?[], IServiceProvider, ValueTask> thunk, string mangledName, Type contextType)
    {
        this.Execute = thunk;
        this.name = mangledName;
        this.ContextType = contextType;
        return this;
    }

    /// <summary>
    /// Sets the current overload to require being executed in a NSFW channel.
    /// </summary>
    public CommandOverloadBuilder RequireNsfwChannel()
    {
        AddNonDuplicateCheckAttribute(new RequireNsfwAttribute());
        AddMetadataItem(new NsfwChannelMetadata());

        return this;
    }

    /// <summary>
    /// Specifies which permissions this overload needs to be allowed to execute.
    /// </summary>
    public CommandOverloadBuilder WithRequiredPermissions(DiscordPermissions userPermissions, DiscordPermissions botPermissions)
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
    public CommandOverloadBuilder SetExecutableInGuilds(bool executableInGuilds = true)
    {
        this.allowGuilds = executableInGuilds;
        return this;
    }

    /// <summary>
    /// Sets whether this overload can be executed in DMs with the bot.
    /// </summary>
    public CommandOverloadBuilder SetExecutableInBotDms(bool executableInBotDms = true)
    {
        this.allowBotDms = executableInBotDms;
        return this;
    }

    /// <summary>
    /// Sets whether this overload can be executed in DMs between users as an user app.
    /// </summary>
    public CommandOverloadBuilder SetExecutableInUserDms(bool executableInUserDms = true)
    {
        this.allowUserDms = executableInUserDms;
        return this;
    }

    /// <summary>
    /// Sets this overload as the overload which takes precedence if an overload could not be resolved or if only one overload is permitted.
    /// </summary>
    public CommandOverloadBuilder SetAsCanonicalOverload()
    {
        this.IsCanonicalOverload = true;
        return this;
    }

    /// <summary>
    /// Adds the check attribute to this parameter.
    /// </summary>
    public CommandOverloadBuilder AddCheckAttribute(ContextCheckAttribute attribute)
    {
        this.checkAttributes.Add(attribute);
        return this;
    }

    /// <summary>
    /// Adds the check attribute to this parameter.
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

    /// <summary>
    /// Adds the specified handler to the allow-list for this command.
    /// </summary>
    public CommandOverloadBuilder AddAllowedHandler(Type handler)
    {
        this.allowedHandlers.Add(handler);
        return this;
    }

    /// <summary>
    /// Adds the specified handlers to the allow-list for this command.
    /// </summary>
    public CommandOverloadBuilder AddAllowedHandlers(params IEnumerable<Type> handlers)
    {
        this.allowedHandlers.AddRange(handlers);
        return this;
    }

    /// <summary>
    /// Builds this overload builder into a complete node.
    /// </summary>
    /// <param name="namingPolicy">The naming policy in use by the extension.</param>
    public CommandOverload Build(IInteractionNamingPolicy namingPolicy)
    {
        ArgumentNullException.ThrowIfNull(this.ContextType);
        ArgumentNullException.ThrowIfNull(this.Execute);
        ArgumentNullException.ThrowIfNull(this.name);

        LocationMetadata location = new()
        {
            IsAllowedInBotDms = this.allowBotDms,
            IsAllowedInOtherDms = this.allowUserDms,
            IsAllowedInGuilds = this.allowGuilds
        };

        AddMetadataItem(location);

        return new CommandOverload
        {
            AllowedHandlers = this.AllowedHandlers,
            CheckAttributes = this.CheckAttributes,
            ContextType = this.ContextType,
            Execute = this.Execute,
            Metadata = this.Metadata,
            Name = this.name,
            Parameters = [..this.Parameters.Select(x => x.Build(namingPolicy))],
            IsCanonicalOverload = this.IsCanonicalOverload
        };
    }

    /// <summary>
    /// Parses an overload builder from the specified method and returns it for further customization.
    /// </summary>
    /// <param name="info">The method to construct this overload from.</param>
    /// <exception cref="ArgumentException">Thrown if the method does not take a CommandContext as first arguments.</exception>
    public static CommandOverloadBuilder FromMethodInfo(MethodInfo info)
    {
        if (info.GetParameters()[0].ParameterType.IsAssignableTo(typeof(CommandContext)))
        {
            throw new ArgumentException($"Failed to register {info.DeclaringType!.FullName}.{info.Name}: a command must take CommandContext or a derived type as first argument");
        }

        CommandOverloadBuilder builder = new();

        builder.WithMethod(info, $"{info.DeclaringType?.FullName ?? "<unknown type>"}#{info.Name}");

        IEnumerable<Attribute> attributes = info.GetCustomAttributes();

        builder.AddCheckAttributes(attributes.Where(x => x.GetType().IsAssignableTo(typeof(ContextCheckAttribute))).Cast<ContextCheckAttribute>());

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

        ParameterInfo[] parameters = info.GetParameters();

        for (int i = 1; i < parameters.Length; i++)
        {
            builder.AddParameter(ParameterNodeBuilder.FromParameterInfo(parameters[i]));
        }

        return builder;
    }
}
