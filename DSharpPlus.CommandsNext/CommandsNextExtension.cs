using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Builders;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Executors;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.CommandsNext;

/// <summary>
/// This is the class which handles command registration, management, and execution.
/// </summary>
public class CommandsNextExtension
{
    private CommandsNextConfiguration Config { get; }
    private HelpFormatterFactory HelpFormatter { get; }

    private MethodInfo ConvertGeneric { get; }
    private Dictionary<Type, string> UserFriendlyTypeNames { get; }
    internal Dictionary<Type, IArgumentConverter> ArgumentConverters { get; }
    internal CultureInfo DefaultParserCulture
        => this.Config.DefaultParserCulture;

    /// <summary>
    /// Gets the service provider this CommandsNext module was configured with.
    /// </summary>
    public IServiceProvider Services
        => this.Client.ServiceProvider;

    internal CommandsNextExtension(CommandsNextConfiguration cfg)
    {
        this.Config = new CommandsNextConfiguration(cfg);
        this.TopLevelCommands = [];
        this.HelpFormatter = new HelpFormatterFactory();
        this.HelpFormatter.SetFormatterType<DefaultHelpFormatter>();

        this.ArgumentConverters = new Dictionary<Type, IArgumentConverter>
        {
            [typeof(string)] = new StringConverter(),
            [typeof(bool)] = new BoolConverter(),
            [typeof(sbyte)] = new Int8Converter(),
            [typeof(byte)] = new Uint8Converter(),
            [typeof(short)] = new Int16Converter(),
            [typeof(ushort)] = new Uint16Converter(),
            [typeof(int)] = new Int32Converter(),
            [typeof(uint)] = new Uint32Converter(),
            [typeof(long)] = new Int64Converter(),
            [typeof(ulong)] = new Uint64Converter(),
            [typeof(float)] = new Float32Converter(),
            [typeof(double)] = new Float64Converter(),
            [typeof(decimal)] = new Float128Converter(),
            [typeof(DateTime)] = new DateTimeConverter(),
            [typeof(DateTimeOffset)] = new DateTimeOffsetConverter(),
            [typeof(TimeSpan)] = new TimeSpanConverter(),
            [typeof(Uri)] = new UriConverter(),
            [typeof(DiscordUser)] = new DiscordUserConverter(),
            [typeof(DiscordMember)] = new DiscordMemberConverter(),
            [typeof(DiscordRole)] = new DiscordRoleConverter(),
            [typeof(DiscordChannel)] = new DiscordChannelConverter(),
            [typeof(DiscordThreadChannel)] = new DiscordThreadChannelConverter(),
            [typeof(DiscordGuild)] = new DiscordGuildConverter(),
            [typeof(DiscordMessage)] = new DiscordMessageConverter(),
            [typeof(DiscordEmoji)] = new DiscordEmojiConverter(),
            [typeof(DiscordColor)] = new DiscordColorConverter()
        };

        this.UserFriendlyTypeNames = new Dictionary<Type, string>()
        {
            [typeof(string)] = "string",
            [typeof(bool)] = "boolean",
            [typeof(sbyte)] = "signed byte",
            [typeof(byte)] = "byte",
            [typeof(short)] = "short",
            [typeof(ushort)] = "unsigned short",
            [typeof(int)] = "int",
            [typeof(uint)] = "unsigned int",
            [typeof(long)] = "long",
            [typeof(ulong)] = "unsigned long",
            [typeof(float)] = "float",
            [typeof(double)] = "double",
            [typeof(decimal)] = "decimal",
            [typeof(DateTime)] = "date and time",
            [typeof(DateTimeOffset)] = "date and time",
            [typeof(TimeSpan)] = "time span",
            [typeof(Uri)] = "URL",
            [typeof(DiscordUser)] = "user",
            [typeof(DiscordMember)] = "member",
            [typeof(DiscordRole)] = "role",
            [typeof(DiscordChannel)] = "channel",
            [typeof(DiscordGuild)] = "guild",
            [typeof(DiscordMessage)] = "message",
            [typeof(DiscordEmoji)] = "emoji",
            [typeof(DiscordColor)] = "color"
        };

        Type ncvt = typeof(NullableConverter<>);
        Type nt = typeof(Nullable<>);
        Type[] cvts = [.. this.ArgumentConverters.Keys];
        foreach (Type? xt in cvts)
        {
            TypeInfo xti = xt.GetTypeInfo();
            if (!xti.IsValueType)
            {
                continue;
            }

            Type xcvt = ncvt.MakeGenericType(xt);
            Type xnt = nt.MakeGenericType(xt);

            if (this.ArgumentConverters.ContainsKey(xcvt) || Activator.CreateInstance(xcvt) is not IArgumentConverter xcv)
            {
                continue;
            }

            this.ArgumentConverters[xnt] = xcv;
            this.UserFriendlyTypeNames[xnt] = this.UserFriendlyTypeNames[xt];
        }

        Type t = typeof(CommandsNextExtension);
        IEnumerable<MethodInfo> ms = t.GetTypeInfo().DeclaredMethods;
        MethodInfo? m = ms.FirstOrDefault(xm => xm.Name == nameof(ConvertArgumentAsync) && xm.ContainsGenericParameters && !xm.IsStatic && xm.IsPublic);
        this.ConvertGeneric = m;
    }

    /// <summary>
    /// Sets the help formatter to use with the default help command.
    /// </summary>
    /// <typeparam name="T">Type of the formatter to use.</typeparam>
    public void SetHelpFormatter<T>() where T : BaseHelpFormatter => this.HelpFormatter.SetFormatterType<T>();

    /// <summary>
    /// Disposes of this the resources used by CNext.
    /// </summary>
    public void Dispose()
    {
        this.Config.CommandExecutor.Dispose();

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }

    #region DiscordClient Registration
    /// <summary>
    /// DO NOT USE THIS MANUALLY.
    /// </summary>
    /// <param name="client">DO NOT USE THIS MANUALLY.</param>
    /// <exception cref="InvalidOperationException"/>
    public void Setup(DiscordClient client)
    {
        this.Client = client;

        if (!Utilities.HasMessageIntents(client.Intents))
        {
            client.Logger.LogCritical(CommandsNextEvents.Intents, "The CommandsNext extension is registered but there are no message intents enabled. It is highly recommended to enable them.");
        }

        if (!client.Intents.HasIntent(DiscordIntents.Guilds))
        {
            client.Logger.LogCritical(CommandsNextEvents.Intents, "The CommandsNext extension is registered but the guilds intent is not enabled. It is highly recommended to enable it.");
        }

        DefaultClientErrorHandler errorHandler = new(client.Logger);

        this.executed = new AsyncEvent<CommandsNextExtension, CommandExecutionEventArgs>(errorHandler);
        this.error = new AsyncEvent<CommandsNextExtension, CommandErrorEventArgs>(errorHandler);

        if (this.Config.EnableDefaultHelp)
        {
            RegisterCommands(typeof(DefaultHelpModule), null, [], out List<CommandBuilder>? tcmds);

            if (this.Config.DefaultHelpChecks.Any())
            {
                CheckBaseAttribute[] checks = this.Config.DefaultHelpChecks.ToArray();

                for (int i = 0; i < tcmds.Count; i++)
                {
                    tcmds[i].WithExecutionChecks(checks);
                }
            }

            if (tcmds != null)
            {
                foreach (CommandBuilder xc in tcmds)
                {
                    AddToCommandDictionary(xc.Build(null));
                }
            }
        }

        if (this.Config.CommandExecutor is ParallelQueuedCommandExecutor pqce)
        {
            this.Client.Logger.LogDebug(CommandsNextEvents.Misc, "Using parallel executor with degree {Parallelism}", pqce.Parallelism);
        }
    }
    #endregion

    #region Command Handling
    internal async Task HandleCommandsAsync(DiscordClient sender, MessageCreatedEventArgs e)
    {
        if (e.Author.IsBot) // bad bot
        {
            return;
        }

        if (!this.Config.EnableDms && e.Channel.IsPrivate)
        {
            return;
        }

        int mpos = -1;
        if (this.Config.EnableMentionPrefix)
        {
            mpos = e.Message.GetMentionPrefixLength(this.Client.CurrentUser);
        }

        if (this.Config.StringPrefixes.Any())
        {
            foreach (string pfix in this.Config.StringPrefixes)
            {
                if (mpos == -1 && !string.IsNullOrWhiteSpace(pfix))
                {
                    mpos = e.Message.GetStringPrefixLength(pfix, this.Config.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        if (mpos == -1 && this.Config.PrefixResolver != null)
        {
            mpos = await this.Config.PrefixResolver(e.Message);
        }

        if (mpos == -1)
        {
            return;
        }

        string pfx = e.Message.Content[..mpos];
        string cnt = e.Message.Content[mpos..];

        int _ = 0;
        string? fname = cnt.ExtractNextArgument(ref _, this.Config.QuotationMarks);

        Command? cmd = FindCommand(cnt, out string? args);
        CommandContext ctx = CreateContext(e.Message, pfx, cmd, args);

        if (cmd is null)
        {
            await this.error.InvokeAsync(this, new CommandErrorEventArgs { Context = ctx, Exception = new CommandNotFoundException(fname ?? "UnknownCmd") });
            return;
        }

        await this.Config.CommandExecutor.ExecuteAsync(ctx);
    }

    /// <summary>
    /// Finds a specified command by its qualified name, then separates arguments.
    /// </summary>
    /// <param name="commandString">Qualified name of the command, optionally with arguments.</param>
    /// <param name="rawArguments">Separated arguments.</param>
    /// <returns>Found command or null if none was found.</returns>
    public Command? FindCommand(string commandString, out string? rawArguments)
    {
        rawArguments = null;

        bool ignoreCase = !this.Config.CaseSensitive;
        int pos = 0;
        string? next = commandString.ExtractNextArgument(ref pos, this.Config.QuotationMarks);
        if (next is null)
        {
            return null;
        }

        if (!this.RegisteredCommands.TryGetValue(next, out Command? cmd))
        {
            if (!ignoreCase)
            {
                return null;
            }

            KeyValuePair<string, Command> cmdKvp = this.RegisteredCommands.FirstOrDefault(x => x.Key.Equals(next, StringComparison.InvariantCultureIgnoreCase));
            if (cmdKvp.Value is null)
            {
                return null;
            }

            cmd = cmdKvp.Value;
        }

        if (cmd is not CommandGroup)
        {
            rawArguments = commandString[pos..].Trim();
            return cmd;
        }

        while (cmd is CommandGroup)
        {
            CommandGroup? cm2 = cmd as CommandGroup;
            int oldPos = pos;
            next = commandString.ExtractNextArgument(ref pos, this.Config.QuotationMarks);
            if (next is null)
            {
                break;
            }

            StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
            cmd = cm2?.Children.FirstOrDefault(x => x.Name.Equals(next, comparison) || x.Aliases.Any(xx => xx.Equals(next, comparison)));

            if (cmd is null)
            {
                cmd = cm2;
                pos = oldPos;
                break;
            }
        }

        rawArguments = commandString[pos..].Trim();
        return cmd;
    }

    /// <summary>
    /// Creates a command execution context from specified arguments.
    /// </summary>
    /// <param name="msg">Message to use for context.</param>
    /// <param name="prefix">Command prefix, used to execute commands.</param>
    /// <param name="cmd">Command to execute.</param>
    /// <param name="rawArguments">Raw arguments to pass to command.</param>
    /// <returns>Created command execution context.</returns>
    public CommandContext CreateContext(DiscordMessage msg, string prefix, Command? cmd, string? rawArguments = null)
    {
        CommandContext ctx = new()
        {
            Client = this.Client,
            Command = cmd,
            Message = msg,
            Config = this.Config,
            RawArgumentString = rawArguments ?? "",
            Prefix = prefix,
            CommandsNext = this,
            Services = this.Services
        };

        if (cmd is not null && (cmd.Module is TransientCommandModule || cmd.Module == null))
        {
            IServiceScope scope = ctx.Services.CreateScope();
            ctx.ServiceScopeContext = new CommandContext.ServiceContext(ctx.Services, scope);
            ctx.Services = scope.ServiceProvider;
        }

        return ctx;
    }

    /// <summary>
    /// Executes specified command from given context.
    /// </summary>
    /// <param name="ctx">Context to execute command from.</param>
    /// <returns></returns>
    public async Task ExecuteCommandAsync(CommandContext ctx)
    {
        try
        {
            Command? cmd = ctx.Command;

            if (cmd is null)
            {
                return;
            }

            await RunAllChecksAsync(cmd, ctx);

            CommandResult res = await cmd.ExecuteAsync(ctx);

            if (res.IsSuccessful)
            {
                await this.executed.InvokeAsync(this, new CommandExecutionEventArgs { Context = res.Context });
            }
            else
            {
                await this.error.InvokeAsync(this, new CommandErrorEventArgs { Context = res.Context, Exception = res.Exception });
            }
        }
        catch (Exception ex)
        {
            await this.error.InvokeAsync(this, new CommandErrorEventArgs { Context = ctx, Exception = ex });
        }
        finally
        {
            if (ctx.ServiceScopeContext.IsInitialized)
            {
                ctx.ServiceScopeContext.Dispose();
            }
        }
    }

    private static async Task RunAllChecksAsync(Command cmd, CommandContext ctx)
    {
        if (cmd.Parent is not null)
        {
            await RunAllChecksAsync(cmd.Parent, ctx);
        }

        IEnumerable<CheckBaseAttribute> fchecks = await cmd.RunChecksAsync(ctx, false);
        if (fchecks.Any())
        {
            throw new ChecksFailedException(cmd, ctx, fchecks);
        }
    }
    #endregion

    #region Command Registration
    /// <summary>
    /// Gets a dictionary of registered top-level commands.
    /// </summary>
    public IReadOnlyDictionary<string, Command> RegisteredCommands
        => this.TopLevelCommands;

    private Dictionary<string, Command> TopLevelCommands { get; set; }
    public DiscordClient Client { get; private set; }

    /// <summary>
    /// Registers all commands from a given assembly. The command classes need to be public to be considered for registration.
    /// </summary>
    /// <param name="assembly">Assembly to register commands from.</param>
    public void RegisterCommands(Assembly assembly)
    {
        IEnumerable<Type> types = assembly.ExportedTypes.Where(xt =>
        {
            TypeInfo xti = xt.GetTypeInfo();
            return xti.IsModuleCandidateType() && !xti.IsNested;
        });
        foreach (Type? xt in types)
        {
            RegisterCommands(xt);
        }
    }

    /// <summary>
    /// Registers all commands from a given command class.
    /// </summary>
    /// <typeparam name="T">Class which holds commands to register.</typeparam>
    public void RegisterCommands<T>() where T : BaseCommandModule
    {
        Type t = typeof(T);
        RegisterCommands(t);
    }

    /// <summary>
    /// Registers all commands from a given command class.
    /// </summary>
    /// <param name="t">Type of the class which holds commands to register.</param>
    public void RegisterCommands(Type t)
    {
        if (t is null)
        {
            throw new ArgumentNullException(nameof(t), "Type cannot be null.");
        }

        if (!t.IsModuleCandidateType())
        {
            throw new ArgumentNullException(nameof(t), "Type must be a class, which cannot be abstract or static.");
        }

        RegisterCommands(t, null, [], out List<CommandBuilder>? tempCommands);

        if (tempCommands != null)
        {
            foreach (CommandBuilder command in tempCommands)
            {
                AddToCommandDictionary(command.Build(null));
            }
        }
    }

    private void RegisterCommands(Type t, CommandGroupBuilder? currentParent, IEnumerable<CheckBaseAttribute> inheritedChecks, out List<CommandBuilder> foundCommands)
    {
        TypeInfo ti = t.GetTypeInfo();

        ModuleLifespanAttribute? lifespan = ti.GetCustomAttribute<ModuleLifespanAttribute>();
        ModuleLifespan moduleLifespan = lifespan != null ? lifespan.Lifespan : ModuleLifespan.Singleton;

        ICommandModule module = new CommandModuleBuilder()
            .WithType(t)
            .WithLifespan(moduleLifespan)
            .Build(this.Services);

        // restrict parent lifespan to more or equally restrictive
        if (currentParent?.Module is TransientCommandModule && moduleLifespan != ModuleLifespan.Transient)
        {
            throw new InvalidOperationException("In a transient module, child modules can only be transient.");
        }

        // check if we are anything
        CommandGroupBuilder? groupBuilder = new(module);
        bool isModule = false;
        IEnumerable<Attribute> moduleAttributes = ti.GetCustomAttributes();
        bool moduleHidden = false;
        List<CheckBaseAttribute> moduleChecks = [];

        groupBuilder.WithCategory(ExtractCategoryAttribute(t));

        foreach (Attribute xa in moduleAttributes)
        {
            switch (xa)
            {
                case GroupAttribute g:
                    isModule = true;
                    string? moduleName = g.Name;
                    if (moduleName is null)
                    {
                        moduleName = ti.Name;

                        if (moduleName.EndsWith("Group") && moduleName != "Group")
                        {
                            moduleName = moduleName[..^5];
                        }
                        else if (moduleName.EndsWith("Module") && moduleName != "Module")
                        {
                            moduleName = moduleName[..^6];
                        }
                        else if (moduleName.EndsWith("Commands") && moduleName != "Commands")
                        {
                            moduleName = moduleName[..^8];
                        }
                    }

                    if (!this.Config.CaseSensitive)
                    {
                        moduleName = moduleName.ToLowerInvariant();
                    }

                    groupBuilder.WithName(moduleName);

                    foreach (CheckBaseAttribute chk in inheritedChecks)
                    {
                        groupBuilder.WithExecutionCheck(chk);
                    }

                    foreach (MethodInfo? mi in ti.DeclaredMethods.Where(x => x.IsCommandCandidate(out _) && x.GetCustomAttribute<GroupCommandAttribute>() != null))
                    {
                        groupBuilder.WithOverload(new CommandOverloadBuilder(mi));
                    }

                    break;

                case AliasesAttribute a:
                    foreach (string xalias in a.Aliases)
                    {
                        groupBuilder.WithAlias(this.Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
                    }

                    break;

                case HiddenAttribute h:
                    groupBuilder.WithHiddenStatus(true);
                    moduleHidden = true;
                    break;

                case DescriptionAttribute d:
                    groupBuilder.WithDescription(d.Description);
                    break;

                case CheckBaseAttribute c:
                    moduleChecks.Add(c);
                    groupBuilder.WithExecutionCheck(c);
                    break;

                default:
                    groupBuilder.WithCustomAttribute(xa);
                    break;
            }
        }

        if (!isModule)
        {
            groupBuilder = null;
            if (!inheritedChecks.Any())
            {
                moduleChecks.AddRange(inheritedChecks);
            }
        }

        // candidate methods
        IEnumerable<MethodInfo> methods = ti.DeclaredMethods;
        List<CommandBuilder> commands = [];
        Dictionary<string, CommandBuilder> commandBuilders = [];
        foreach (MethodInfo m in methods)
        {
            if (!m.IsCommandCandidate(out _))
            {
                continue;
            }

            IEnumerable<Attribute> attrs = m.GetCustomAttributes();
            if (attrs.FirstOrDefault(xa => xa is CommandAttribute) is not CommandAttribute cattr)
            {
                continue;
            }

            string? commandName = cattr.Name;
            if (commandName is null)
            {
                commandName = m.Name;
                if (commandName.EndsWith("Async") && commandName != "Async")
                {
                    commandName = commandName[..^5];
                }
            }

            if (!this.Config.CaseSensitive)
            {
                commandName = commandName.ToLowerInvariant();
            }

            if (!commandBuilders.TryGetValue(commandName, out CommandBuilder? commandBuilder))
            {
                commandBuilders.Add(commandName, commandBuilder = new CommandBuilder(module).WithName(commandName));

                if (!isModule)
                {
                    if (currentParent != null)
                    {
                        currentParent.WithChild(commandBuilder);
                    }
                    else
                    {
                        commands.Add(commandBuilder);
                    }
                }
                else
                {
                    groupBuilder?.WithChild(commandBuilder);
                }
            }

            commandBuilder.WithOverload(new CommandOverloadBuilder(m));

            if (!isModule && moduleChecks.Count != 0)
            {
                foreach (CheckBaseAttribute chk in moduleChecks)
                {
                    commandBuilder.WithExecutionCheck(chk);
                }
            }

            commandBuilder.WithCategory(ExtractCategoryAttribute(m));

            foreach (Attribute xa in attrs)
            {
                switch (xa)
                {
                    case AliasesAttribute a:
                        foreach (string xalias in a.Aliases)
                        {
                            commandBuilder.WithAlias(this.Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
                        }

                        break;

                    case CheckBaseAttribute p:
                        commandBuilder.WithExecutionCheck(p);
                        break;

                    case DescriptionAttribute d:
                        commandBuilder.WithDescription(d.Description);
                        break;

                    case HiddenAttribute h:
                        commandBuilder.WithHiddenStatus(true);
                        break;

                    default:
                        commandBuilder.WithCustomAttribute(xa);
                        break;
                }
            }

            if (!isModule && moduleHidden)
            {
                commandBuilder.WithHiddenStatus(true);
            }
        }

        // candidate types
        IEnumerable<TypeInfo> types = ti.DeclaredNestedTypes
            .Where(xt => xt.IsModuleCandidateType() && xt.DeclaredConstructors.Any(xc => xc.IsPublic));
        foreach (TypeInfo? type in types)
        {
            RegisterCommands(type.AsType(),
                groupBuilder,
                !isModule ? moduleChecks : Enumerable.Empty<CheckBaseAttribute>(),
                out List<CommandBuilder>? tempCommands);

            if (isModule && groupBuilder is not null)
            {
                foreach (CheckBaseAttribute chk in moduleChecks)
                {
                    groupBuilder.WithExecutionCheck(chk);
                }
            }

            if (isModule && tempCommands is not null && groupBuilder is not null)
            {
                foreach (CommandBuilder xtcmd in tempCommands)
                {
                    groupBuilder.WithChild(xtcmd);
                }
            }
            else if (tempCommands != null)
            {
                commands.AddRange(tempCommands);
            }
        }

        if (isModule && currentParent is null && groupBuilder is not null)
        {
            commands.Add(groupBuilder);
        }
        else if (isModule && currentParent is not null && groupBuilder is not null)
        {
            currentParent.WithChild(groupBuilder);
        }

        foundCommands = commands;
    }

    /// <summary>
    /// Builds and registers all supplied commands.
    /// </summary>
    /// <param name="cmds">Commands to build and register.</param>
    public void RegisterCommands(params CommandBuilder[] cmds)
    {
        foreach (CommandBuilder cmd in cmds)
        {
            AddToCommandDictionary(cmd.Build(null));
        }
    }

    /// <summary>
    /// Unregisters specified commands from CommandsNext.
    /// </summary>
    /// <param name="cmds">Commands to unregister.</param>
    public void UnregisterCommands(params Command[] cmds)
    {
        if (cmds.Any(x => x.Parent is not null))
        {
            throw new InvalidOperationException("Cannot unregister nested commands.");
        }

        List<string> keys = this.RegisteredCommands.Where(x => cmds.Contains(x.Value)).Select(x => x.Key).ToList();
        foreach (string? key in keys)
        {
            this.TopLevelCommands.Remove(key);
        }
    }

    private static string? ExtractCategoryAttribute(MethodInfo method)
    {
        CategoryAttribute attribute = method.GetCustomAttribute<CategoryAttribute>();

        if (attribute is not null)
        {
            return attribute.Name;
        }

        // extract from types

        return ExtractCategoryAttribute(method.DeclaringType);
    }

    private static string? ExtractCategoryAttribute(Type type)
    {
        CategoryAttribute attribute;

        do
        {
            attribute = type.GetCustomAttribute<CategoryAttribute>();

            if (attribute is not null)
            {
                return attribute.Name;
            }

            type = type.DeclaringType;

        } while (type is not null);

        return null;
    }

    private void AddToCommandDictionary(Command cmd)
    {
        if (cmd.Parent is not null)
        {
            return;
        }

        if (this.TopLevelCommands.ContainsKey(cmd.Name) || cmd.Aliases.Any(xs => this.TopLevelCommands.ContainsKey(xs)))
        {
            throw new DuplicateCommandException(cmd.QualifiedName);
        }

        this.TopLevelCommands[cmd.Name] = cmd;

        foreach (string xs in cmd.Aliases)
        {
            this.TopLevelCommands[xs] = cmd;
        }
    }
    #endregion

    #region Default Help
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class DefaultHelpModule : BaseCommandModule
    {
        [Command("help"), Description("Displays command help."), SuppressMessage("Quality Assurance", "CA1822:Mark members as static", Justification = "CommandsNext does not support static commands.")]
        public async Task DefaultHelpAsync(CommandContext ctx, [Description("Command to provide help for.")] params string[] command)
        {
            IEnumerable<Command> topLevel = ctx.CommandsNext.TopLevelCommands.Values.Distinct();
            BaseHelpFormatter helpBuilder = ctx.CommandsNext.HelpFormatter.Create(ctx);

            if (command != null && command.Length != 0)
            {
                Command? cmd = null;
                IEnumerable<Command>? searchIn = topLevel;
                foreach (string c in command)
                {
                    if (searchIn is null)
                    {
                        cmd = null;
                        break;
                    }

                    (StringComparison comparison, StringComparer comparer) = ctx.Config.CaseSensitive switch
                    {
                        true => (StringComparison.InvariantCulture, StringComparer.InvariantCulture),
                        false => (StringComparison.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase)
                    };
                    cmd = searchIn.FirstOrDefault(xc => xc.Name.Equals(c, comparison) || xc.Aliases.Contains(c, comparer));

                    if (cmd is null)
                    {
                        break;
                    }

                    IEnumerable<CheckBaseAttribute> failedChecks = await cmd.RunChecksAsync(ctx, true);
                    if (failedChecks.Any())
                    {
                        throw new ChecksFailedException(cmd, ctx, failedChecks);
                    }

                    searchIn = cmd is CommandGroup cmdGroup ? cmdGroup.Children : null;
                }

                if (cmd is null)
                {
                    throw new CommandNotFoundException(string.Join(" ", command));
                }

                helpBuilder.WithCommand(cmd);

                if (cmd is CommandGroup group)
                {
                    IEnumerable<Command> commandsToSearch = group.Children.Where(xc => !xc.IsHidden);
                    List<Command> eligibleCommands = [];
                    foreach (Command? candidateCommand in commandsToSearch)
                    {
                        if (candidateCommand.ExecutionChecks == null || !candidateCommand.ExecutionChecks.Any())
                        {
                            eligibleCommands.Add(candidateCommand);
                            continue;
                        }

                        IEnumerable<CheckBaseAttribute> candidateFailedChecks = await candidateCommand.RunChecksAsync(ctx, true);
                        if (!candidateFailedChecks.Any())
                        {
                            eligibleCommands.Add(candidateCommand);
                        }
                    }

                    if (eligibleCommands.Count != 0)
                    {
                        helpBuilder.WithSubcommands(eligibleCommands.OrderBy(xc => xc.Name));
                    }
                }
            }
            else
            {
                IEnumerable<Command> commandsToSearch = topLevel.Where(xc => !xc.IsHidden);
                List<Command> eligibleCommands = [];
                foreach (Command? sc in commandsToSearch)
                {
                    if (sc.ExecutionChecks == null || !sc.ExecutionChecks.Any())
                    {
                        eligibleCommands.Add(sc);
                        continue;
                    }

                    IEnumerable<CheckBaseAttribute> candidateFailedChecks = await sc.RunChecksAsync(ctx, true);
                    if (!candidateFailedChecks.Any())
                    {
                        eligibleCommands.Add(sc);
                    }
                }

                if (eligibleCommands.Count != 0)
                {
                    helpBuilder.WithSubcommands(eligibleCommands.OrderBy(xc => xc.Name));
                }
            }

            CommandHelpMessage helpMessage = helpBuilder.Build();

            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithContent(helpMessage.Content).AddEmbed(helpMessage.Embed);

            if (!ctx.Config.DmHelp || ctx.Channel is DiscordDmChannel || ctx.Guild is null || ctx.Member is null)
            {
                await ctx.RespondAsync(builder);
            }
            else
            {
                await ctx.Member.SendMessageAsync(builder);
            }
        }
    }
    #endregion

    #region Sudo
    /// <summary>
    /// Creates a fake command context to execute commands with.
    /// </summary>
    /// <param name="actor">The user or member to use as message author.</param>
    /// <param name="channel">The channel the message is supposed to appear from.</param>
    /// <param name="messageContents">Contents of the message.</param>
    /// <param name="prefix">Command prefix, used to execute commands.</param>
    /// <param name="cmd">Command to execute.</param>
    /// <param name="rawArguments">Raw arguments to pass to command.</param>
    /// <returns>Created fake context.</returns>
    public CommandContext CreateFakeContext(DiscordUser actor, DiscordChannel channel, string messageContents, string prefix, Command cmd, string? rawArguments = null)
    {
        DateTimeOffset epoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ulong timeSpan = (ulong)(now - epoch).TotalMilliseconds;

        // create fake message
        DiscordMessage msg = new()
        {
            Discord = this.Client,
            Author = actor,
            Channel = channel,
            ChannelId = channel.Id,
            Content = messageContents,
            Id = timeSpan << 22,
            Pinned = false,
            MentionEveryone = messageContents.Contains("@everyone"),
            IsTTS = false,
            attachments = [],
            embeds = [],
            Timestamp = now,
            reactions = []
        };

        List<DiscordUser> mentionedUsers = [];
        List<DiscordRole>? mentionedRoles = msg.Channel.Guild != null ? [] : null;
        List<DiscordChannel>? mentionedChannels = msg.Channel.Guild != null ? [] : null;

        if (!string.IsNullOrWhiteSpace(msg.Content))
        {
            if (msg.Channel.Guild != null)
            {
                mentionedUsers = Utilities.GetUserMentions(msg).Select(xid => msg.Channel.Guild.members.TryGetValue(xid, out DiscordMember? member) ? member : null).Cast<DiscordUser>().ToList();
                mentionedRoles = Utilities.GetRoleMentions(msg).Select(xid => msg.Channel.Guild.GetRole(xid)).ToList();
                mentionedChannels = Utilities.GetChannelMentions(msg).Select(xid => msg.Channel.Guild.GetChannel(xid)).ToList();
            }
            else
            {
                mentionedUsers = Utilities.GetUserMentions(msg).Select(this.Client.GetCachedOrEmptyUserInternal).ToList();
            }
        }

        msg.mentionedUsers = mentionedUsers;
        msg.mentionedRoles = mentionedRoles;
        msg.mentionedChannels = mentionedChannels;

        CommandContext ctx = new()
        {
            Client = this.Client,
            Command = cmd,
            Message = msg,
            Config = this.Config,
            RawArgumentString = rawArguments ?? "",
            Prefix = prefix,
            CommandsNext = this,
            Services = this.Services
        };

        if (cmd is not null && (cmd.Module is TransientCommandModule || cmd.Module is null))
        {
            IServiceScope scope = ctx.Services.CreateScope();
            ctx.ServiceScopeContext = new CommandContext.ServiceContext(ctx.Services, scope);
            ctx.Services = scope.ServiceProvider;
        }

        return ctx;
    }
    #endregion

    #region Type Conversion
    /// <summary>
    /// Converts a string to specified type.
    /// </summary>
    /// <typeparam name="T">Type to convert to.</typeparam>
    /// <param name="value">Value to convert.</param>
    /// <param name="ctx">Context in which to convert to.</param>
    /// <returns>Converted object.</returns>
    public async Task<object> ConvertArgumentAsync<T>(string value, CommandContext ctx)
    {
        Type t = typeof(T);
        if (!this.ArgumentConverters.TryGetValue(t, out IArgumentConverter argumentConverter))
        {
            throw new ArgumentException("There is no converter specified for given type.", nameof(T));
        }

        if (argumentConverter is not IArgumentConverter<T> cv)
        {
            throw new ArgumentException("Invalid converter registered for this type.", nameof(T));
        }

        Optional<T> cvr = await cv.ConvertAsync(value, ctx);
        return !cvr.HasValue ? throw new ArgumentException("Could not convert specified value to given type.", nameof(value)) : cvr.Value!;
    }

    /// <summary>
    /// Converts a string to specified type.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <param name="ctx">Context in which to convert to.</param>
    /// <param name="type">Type to convert to.</param>
    /// <returns>Converted object.</returns>
    public async Task<object> ConvertArgumentAsync(string? value, CommandContext ctx, Type type)
    {
        MethodInfo m = this.ConvertGeneric.MakeGenericMethod(type);
        try
        {
            return await (Task<object>)m.Invoke(this, [value, ctx]);
        }
        catch (Exception ex) when (ex is TargetInvocationException or InvalidCastException)
        {
            throw ex.InnerException;
        }
    }

    /// <summary>
    /// Registers an argument converter for specified type.
    /// </summary>
    /// <typeparam name="T">Type for which to register the converter.</typeparam>
    /// <param name="converter">Converter to register.</param>
    public void RegisterConverter<T>(IArgumentConverter<T> converter)
    {
        if (converter is null)
        {
            throw new ArgumentNullException(nameof(converter), "Converter cannot be null.");
        }

        Type t = typeof(T);
        TypeInfo ti = t.GetTypeInfo();
        this.ArgumentConverters[t] = converter;

        if (!ti.IsValueType)
        {
            return;
        }

        Type nullableConverterType = typeof(NullableConverter<>).MakeGenericType(t);
        Type nullableType = typeof(Nullable<>).MakeGenericType(t);
        if (this.ArgumentConverters.ContainsKey(nullableType))
        {
            return;
        }

        IArgumentConverter? nullableConverter = Activator.CreateInstance(nullableConverterType) as IArgumentConverter;

        if (nullableConverter is not null)
        {
            this.ArgumentConverters[nullableType] = nullableConverter;
        }
    }

    /// <summary>
    /// Unregisters an argument converter for specified type.
    /// </summary>
    /// <typeparam name="T">Type for which to unregister the converter.</typeparam>
    public void UnregisterConverter<T>()
    {
        Type t = typeof(T);
        TypeInfo ti = t.GetTypeInfo();
        this.ArgumentConverters.Remove(t);
        this.UserFriendlyTypeNames.Remove(t);

        if (!ti.IsValueType)
        {
            return;
        }

        Type nullableType = typeof(Nullable<>).MakeGenericType(t);
        if (!this.ArgumentConverters.ContainsKey(nullableType))
        {
            return;
        }

        this.ArgumentConverters.Remove(nullableType);
        this.UserFriendlyTypeNames.Remove(nullableType);
    }

    /// <summary>
    /// Registers a user-friendly type name.
    /// </summary>
    /// <typeparam name="T">Type to register the name for.</typeparam>
    /// <param name="value">Name to register.</param>
    public void RegisterUserFriendlyTypeName<T>(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(nameof(value), "Name cannot be null or empty.");
        }

        Type t = typeof(T);
        TypeInfo ti = t.GetTypeInfo();
        if (!this.ArgumentConverters.ContainsKey(t))
        {
            throw new InvalidOperationException("Cannot register a friendly name for a type which has no associated converter.");
        }

        this.UserFriendlyTypeNames[t] = value;

        if (!ti.IsValueType)
        {
            return;
        }

        Type nullableType = typeof(Nullable<>).MakeGenericType(t);
        this.UserFriendlyTypeNames[nullableType] = value;
    }

    /// <summary>
    /// Converts a type into user-friendly type name.
    /// </summary>
    /// <param name="t">Type to convert.</param>
    /// <returns>User-friendly type name.</returns>
    public string GetUserFriendlyTypeName(Type t)
    {
        if (this.UserFriendlyTypeNames.TryGetValue(t, out string value))
        {
            return value;
        }

        TypeInfo ti = t.GetTypeInfo();
        if (ti.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            Type tn = ti.GenericTypeArguments[0];
            return this.UserFriendlyTypeNames.TryGetValue(tn, out value) ? value : tn.Name;
        }

        return t.Name;
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Gets the configuration-specific string comparer. This returns <see cref="StringComparer.Ordinal"/> or <see cref="StringComparer.OrdinalIgnoreCase"/>,
    /// depending on whether <see cref="CommandsNextConfiguration.CaseSensitive"/> is set to <see langword="true"/> or <see langword="false"/>.
    /// </summary>
    /// <returns>A string comparer.</returns>
    internal IEqualityComparer<string> GetStringComparer()
        => this.Config.CaseSensitive
            ? StringComparer.Ordinal
            : StringComparer.OrdinalIgnoreCase;
    #endregion

    #region Events
    /// <summary>
    /// Triggered whenever a command executes successfully.
    /// </summary>
    public event AsyncEventHandler<CommandsNextExtension, CommandExecutionEventArgs> CommandExecuted
    {
        add => this.executed.Register(value);
        remove => this.executed.Unregister(value);
    }
    private AsyncEvent<CommandsNextExtension, CommandExecutionEventArgs> executed = null!;

    /// <summary>
    /// Triggered whenever a command throws an exception during execution.
    /// </summary>
    public event AsyncEventHandler<CommandsNextExtension, CommandErrorEventArgs> CommandErrored
    {
        add => this.error.Register(value);
        remove => this.error.Unregister(value);
    }
    private AsyncEvent<CommandsNextExtension, CommandErrorEventArgs> error = null!;

    private Task OnCommandExecuted(CommandExecutionEventArgs e)
        => this.executed.InvokeAsync(this, e);

    private Task OnCommandErrored(CommandErrorEventArgs e)
        => this.error.InvokeAsync(this, e);
    #endregion
}
