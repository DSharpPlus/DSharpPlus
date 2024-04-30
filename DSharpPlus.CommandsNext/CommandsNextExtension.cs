using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
public class CommandsNextExtension : BaseExtension
{
    private CommandsNextConfiguration Config { get; }
    private HelpFormatterFactory HelpFormatter { get; }

    private MethodInfo ConvertGeneric { get; }
    private Dictionary<Type, string> UserFriendlyTypeNames { get; }
    internal Dictionary<Type, IArgumentConverter> ArgumentConverters { get; }
    internal CultureInfo DefaultParserCulture
        => Config.DefaultParserCulture;

    /// <summary>
    /// Gets the service provider this CommandsNext module was configured with.
    /// </summary>
    public IServiceProvider Services
        => Config.Services;

    internal CommandsNextExtension(CommandsNextConfiguration cfg)
    {
        Config = new CommandsNextConfiguration(cfg);
        TopLevelCommands = new Dictionary<string, Command>();
        _registeredCommandsLazy = new Lazy<IReadOnlyDictionary<string, Command>>(() => new ReadOnlyDictionary<string, Command>(TopLevelCommands));
        HelpFormatter = new HelpFormatterFactory();
        HelpFormatter.SetFormatterType<DefaultHelpFormatter>();

        ArgumentConverters = new Dictionary<Type, IArgumentConverter>
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

        UserFriendlyTypeNames = new Dictionary<Type, string>()
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
        Type[] cvts = ArgumentConverters.Keys.ToArray();
        foreach (Type? xt in cvts)
        {
            TypeInfo xti = xt.GetTypeInfo();
            if (!xti.IsValueType)
            {
                continue;
            }

            Type xcvt = ncvt.MakeGenericType(xt);
            Type xnt = nt.MakeGenericType(xt);

            if (ArgumentConverters.ContainsKey(xcvt) || Activator.CreateInstance(xcvt) is not IArgumentConverter xcv)
            {
                continue;
            }

            ArgumentConverters[xnt] = xcv;
            UserFriendlyTypeNames[xnt] = UserFriendlyTypeNames[xt];
        }

        Type t = typeof(CommandsNextExtension);
        IEnumerable<MethodInfo> ms = t.GetTypeInfo().DeclaredMethods;
        MethodInfo? m = ms.FirstOrDefault(xm => xm.Name == nameof(ConvertArgument) && xm.ContainsGenericParameters && !xm.IsStatic && xm.IsPublic);
        ConvertGeneric = m;
    }

    /// <summary>
    /// Sets the help formatter to use with the default help command.
    /// </summary>
    /// <typeparam name="T">Type of the formatter to use.</typeparam>
    public void SetHelpFormatter<T>() where T : BaseHelpFormatter => HelpFormatter.SetFormatterType<T>();

    /// <summary>
    /// Disposes of this the resources used by CNext.
    /// </summary>
    public override void Dispose()
    {
        Config.CommandExecutor.Dispose();

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }

    #region DiscordClient Registration
    /// <summary>
    /// DO NOT USE THIS MANUALLY.
    /// </summary>
    /// <param name="client">DO NOT USE THIS MANUALLY.</param>
    /// <exception cref="InvalidOperationException"/>
    protected internal override void Setup(DiscordClient client)
    {
        if (Client != null)
        {
            throw new InvalidOperationException("What did I tell you?");
        }

        Client = client;

        _executed = new AsyncEvent<CommandsNextExtension, CommandExecutionEventArgs>("COMMAND_EXECUTED", Client.EventErrorHandler);
        _error = new AsyncEvent<CommandsNextExtension, CommandErrorEventArgs>("COMMAND_ERRORED", Client.EventErrorHandler);

        if (Config.UseDefaultCommandHandler)
        {
            Client.MessageCreated += HandleCommandsAsync;
        }
        else
        {
            Client.Logger.LogWarning(CommandsNextEvents.Misc, "Not attaching default command handler - if this is intentional, you can ignore this message");
        }

        if (Config.EnableDefaultHelp)
        {
            RegisterCommands(typeof(DefaultHelpModule), null, Enumerable.Empty<CheckBaseAttribute>(), out List<CommandBuilder>? tcmds);

            if (Config.DefaultHelpChecks.Any())
            {
                CheckBaseAttribute[] checks = Config.DefaultHelpChecks.ToArray();

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

        if (Config.CommandExecutor is ParallelQueuedCommandExecutor pqce)
        {
            Client.Logger.LogDebug(CommandsNextEvents.Misc, "Using parallel executor with degree {0}", pqce.Parallelism);
        }
    }
    #endregion

    #region Command Handling
    private async Task HandleCommandsAsync(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot) // bad bot
        {
            return;
        }

        if (!Config.EnableDms && e.Channel.IsPrivate)
        {
            return;
        }

        int mpos = -1;
        if (Config.EnableMentionPrefix)
        {
            mpos = e.Message.GetMentionPrefixLength(Client.CurrentUser);
        }

        if (Config.StringPrefixes.Any())
        {
            foreach (string pfix in Config.StringPrefixes)
            {
                if (mpos == -1 && !string.IsNullOrWhiteSpace(pfix))
                {
                    mpos = e.Message.GetStringPrefixLength(pfix, Config.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        if (mpos == -1 && Config.PrefixResolver != null)
        {
            mpos = await Config.PrefixResolver(e.Message);
        }

        if (mpos == -1)
        {
            return;
        }

        string pfx = e.Message.Content.Substring(0, mpos);
        string cnt = e.Message.Content.Substring(mpos);

        int __ = 0;
        string? fname = cnt.ExtractNextArgument(ref __, Config.QuotationMarks);

        Command? cmd = FindCommand(cnt, out string? args);
        CommandContext ctx = CreateContext(e.Message, pfx, cmd, args);

        if (cmd is null)
        {
            await _error.InvokeAsync(this, new CommandErrorEventArgs { Context = ctx, Exception = new CommandNotFoundException(fname ?? "UnknownCmd") });
            return;
        }

        await Config.CommandExecutor.ExecuteAsync(ctx);
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

        bool ignoreCase = !Config.CaseSensitive;
        int pos = 0;
        string? next = commandString.ExtractNextArgument(ref pos, Config.QuotationMarks);
        if (next is null)
        {
            return null;
        }

        if (!RegisteredCommands.TryGetValue(next, out Command? cmd))
        {
            if (!ignoreCase)
            {
                return null;
            }

            KeyValuePair<string, Command> cmdKvp = RegisteredCommands.FirstOrDefault(x => x.Key.Equals(next, StringComparison.InvariantCultureIgnoreCase));
            if (cmdKvp.Value is null)
            {
                return null;
            }

            cmd = cmdKvp.Value;
        }

        if (cmd is not CommandGroup)
        {
            rawArguments = commandString.Substring(pos).Trim();
            return cmd;
        }

        while (cmd is CommandGroup)
        {
            CommandGroup? cm2 = cmd as CommandGroup;
            int oldPos = pos;
            next = commandString.ExtractNextArgument(ref pos, Config.QuotationMarks);
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

        rawArguments = commandString.Substring(pos).Trim();
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
        CommandContext ctx = new CommandContext
        {
            Client = Client,
            Command = cmd,
            Message = msg,
            Config = Config,
            RawArgumentString = rawArguments ?? "",
            Prefix = prefix,
            CommandsNext = this,
            Services = Services
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
                await _executed.InvokeAsync(this, new CommandExecutionEventArgs { Context = res.Context });
            }
            else
            {
                await _error.InvokeAsync(this, new CommandErrorEventArgs { Context = res.Context, Exception = res.Exception });
            }
        }
        catch (Exception ex)
        {
            await _error.InvokeAsync(this, new CommandErrorEventArgs { Context = ctx, Exception = ex });
        }
        finally
        {
            if (ctx.ServiceScopeContext.IsInitialized)
            {
                ctx.ServiceScopeContext.Dispose();
            }
        }
    }

    private async Task RunAllChecksAsync(Command cmd, CommandContext ctx)
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
        => _registeredCommandsLazy.Value;

    private Dictionary<string, Command> TopLevelCommands { get; set; }
    private readonly Lazy<IReadOnlyDictionary<string, Command>> _registeredCommandsLazy;

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

        RegisterCommands(t, null, Enumerable.Empty<CheckBaseAttribute>(), out List<CommandBuilder>? tempCommands);

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
            .Build(Services);

        // restrict parent lifespan to more or equally restrictive
        if (currentParent?.Module is TransientCommandModule && moduleLifespan != ModuleLifespan.Transient)
        {
            throw new InvalidOperationException("In a transient module, child modules can only be transient.");
        }

        // check if we are anything
        CommandGroupBuilder? groupBuilder = new CommandGroupBuilder(module);
        bool isModule = false;
        IEnumerable<Attribute> moduleAttributes = ti.GetCustomAttributes();
        bool moduleHidden = false;
        List<CheckBaseAttribute> moduleChecks = new List<CheckBaseAttribute>();

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
                            moduleName = moduleName.Substring(0, moduleName.Length - 5);
                        }
                        else if (moduleName.EndsWith("Module") && moduleName != "Module")
                        {
                            moduleName = moduleName.Substring(0, moduleName.Length - 6);
                        }
                        else if (moduleName.EndsWith("Commands") && moduleName != "Commands")
                        {
                            moduleName = moduleName.Substring(0, moduleName.Length - 8);
                        }
                    }

                    if (!Config.CaseSensitive)
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
                        groupBuilder.WithAlias(Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
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
        List<CommandBuilder> commands = new List<CommandBuilder>();
        Dictionary<string, CommandBuilder> commandBuilders = new Dictionary<string, CommandBuilder>();
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
                    commandName = commandName.Substring(0, commandName.Length - 5);
                }
            }

            if (!Config.CaseSensitive)
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

            if (!isModule && moduleChecks.Any())
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
                            commandBuilder.WithAlias(Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
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

        List<string> keys = RegisteredCommands.Where(x => cmds.Contains(x.Value)).Select(x => x.Key).ToList();
        foreach (string? key in keys)
        {
            TopLevelCommands.Remove(key);
        }
    }

    private string? ExtractCategoryAttribute(MethodInfo method)
    {
        CategoryAttribute attribute = method.GetCustomAttribute<CategoryAttribute>();

        if (attribute is not null)
        {
            return attribute.Name;
        }

        // extract from types

        return ExtractCategoryAttribute(method.DeclaringType);
    }

    private string? ExtractCategoryAttribute(Type type)
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

        if (TopLevelCommands.ContainsKey(cmd.Name) || cmd.Aliases.Any(xs => TopLevelCommands.ContainsKey(xs)))
        {
            throw new DuplicateCommandException(cmd.QualifiedName);
        }

        TopLevelCommands[cmd.Name] = cmd;

        foreach (string xs in cmd.Aliases)
        {
            TopLevelCommands[xs] = cmd;
        }
    }
    #endregion

    #region Default Help
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class DefaultHelpModule : BaseCommandModule
    {
        [Command("help"), Description("Displays command help.")]
        public async Task DefaultHelpAsync(CommandContext ctx, [Description("Command to provide help for.")] params string[] command)
        {
            IEnumerable<Command> topLevel = ctx.CommandsNext.TopLevelCommands.Values.Distinct();
            BaseHelpFormatter helpBuilder = ctx.CommandsNext.HelpFormatter.Create(ctx);

            if (command != null && command.Any())
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
                    List<Command> eligibleCommands = new List<Command>();
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

                    if (eligibleCommands.Any())
                    {
                        helpBuilder.WithSubcommands(eligibleCommands.OrderBy(xc => xc.Name));
                    }
                }
            }
            else
            {
                IEnumerable<Command> commandsToSearch = topLevel.Where(xc => !xc.IsHidden);
                List<Command> eligibleCommands = new List<Command>();
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

                if (eligibleCommands.Any())
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
        DateTimeOffset epoch = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ulong timeSpan = (ulong)(now - epoch).TotalMilliseconds;

        // create fake message
        DiscordMessage msg = new DiscordMessage
        {
            Discord = Client,
            Author = actor,
            Channel = channel,
            ChannelId = channel.Id,
            Content = messageContents,
            Id = timeSpan << 22,
            Pinned = false,
            MentionEveryone = messageContents.Contains("@everyone"),
            IsTTS = false,
            _attachments = new List<DiscordAttachment>(),
            _embeds = new List<DiscordEmbed>(),
            Timestamp = now,
            _reactions = new List<DiscordReaction>()
        };

        List<DiscordUser> mentionedUsers = new List<DiscordUser>();
        List<DiscordRole>? mentionedRoles = msg.Channel.Guild != null ? new List<DiscordRole>() : null;
        List<DiscordChannel>? mentionedChannels = msg.Channel.Guild != null ? new List<DiscordChannel>() : null;

        if (!string.IsNullOrWhiteSpace(msg.Content))
        {
            if (msg.Channel.Guild != null)
            {
                mentionedUsers = Utilities.GetUserMentions(msg).Select(xid => msg.Channel.Guild._members.TryGetValue(xid, out DiscordMember? member) ? member : null).Cast<DiscordUser>().ToList();
                mentionedRoles = Utilities.GetRoleMentions(msg).Select(xid => msg.Channel.Guild.GetRole(xid)).ToList();
                mentionedChannels = Utilities.GetChannelMentions(msg).Select(xid => msg.Channel.Guild.GetChannel(xid)).ToList();
            }
            else
            {
                mentionedUsers = Utilities.GetUserMentions(msg).Select(Client.GetCachedOrEmptyUserInternal).ToList();
            }
        }

        msg._mentionedUsers = mentionedUsers;
        msg._mentionedRoles = mentionedRoles;
        msg._mentionedChannels = mentionedChannels;

        CommandContext ctx = new CommandContext
        {
            Client = Client,
            Command = cmd,
            Message = msg,
            Config = Config,
            RawArgumentString = rawArguments ?? "",
            Prefix = prefix,
            CommandsNext = this,
            Services = Services
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
    public async Task<object> ConvertArgument<T>(string value, CommandContext ctx)
    {
        Type t = typeof(T);
        if (!ArgumentConverters.ContainsKey(t))
        {
            throw new ArgumentException("There is no converter specified for given type.", nameof(T));
        }

        if (ArgumentConverters[t] is not IArgumentConverter<T> cv)
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
    public async Task<object> ConvertArgument(string? value, CommandContext ctx, Type type)
    {
        MethodInfo m = ConvertGeneric.MakeGenericMethod(type);
        try
        {
            return await (Task<object>)m.Invoke(this, new object?[] { value, ctx });
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
        ArgumentConverters[t] = converter;

        if (!ti.IsValueType)
        {
            return;
        }

        Type nullableConverterType = typeof(NullableConverter<>).MakeGenericType(t);
        Type nullableType = typeof(Nullable<>).MakeGenericType(t);
        if (ArgumentConverters.ContainsKey(nullableType))
        {
            return;
        }

        IArgumentConverter? nullableConverter = Activator.CreateInstance(nullableConverterType) as IArgumentConverter;

        if (nullableConverter is not null)
        {
            ArgumentConverters[nullableType] = nullableConverter;
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
        if (ArgumentConverters.ContainsKey(t))
        {
            ArgumentConverters.Remove(t);
        }

        if (UserFriendlyTypeNames.ContainsKey(t))
        {
            UserFriendlyTypeNames.Remove(t);
        }

        if (!ti.IsValueType)
        {
            return;
        }

        Type nullableType = typeof(Nullable<>).MakeGenericType(t);
        if (!ArgumentConverters.ContainsKey(nullableType))
        {
            return;
        }

        ArgumentConverters.Remove(nullableType);
        UserFriendlyTypeNames.Remove(nullableType);
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
        if (!ArgumentConverters.ContainsKey(t))
        {
            throw new InvalidOperationException("Cannot register a friendly name for a type which has no associated converter.");
        }

        UserFriendlyTypeNames[t] = value;

        if (!ti.IsValueType)
        {
            return;
        }

        Type nullableType = typeof(Nullable<>).MakeGenericType(t);
        UserFriendlyTypeNames[nullableType] = value;
    }

    /// <summary>
    /// Converts a type into user-friendly type name.
    /// </summary>
    /// <param name="t">Type to convert.</param>
    /// <returns>User-friendly type name.</returns>
    public string GetUserFriendlyTypeName(Type t)
    {
        if (UserFriendlyTypeNames.ContainsKey(t))
        {
            return UserFriendlyTypeNames[t];
        }

        TypeInfo ti = t.GetTypeInfo();
        if (ti.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            Type tn = ti.GenericTypeArguments[0];
            return UserFriendlyTypeNames.ContainsKey(tn) ? UserFriendlyTypeNames[tn] : tn.Name;
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
        => Config.CaseSensitive
            ? StringComparer.Ordinal
            : StringComparer.OrdinalIgnoreCase;
    #endregion

    #region Events
    /// <summary>
    /// Triggered whenever a command executes successfully.
    /// </summary>
    public event AsyncEventHandler<CommandsNextExtension, CommandExecutionEventArgs> CommandExecuted
    {
        add => _executed.Register(value);
        remove => _executed.Unregister(value);
    }
    private AsyncEvent<CommandsNextExtension, CommandExecutionEventArgs> _executed = null!;

    /// <summary>
    /// Triggered whenever a command throws an exception during execution.
    /// </summary>
    public event AsyncEventHandler<CommandsNextExtension, CommandErrorEventArgs> CommandErrored
    {
        add => _error.Register(value);
        remove => _error.Unregister(value);
    }
    private AsyncEvent<CommandsNextExtension, CommandErrorEventArgs> _error = null!;

    private Task OnCommandExecuted(CommandExecutionEventArgs e)
        => _executed.InvokeAsync(this, e);

    private Task OnCommandErrored(CommandErrorEventArgs e)
        => _error.InvokeAsync(this, e);
    #endregion
}
