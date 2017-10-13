using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// This is the class which handles command registration, management, and execution. 
    /// </summary>
    public class CommandsNextExtension : BaseExtension
    {
        #region Events
        /// <summary>
        /// Triggered whenever a command executes successfully.
        /// </summary>
        public event AsyncEventHandler<CommandExecutionEventArgs> CommandExecuted
        {
            add => _executed.Register(value);
            remove => _executed.Unregister(value);
        }
        private AsyncEvent<CommandExecutionEventArgs> _executed;

        /// <summary>
        /// Triggered whenever a command throws an exception during execution.
        /// </summary>
        public event AsyncEventHandler<CommandErrorEventArgs> CommandErrored
        {
            add => _error.Register(value);
            remove => _error.Unregister(value);
        }
        private AsyncEvent<CommandErrorEventArgs> _error;

        // ReSharper disable once UnusedMember.Local
        private async Task OnCommandExecuted(CommandExecutionEventArgs e) =>
            await _executed.InvokeAsync(e).ConfigureAwait(false);
        
        // ReSharper disable once UnusedMember.Local
        private async Task OnCommandErrored(CommandErrorEventArgs e) =>
            await _error.InvokeAsync(e).ConfigureAwait(false);
        #endregion

        private CommandsNextConfiguration Config { get; }
        private Type HelpFormatterType { get; set; } = typeof(DefaultHelpFormatter);
        private const string GroupCommandMethodName = "ExecuteGroupAsync";

        /// <summary>
        /// Gets the dependency collection this CommandsNext module was configured with.
        /// </summary>
        public DependencyCollection Dependencies => Config.Dependencies;

        internal CommandsNextExtension(CommandsNextConfiguration cfg)
        {
            Config = cfg;
            TopLevelCommands = new Dictionary<string, Command>();
            _registeredCommandsLazy = new Lazy<IReadOnlyDictionary<string, Command>>(() => new ReadOnlyDictionary<string, Command>(TopLevelCommands));
        }

        /// <summary>
        /// Sets the help formatter to use with the default help command.
        /// </summary>
        /// <typeparam name="T">Type of the formatter to use.</typeparam>
        public void SetHelpFormatter<T>() where T : class, IHelpFormatter, new()
        {
            HelpFormatterType = typeof(T);
        }

        #region Helpers
        /// <summary>
        /// Registers an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to register the converter.</typeparam>
        /// <param name="converter">Converter to register.</param>
        public void RegisterConverter<T>(IArgumentConverter<T> converter) =>
            CommandsNextUtilities.RegisterConverter(converter);

        /// <summary>
        /// Unregisters an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to unregister the converter.</typeparam>
        public void UnregisterConverter<T>() =>
            CommandsNextUtilities.UnregisterConverter<T>();

        /// <summary>
        /// Registers a user-friendly type name.
        /// </summary>
        /// <typeparam name="T">Type to register the name for.</typeparam>
        /// <param name="value">Name to register.</param>
        public void RegisterUserFriendlyTypeName<T>(string value) =>
            CommandsNextUtilities.RegisterUserFriendlyTypeName<T>(value);
        #endregion

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

            _executed = new AsyncEvent<CommandExecutionEventArgs>(Client.EventErrorHandler, "COMMAND_EXECUTED");
            _error = new AsyncEvent<CommandErrorEventArgs>(Client.EventErrorHandler, "COMMAND_ERRORED");

            Client.MessageCreated += HandleCommandsAsync;

            if (Config.EnableDefaultHelp)
            {
                var dlg = new Func<CommandContext, string[], Task>(DefaultHelpAsync);
                var mi = dlg.GetMethodInfo();
                MakeCallable(mi, dlg.Target, out var cbl, out var args);

                var attrs = mi.GetCustomAttributes();
                var attributes = attrs as Attribute[] ?? attrs.ToArray();
                if (attributes.All(xa => xa.GetType() != typeof(CommandAttribute)))
                {
                    return;
                }

                var cmd = new Command();

                var cbas = new List<CheckBaseAttribute>();
                foreach (var xa in attributes)
                {
                    switch (xa)
                    {
                        case CommandAttribute c:
                            cmd.Name = c.Name;
                            break;

                        case AliasesAttribute a:
                            cmd.Aliases = a.Aliases;
                            break;

                        case CheckBaseAttribute p:
                            cbas.Add(p);
                            break;

                        case DescriptionAttribute d:
                            cmd.Description = d.Description;
                            break;

                        case HiddenAttribute _:
                            cmd.IsHidden = true;
                            break;
                    }
                }

                if (Config.DefaultHelpChecks != null && Config.DefaultHelpChecks.Any())
                {
                    cbas.AddRange(Config.DefaultHelpChecks);
                }

                cmd.ExecutionChecks = new ReadOnlyCollection<CheckBaseAttribute>(cbas);
                cmd.Arguments = args;
                cmd.Callable = cbl;

                AddToCommandDictionary(cmd);
            }
        }
        #endregion

        #region Command Handler
        public async Task HandleCommandsAsync(MessageCreateEventArgs e)
        {
            // Let the bot do its things
            await Task.Yield();

            if (e.Author.IsBot) // bad bot
            {
                return;
            }

            if (!Config.EnableDms && e.Channel.IsPrivate)
            {
                return;
            }

            if (Config.SelfBot && e.Author.Id != Client.CurrentUser.Id)
            {
                return;
            }

            var mpos = -1;
            if (Config.EnableMentionPrefix)
            {
                mpos = e.Message.GetMentionPrefixLength(Client.CurrentUser);
            }

            if (mpos == -1 && !string.IsNullOrWhiteSpace(Config.StringPrefix))
            {
                mpos = e.Message.GetStringPrefixLength(Config.StringPrefix);
            }

            if (mpos == -1 && Config.CustomPrefixPredicate != null)
            {
                mpos = await Config.CustomPrefixPredicate(e.Message);
            }

            if (mpos == -1)
            {
                return;
            }

            var cnt = e.Message.Content.Substring(mpos);
            var cms = CommandsNextUtilities.ExtractNextArgument(cnt, out var rrg);

            var cmd = TopLevelCommands.ContainsKey(cms) ? TopLevelCommands[cms] : null;
            if (cmd == null && !Config.CaseSensitive)
            {
                cmd = TopLevelCommands.FirstOrDefault(xkvp => xkvp.Key.ToLowerInvariant() == cms.ToLowerInvariant()).Value;
            }

            var ctx = new CommandContext
            {
                Client = Client,
                Command = cmd,
                Message = e.Message,
                //RawArguments = new ReadOnlyCollection<string>(arg.ToList()),
                Config = Config,
                RawArgumentString = rrg,
                CommandsNext = this,
                Dependencies = Config.Dependencies
            };

            if (cmd == null)
            {
                await _error.InvokeAsync(new CommandErrorEventArgs { Context = ctx, Exception = new CommandNotFoundException(cms) });
                return;
            }

            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = Task.Run(async () =>
            {
                try
                {
                    var fchecks = await cmd.RunChecksAsync(ctx, false);
                    var checkBaseAttributes = fchecks as CheckBaseAttribute[] ?? fchecks.ToArray();
                    if (checkBaseAttributes.Any())
                    {
                        throw new ChecksFailedException(cmd, ctx, checkBaseAttributes);
                    }

                    var res = await cmd.ExecuteAsync(ctx);
                    
                    if (res.IsSuccessful)
                    {
                        await _executed.InvokeAsync(new CommandExecutionEventArgs { Context = res.Context });
                    }
                    else
                    {
                        await _error.InvokeAsync(new CommandErrorEventArgs { Context = res.Context, Exception = res.Exception });
                    }
                }
                catch (Exception ex)
                {
                    await _error.InvokeAsync(new CommandErrorEventArgs { Context = ctx, Exception = ex });
                }
            });
        }
        #endregion

        #region Command Registration
        private Dictionary<string, Command> TopLevelCommands { get; set; }
        private Lazy<IReadOnlyDictionary<string, Command>> _registeredCommandsLazy;
        public IReadOnlyDictionary<string, Command> RegisteredCommands => _registeredCommandsLazy.Value;

        /// <summary>
        /// Registers all commands from a given assembly. The command classes need to be public to be considered for registration.
        /// </summary>
        /// <param name="assembly">Assembly to register commands from.</param>
        public void RegisterCommands(Assembly assembly)
        {
            var types = assembly.ExportedTypes.Where(xt =>
            {
                var xti = xt.GetTypeInfo();
                if (!xti.IsModuleCandidateType() || xti.IsNested)
                {
                    return false;
                }

                return xti.DeclaredMethods.Any(xmi => xmi.IsCommandCandidate(out _));
            });
            foreach (var xt in types)
            {
                RegisterCommands(xt);
            }
        }

        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <typeparam name="T">Class which holds commands to register.</typeparam>
        public void RegisterCommands<T>() where T : class
        {
            var t = typeof(T);
            RegisterCommands(t);
        }

        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <param name="t">Type of the class which holds commands to register.</param>
        public void RegisterCommands(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t), "Type cannot be null.");
            }

            if (!t.IsModuleCandidateType())
            {
                throw new ArgumentNullException(nameof(t), "Type must be a class, which cannot be abstract or static.");
            }

            RegisterCommands(t, CreateInstance(t), null, out var tres, out var tcmds);
            
            if (tres != null)
            {
                AddToCommandDictionary(tres);
            }

            if (tcmds != null)
            {
                foreach (var xc in tcmds)
                {
                    AddToCommandDictionary(xc);
                }
            }
        }

        private void RegisterCommands(Type t, object inst, CommandGroup currentparent, out CommandGroup result, out IReadOnlyList<Command> commands)
        {
            var ti = t.GetTypeInfo();

            // check if we are anything
            var mdlAttrs = ti.GetCustomAttributes();
            var isMdl = false;
            var mdlName = "";
            IReadOnlyList<string> mdlAliases = null;
            var mdlHidden = false;
            var mdlDesc = "";
            var mdlChks = new List<CheckBaseAttribute>();
            Delegate mdlCbl = null;
            IReadOnlyList<CommandArgument> mdlArgs = null;
            CommandGroup mdl = null;
            foreach (var xa in mdlAttrs)
            {
                switch (xa)
                {
                    case GroupAttribute g:
                        isMdl = true;
                        mdlName = g.Name;
                        if (g.CanInvokeWithoutSubcommand)
                        {
                            MakeCallableModule(ti, inst, out mdlCbl, out mdlArgs);
                        }
                        break;

                    case AliasesAttribute a:
                        mdlAliases = a.Aliases;
                        break;

                    case HiddenAttribute _:
                        mdlHidden = true;
                        break;

                    case DescriptionAttribute d:
                        mdlDesc = d.Description;
                        break;

                    case CheckBaseAttribute c:
                        mdlChks.Add(c);
                        break;
                }
            }

            if (isMdl)
            {
                mdl = new CommandGroup
                {
                    Name = mdlName,
                    Aliases = mdlAliases,
                    Description = mdlDesc,
                    ExecutionChecks = new ReadOnlyCollection<CheckBaseAttribute>(mdlChks),
                    IsHidden = mdlHidden,
                    Parent = currentparent,
                    Callable = mdlCbl,
                    Arguments = mdlArgs,
                    Children = null
                };
            }

            // candidate methods
            var ms = ti.DeclaredMethods
                .Where(xm => xm.IsPublic && !xm.IsStatic && xm.Name != GroupCommandMethodName);
            var cmds = new List<Command>();
            foreach (var m in ms)
            {
                if (m.ReturnType != typeof(Task))
                {
                    continue;
                }

                var ps = m.GetParameters();
                if (!ps.Any() || ps.First().ParameterType != typeof(CommandContext))
                {
                    continue;
                }

                var attrs = m.GetCustomAttributes();
                var attributes = attrs as Attribute[] ?? attrs.ToArray();
                if (attributes.All(xa => xa.GetType() != typeof(CommandAttribute)))
                {
                    continue;
                }

                var cmd = new Command();

                var cbas = new List<CheckBaseAttribute>();
                if (!isMdl && mdlChks.Any())
                {
                    cbas.AddRange(mdlChks);
                }

                foreach (var xa in attributes)
                {
                    switch (xa)
                    {
                        case CommandAttribute c:
                            cmd.Name = c.Name;
                            break;

                        case AliasesAttribute a:
                            cmd.Aliases = a.Aliases;
                            break;

                        case CheckBaseAttribute p:
                            cbas.Add(p);
                            break;

                        case DescriptionAttribute d:
                            cmd.Description = d.Description;
                            break;

                        case HiddenAttribute _:
                            cmd.IsHidden = true;
                            break;
                    }
                }
                cmd.ExecutionChecks = new ReadOnlyCollection<CheckBaseAttribute>(cbas);
                cmd.Parent = mdl;
                MakeCallable(m, inst, out var cbl, out var args);
                cmd.Callable = cbl;
                cmd.Arguments = args;

                cmds.Add(cmd);
            }

            // candidate types
            var ts = ti.DeclaredNestedTypes
                .Where(xt => xt.IsModuleCandidateType() && xt.DeclaredConstructors.Any(xc => xc.IsPublic));
            foreach (var xt in ts)
            {
                RegisterCommands(xt.AsType(), CreateInstance(xt.AsType()), mdl, out var tmdl, out var tcmds);

                if (tmdl != null)
                {
                    cmds.Add(tmdl);
                }
                cmds.AddRange(tcmds);
            }

            commands = new ReadOnlyCollection<Command>(cmds);
            if (mdl != null)
            {
                mdl.Children = commands;
                commands = new ReadOnlyCollection<Command>(new List<Command>());
            }
            result = mdl;
        }

        private void MakeCallable(MethodInfo mi, object inst, out Delegate cbl, out IReadOnlyList<CommandArgument> args)
        {
            if (!mi.IsCommandCandidate(out var ps))
            {
                throw new MissingMethodException("Specified method is not suitable for a command.");
            }

            var ei = Expression.Constant(inst);

            var ea = new ParameterExpression[ps.Length];
            ea[0] = Expression.Parameter(typeof(CommandContext), "ctx");

            var i = 1;
            var ps1 = ps.Skip(1);
            var argsl = new List<CommandArgument>(ps.Length - 1);
            foreach (var xp in ps1)
            {
                var ca = new CommandArgument
                {
                    Name = xp.Name,
                    Type = xp.ParameterType,
                    IsOptional = xp.IsOptional,
                    DefaultValue = xp.IsOptional ? xp.DefaultValue : null
                };

                var attrs = xp.GetCustomAttributes();
                foreach (var xa in attrs)
                {
                    switch (xa)
                    {
                        case DescriptionAttribute d:
                            ca.Description = d.Description;
                            break;

                        case RemainingTextAttribute _:
                            ca.IsCatchAll = true;
                            break;

                        case ParamArrayAttribute _:
                            ca.IsCatchAll = true;
                            ca.Type = xp.ParameterType.GetElementType();
                            ca.IsArray = true;
                            break;
                    }
                }

                if (i > 1 && !ca.IsOptional && !ca.IsCatchAll && argsl[i - 2].IsOptional)
                {
                    throw new InvalidOperationException("Non-optional argument cannot appear after an optional one");
                }

                argsl.Add(ca);
                ea[i++] = Expression.Parameter(xp.ParameterType, xp.Name);
            }

            // ReSharper disable once CoVariantArrayConversion
            var ec = Expression.Call(ei, mi, ea);
            var el = Expression.Lambda(ec, ea);

            cbl = el.Compile();
            args = new ReadOnlyCollection<CommandArgument>(argsl);
        }

        private void MakeCallableModule(TypeInfo ti, object inst, out Delegate cbl, out IReadOnlyList<CommandArgument> args)
        {
            var mtd = ti.GetDeclaredMethod(GroupCommandMethodName);
            if (mtd == null)
            {
                throw new MissingMethodException($"A group marked with CanExecute must have a method named {GroupCommandMethodName}.");
            }

            MakeCallable(mtd, inst, out cbl, out args);
        }

        private object CreateInstance(Type t)
        {
            var ti = t.GetTypeInfo();
            var cs = ti.DeclaredConstructors
                .Where(xci => xci.IsPublic)
                .ToArray();

            if (cs.Length != 1)
            {
                throw new ArgumentException("Specified type does not contain a public constructor or contains more than one public constructor.");
            }

            var constr = cs[0];
            var prms = constr.GetParameters();
            var args = new object[prms.Length];
            var deps = Config.Dependencies;

            if (prms.Length != 0 && deps == null)
            {
                throw new InvalidOperationException("Dependency collection needs to be specified for parametered constructors.");
            }

            if (prms.Length != 0)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = deps.GetDependency(prms[i].ParameterType);
                }
            }

            return Activator.CreateInstance(t, args);
        }

        private void AddToCommandDictionary(Command cmd)
        {
            if (cmd.Parent != null)
            {
                return;
            }

            if (TopLevelCommands.ContainsKey(cmd.Name) || cmd.Aliases != null && cmd.Aliases.Any(xs => TopLevelCommands.ContainsKey(xs)))
            {
                throw new DuplicateCommandException(cmd.Name);
            }

            TopLevelCommands[cmd.Name] = cmd;
            if (cmd.Aliases != null)
            {
                foreach (var xs in cmd.Aliases)
                {
                    TopLevelCommands[xs] = cmd;
                }
            }
        }
        #endregion

        #region Default Help
        [Command("help"), Description("Displays command help.")]
        public async Task DefaultHelpAsync(CommandContext ctx, [Description("Command to provide help for.")] params string[] command)
        {
            var toplevel = TopLevelCommands.Values.Distinct();
            var helpbuilder = Activator.CreateInstance(HelpFormatterType) as IHelpFormatter;
            
            if (command != null && command.Any())
            {
                Command cmd = null;
                var searchIn = toplevel;
                foreach (var c in command)
                {
                    if (searchIn == null)
                    {
                        cmd = null;
                        break;
                    }

                    if (Config.CaseSensitive)
                    {
                        cmd = searchIn.FirstOrDefault(xc => xc.Name == c || xc.Aliases != null && xc.Aliases.Contains(c));
                    }
                    else
                    {
                        cmd = searchIn.FirstOrDefault(xc => xc.Name.ToLowerInvariant() == c.ToLowerInvariant() || xc.Aliases != null && xc.Aliases.Select(xs => xs.ToLowerInvariant()).Contains(c.ToLowerInvariant()));
                    }

                    if (cmd == null)
                    {
                        break;
                    }

                    var cfl = await cmd.RunChecksAsync(ctx, true);
                    var checkBaseAttributes = cfl as CheckBaseAttribute[] ?? cfl.ToArray();
                    if (checkBaseAttributes.Any())
                    {
                        throw new ChecksFailedException(cmd, ctx, checkBaseAttributes);
                    }

                    if (cmd is CommandGroup)
                    {
                        searchIn = (cmd as CommandGroup).Children;
                    }
                    else
                    {
                        searchIn = null;
                    }
                }

                if (cmd == null)
                {
                    throw new CommandNotFoundException(string.Join(" ", command));
                }

                if (helpbuilder != null)
                {
                    helpbuilder.WithCommandName(cmd.Name).WithDescription(cmd.Description);

                    if (cmd is CommandGroup g && g.Callable != null)
                    {
                        helpbuilder.WithGroupExecutable();
                    }

                    if (cmd.Aliases != null && cmd.Aliases.Any())
                    {
                        helpbuilder.WithAliases(cmd.Aliases.OrderBy(xs => xs));
                    }

                    if (cmd.Arguments != null && cmd.Arguments.Any())
                    {
                        helpbuilder.WithArguments(cmd.Arguments);
                    }

                    if (cmd is CommandGroup gx)
                    {
                        var sxs = gx.Children.Where(xc => !xc.IsHidden);
                        var scs = new List<Command>();
                        foreach (var sc in sxs)
                        {
                            if (sc.ExecutionChecks == null || !sc.ExecutionChecks.Any())
                            {
                                scs.Add(sc);
                                continue;
                            }

                            var cfl = await sc.RunChecksAsync(ctx, true);
                            if (!cfl.Any())
                            {
                                scs.Add(sc);
                            }
                        }

                        if (scs.Any())
                        {
                            helpbuilder.WithSubcommands(scs.OrderBy(xc => xc.Name));
                        }
                    }
                }
            }
            else
            {
                var sxs = toplevel.Where(xc => !xc.IsHidden);
                var scs = new List<Command>();
                foreach (var sc in sxs)
                {
                    if (sc.ExecutionChecks == null || !sc.ExecutionChecks.Any())
                    { 
                        scs.Add(sc);
                        continue;
                    }

                    var cfl = await sc.RunChecksAsync(ctx, true);
                    if (!cfl.Any())
                    {
                        scs.Add(sc);
                    }
                }

                if (scs.Any())
                {
                    helpbuilder?.WithSubcommands(scs.OrderBy(xc => xc.Name));
                }
            }

            if (helpbuilder != null)
            {
                var hmsg = helpbuilder.Build();
                await ctx.RespondAsync(hmsg.Content, embed: hmsg.Embed);
            }
        }
        #endregion

        #region Sudo
        /// <summary>
        /// Creates a fake message and executes a command using said message as context. Note that any command that looks the message up might throw.
        /// </summary>
        /// <param name="user">User to execute as.</param>
        /// <param name="channel">Channel to execute in.</param>
        /// <param name="message">Contents of the fake message.</param>
        /// <returns></returns>
        public async Task SudoAsync(DiscordUser user, DiscordChannel channel, string message)
        {
            var eph = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var dtn = DateTimeOffset.UtcNow;
            var ts = (ulong)(dtn - eph).TotalMilliseconds;

            // create fake message
            var msg = new DiscordMessage
            {
                Discord = Client,
                Author = user,
                ChannelId = channel.Id,
                Content = message,
                Id = ts << 22,
                Pinned = false,
                MentionEveryone = message.Contains("@everyone"),
                IsTts = false,
                _attachments = new List<DiscordAttachment>(),
                _embeds = new List<DiscordEmbed>(),
                TimestampRaw = dtn.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                _reactions = new List<DiscordReaction>()
            };

            var mentionedUsers = new List<DiscordUser>();
            var mentionedRoles = msg.Channel.Guild != null ? new List<DiscordRole>() : null;
            var mentionedChannels = msg.Channel.Guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(msg.Content))
            {
                if (msg.Channel.Guild != null)
                {
                    mentionedUsers = Utilities.GetUserMentions(msg).Select(xid => msg.Channel.Guild._members.FirstOrDefault(xm => xm.Id == xid)).Cast<DiscordUser>().ToList();
                    mentionedRoles = Utilities.GetRoleMentions(msg).Select(xid => msg.Channel.Guild._roles.FirstOrDefault(xr => xr.Id == xid)).ToList();
                    mentionedChannels = Utilities.GetChannelMentions(msg).Select(xid => msg.Channel.Guild._channels.FirstOrDefault(xc => xc.Id == xid)).ToList();
                }
                else
                {
                    mentionedUsers = Utilities.GetUserMentions(msg).Select(xid => Client.InternalGetCachedUser(xid)).ToList();
                }
            }

            msg._mentionedUsers = mentionedUsers;
            msg._mentionedRoles = mentionedRoles;
            msg._mentionedChannels = mentionedChannels;

            await HandleCommandsAsync(new MessageCreateEventArgs(Client) { Message = msg });
        }
        #endregion
    }
}
