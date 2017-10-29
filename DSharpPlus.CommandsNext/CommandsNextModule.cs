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
    public class CommandsNextModule : BaseModule
    {
        #region Events
        /// <summary>
        /// Triggered whenever a command executes successfully.
        /// </summary>
        public event AsyncEventHandler<CommandExecutionEventArgs> CommandExecuted
        {
            add { this._executed.Register(value); }
            remove { this._executed.Unregister(value); }
        }
        private AsyncEvent<CommandExecutionEventArgs> _executed;

        /// <summary>
        /// Triggered whenever a command throws an exception during execution.
        /// </summary>
        public event AsyncEventHandler<CommandErrorEventArgs> CommandErrored
        {
            add { this._error.Register(value); }
            remove { this._error.Unregister(value); }
        }
        private AsyncEvent<CommandErrorEventArgs> _error;

        private async Task OnCommandExecuted(CommandExecutionEventArgs e) =>
            await this._executed.InvokeAsync(e).ConfigureAwait(false);

        private async Task OnCommandErrored(CommandErrorEventArgs e) =>
            await this._error.InvokeAsync(e).ConfigureAwait(false);
        #endregion

        private CommandsNextConfiguration Config { get; }
        private Type HelpFormatterType { get; set; } = typeof(DefaultHelpFormatter);
        private const string GROUP_COMMAND_METHOD_NAME = "ExecuteGroupAsync";

        /// <summary>
        /// Gets the dependency collection this CommandsNext module was configured with.
        /// </summary>
        public DependencyCollection Dependencies => this.Config.Dependencies;

        internal CommandsNextModule(CommandsNextConfiguration cfg)
        {
            this.Config = cfg;
            this.TopLevelCommands = new Dictionary<string, Command>();
            this._registered_commands_lazy = new Lazy<IReadOnlyDictionary<string, Command>>(() => new ReadOnlyDictionary<string, Command>(this.TopLevelCommands));
        }

        /// <summary>
        /// Sets the help formatter to use with the default help command.
        /// </summary>
        /// <typeparam name="T">Type of the formatter to use.</typeparam>
        public void SetHelpFormatter<T>() where T : class, IHelpFormatter, new()
        {
            this.HelpFormatterType = typeof(T);
        }

        #region DiscordClient Registration
        /// <summary>
        /// DO NOT USE THIS MANUALLY.
        /// </summary>
        /// <param name="client">DO NOT USE THIS MANUALLY.</param>
        /// <exception cref="InvalidOperationException"/>
        protected internal override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = client;

            this._executed = new AsyncEvent<CommandExecutionEventArgs>(this.Client.EventErrorHandler, "COMMAND_EXECUTED");
            this._error = new AsyncEvent<CommandErrorEventArgs>(this.Client.EventErrorHandler, "COMMAND_ERRORED");

            this.Client.MessageCreated += this.HandleCommandsAsync;

            if (this.Config.EnableDefaultHelp)
            {
                var dlg = new Func<CommandContext, string[], Task>(this.DefaultHelpAsync);
                var mi = dlg.GetMethodInfo();
                this.MakeCallable(mi, dlg.Target, out var cbl, out var args);

                var attrs = mi.GetCustomAttributes();
                if (!attrs.Any(xa => xa.GetType() == typeof(CommandAttribute)))
                    return;

                var cmd = new Command();

                var cbas = new List<CheckBaseAttribute>();
                foreach (var xa in attrs)
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

                        case HiddenAttribute h:
                            cmd.IsHidden = true;
                            break;
                    }
                }

                if (this.Config.DefaultHelpChecks != null && this.Config.DefaultHelpChecks.Any())
                    cbas.AddRange(this.Config.DefaultHelpChecks);

                cmd.ExecutionChecks = new ReadOnlyCollection<CheckBaseAttribute>(cbas);
                cmd.Arguments = args;
                cmd.Callable = cbl;

                this.AddToCommandDictionary(cmd);
            }
        }
        #endregion

        #region Command Handler
        public async Task HandleCommandsAsync(MessageCreateEventArgs e)
        {
            // Let the bot do its things
            await Task.Yield();

            if (e.Author.IsBot) // bad bot
                return;

            if (!this.Config.EnableDms && e.Channel.IsPrivate)
                return;

            if (this.Config.SelfBot && e.Author.Id != this.Client.CurrentUser.Id)
                return;

            var mpos = -1;
            if (this.Config.EnableMentionPrefix)
                mpos = e.Message.GetMentionPrefixLength(this.Client.CurrentUser);

            if (mpos == -1 && !string.IsNullOrWhiteSpace(this.Config.StringPrefix))
                mpos = e.Message.GetStringPrefixLength(this.Config.StringPrefix);

            if (mpos == -1 && this.Config.CustomPrefixPredicate != null)
                mpos = await this.Config.CustomPrefixPredicate(e.Message);

            if (mpos == -1)
                return;

            var cnt = e.Message.Content.Substring(mpos);
            var cms = CommandsNextUtilities.ExtractNextArgument(cnt, out var rrg);
            //var cmi = cnt.IndexOf(' ', mpos);
            //var cms = cmi != -1 ? cnt.Substring(mpos, cmi - mpos) : cnt.Substring(mpos);
            //var rrg = cmi != -1 ? cnt.Substring(cmi + 1) : "";
            //var arg = CommandsNextUtilities.SplitArguments(rrg);

            var cmd = this.TopLevelCommands.ContainsKey(cms) ? this.TopLevelCommands[cms] : null;
            if (cmd == null && !this.Config.CaseSensitive)
                cmd = this.TopLevelCommands.FirstOrDefault(xkvp => xkvp.Key.ToLowerInvariant() == cms.ToLowerInvariant()).Value;

            var ctx = new CommandContext
            {
                Client = this.Client,
                Command = cmd,
                Message = e.Message,
                //RawArguments = new ReadOnlyCollection<string>(arg.ToList()),
                Config = this.Config,
                RawArgumentString = rrg,
                CommandsNext = this,
                Dependencies = this.Config.Dependencies
            };

            if (cmd == null)
            {
                await this._error.InvokeAsync(new CommandErrorEventArgs { Context = ctx, Exception = new CommandNotFoundException(cms) });
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var fchecks = await cmd.RunChecksAsync(ctx, false);
                    if (fchecks.Any())
                        throw new ChecksFailedException(cmd, ctx, fchecks);

                    var res = await cmd.ExecuteAsync(ctx);
                    
                    if (res.IsSuccessful)
                        await this._executed.InvokeAsync(new CommandExecutionEventArgs { Context = res.Context });
                    else
                        await this._error.InvokeAsync(new CommandErrorEventArgs { Context = res.Context, Exception = res.Exception });
                }
                catch (Exception ex)
                {
                    await this._error.InvokeAsync(new CommandErrorEventArgs { Context = ctx, Exception = ex });
                }
            });
        }
        #endregion

        #region Command Registration
        private Dictionary<string, Command> TopLevelCommands { get; set; }
        private Lazy<IReadOnlyDictionary<string, Command>> _registered_commands_lazy;
        public IReadOnlyDictionary<string, Command> RegisteredCommands => this._registered_commands_lazy.Value;

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
                    return false;

                return xti.DeclaredMethods.Any(xmi => xmi.IsCommandCandidate(out _));
            });
            foreach (var xt in types)
                this.RegisterCommands(xt);
        }

        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <typeparam name="T">Class which holds commands to register.</typeparam>
        public void RegisterCommands<T>() where T : class
        {
            var t = typeof(T);
            this.RegisterCommands(t);
        }

        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <param name="t">Type of the class which holds commands to register.</param>
        public void RegisterCommands(Type t)
        {
            if (t == null)
                throw new ArgumentNullException("Type cannot be null.", nameof(t));

            if (!t.IsModuleCandidateType())
                throw new ArgumentNullException("Type must be a class, which cannot be abstract or static.", nameof(t));

            RegisterCommands(t, CreateInstance(t), null, out var tres, out var tcmds);
            
            if (tres != null)
                this.AddToCommandDictionary(tres);

            if (tcmds != null)
                foreach (var xc in tcmds)
                    this.AddToCommandDictionary(xc);
        }

        private void RegisterCommands(Type t, object inst, CommandGroup currentparent, out CommandGroup result, out IReadOnlyList<Command> commands)
        {
            var ti = t.GetTypeInfo();

            // check if we are anything
            var mdl_attrs = ti.GetCustomAttributes();
            var is_mdl = false;
            var mdl_name = "";
            IReadOnlyList<string> mdl_aliases = null;
            var mdl_hidden = false;
            var mdl_desc = "";
            var mdl_chks = new List<CheckBaseAttribute>();
            Delegate mdl_cbl = null;
            IReadOnlyList<CommandArgument> mdl_args = null;
            CommandGroup mdl = null;
            foreach (var xa in mdl_attrs)
            {
                switch (xa)
                {
                    case GroupAttribute g:
                        is_mdl = true;
                        mdl_name = g.Name;
                        if (g.CanInvokeWithoutSubcommand)
                            this.MakeCallableModule(ti, inst, out mdl_cbl, out mdl_args);
                        break;

                    case AliasesAttribute a:
                        mdl_aliases = a.Aliases;
                        break;

                    case HiddenAttribute h:
                        mdl_hidden = true;
                        break;

                    case DescriptionAttribute d:
                        mdl_desc = d.Description;
                        break;

                    case CheckBaseAttribute c:
                        mdl_chks.Add(c);
                        break;
                }
            }

            if (is_mdl)
                mdl = new CommandGroup
                {
                    Name = mdl_name,
                    Aliases = mdl_aliases,
                    Description = mdl_desc,
                    ExecutionChecks = new ReadOnlyCollection<CheckBaseAttribute>(mdl_chks),
                    IsHidden = mdl_hidden,
                    Parent = currentparent,
                    Callable = mdl_cbl,
                    Arguments = mdl_args,
                    Children = null
                };

            // candidate methods
            var ms = ti.DeclaredMethods
                .Where(xm => xm.IsPublic && !xm.IsStatic && xm.Name != GROUP_COMMAND_METHOD_NAME);
            var cmds = new List<Command>();
            var uniq = new HashSet<string>();
            foreach (var m in ms)
            {
                if (m.ReturnType != typeof(Task))
                    continue;

                var ps = m.GetParameters();
                if (!ps.Any() || ps.First().ParameterType != typeof(CommandContext))
                    continue;

                var attrs = m.GetCustomAttributes();
                if (!attrs.Any(xa => xa.GetType() == typeof(CommandAttribute)))
                    continue;

                var cmd = new Command();

                var cbas = new List<CheckBaseAttribute>();
                if (!is_mdl && mdl_chks.Any())
                    cbas.AddRange(mdl_chks);

                foreach (var xa in attrs)
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

                        case HiddenAttribute h:
                            cmd.IsHidden = true;
                            break;
                    }
                }
                cmd.ExecutionChecks = new ReadOnlyCollection<CheckBaseAttribute>(cbas);
                cmd.Parent = mdl;
                MakeCallable(m, inst, out var cbl, out var args);
                cmd.Callable = cbl;
                cmd.Arguments = args;
                
                if (uniq.Contains(cmd.Name))
                    throw new DuplicateCommandException(cmd.QualifiedName);
                uniq.Add(cmd.Name);

                if (cmd.Aliases != null && cmd.Aliases.Any())
                {
                    foreach (var xa in cmd.Aliases)
                    {
                        if (uniq.Contains(xa))
                            throw new DuplicateCommandException(cmd.QualifiedName);

                        uniq.Add(xa);
                    }
                }

                cmds.Add(cmd);
            }

            // candidate types
            var ts = ti.DeclaredNestedTypes
                .Where(xt => xt.IsModuleCandidateType() && xt.DeclaredConstructors.Any(xc => xc.IsPublic));
            foreach (var xt in ts)
            {
                this.RegisterCommands(xt.AsType(), this.CreateInstance(xt.AsType()), mdl, out var tmdl, out var tcmds);

                if (tmdl != null)
                    cmds.Add(tmdl);
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
                throw new MissingMethodException("Specified method is not suitable for a command.");

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

                        case RemainingTextAttribute r:
                            ca.IsCatchAll = true;
                            break;

                        case ParamArrayAttribute p:
                            ca.IsCatchAll = true;
                            ca.Type = xp.ParameterType.GetElementType();
                            ca._is_array = true;
                            break;
                    }
                }

                if (i > 1 && !ca.IsOptional && !ca.IsCatchAll && argsl[i - 2].IsOptional)
                    throw new InvalidOperationException("Non-optional argument cannot appear after an optional one");

                argsl.Add(ca);
                ea[i++] = Expression.Parameter(xp.ParameterType, xp.Name);
            }

            var ec = Expression.Call(ei, mi, ea);
            var el = Expression.Lambda(ec, ea);

            cbl = el.Compile();
            args = new ReadOnlyCollection<CommandArgument>(argsl);
        }

        private void MakeCallableModule(TypeInfo ti, object inst, out Delegate cbl, out IReadOnlyList<CommandArgument> args)
        {
            var mtd = ti.GetDeclaredMethod(GROUP_COMMAND_METHOD_NAME);
            if (mtd == null)
                throw new MissingMethodException($"A group marked with CanExecute must have a method named {GROUP_COMMAND_METHOD_NAME}.");

            this.MakeCallable(mtd, inst, out cbl, out args);
        }

        private object CreateInstance(Type t)
        {
            var ti = t.GetTypeInfo();
            var cs = ti.DeclaredConstructors
                .Where(xci => xci.IsPublic)
                .ToArray();

            if (cs.Length != 1)
                throw new ArgumentException("Specified type does not contain a public constructor or contains more than one public constructor.");

            var constr = cs[0];
            var prms = constr.GetParameters();
            var args = new object[prms.Length];
            var deps = this.Config.Dependencies;

            if (prms.Length != 0 && deps == null)
                throw new InvalidOperationException("Dependency collection needs to be specified for parametered constructors.");

            if (prms.Length != 0)
                for (var i = 0; i < args.Length; i++)
                    args[i] = deps.GetDependency(prms[i].ParameterType);

            return Activator.CreateInstance(t, args);
        }

        private void AddToCommandDictionary(Command cmd)
        {
            if (cmd.Parent != null)
                return;

            if (this.TopLevelCommands.ContainsKey(cmd.Name) || (cmd.Aliases != null && cmd.Aliases.Any(xs => this.TopLevelCommands.ContainsKey(xs))))
                throw new DuplicateCommandException(cmd.QualifiedName);

            this.TopLevelCommands[cmd.Name] = cmd;
            if (cmd.Aliases != null)
                foreach (var xs in cmd.Aliases)
                    this.TopLevelCommands[xs] = cmd;
        }
        #endregion

        #region Default Help
        [Command("help"), Description("Displays command help.")]
        public async Task DefaultHelpAsync(CommandContext ctx, [Description("Command to provide help for.")] params string[] command)
        {
            var toplevel = this.TopLevelCommands.Values.Distinct();
            var helpbuilder = Activator.CreateInstance(this.HelpFormatterType) as IHelpFormatter;
            
            if (command != null && command.Any())
            {
                Command cmd = null;
                var search_in = toplevel;
                foreach (var c in command)
                {
                    if (search_in == null)
                    {
                        cmd = null;
                        break;
                    }

                    if (this.Config.CaseSensitive)
                        cmd = search_in.FirstOrDefault(xc => xc.Name == c || (xc.Aliases != null && xc.Aliases.Contains(c)));
                    else
                        cmd = search_in.FirstOrDefault(xc => xc.Name.ToLowerInvariant() == c.ToLowerInvariant() || (xc.Aliases != null && xc.Aliases.Select(xs => xs.ToLowerInvariant()).Contains(c.ToLowerInvariant())));

                    if (cmd == null)
                        break;

                    var cfl = await cmd.RunChecksAsync(ctx, true);
                    if (cfl.Any())
                        throw new ChecksFailedException(cmd, ctx, cfl);

                    if (cmd is CommandGroup)
                        search_in = (cmd as CommandGroup).Children;
                    else
                        search_in = null;
                }

                if (cmd == null)
                    throw new CommandNotFoundException(string.Join(" ", command));

                helpbuilder.WithCommandName(cmd.Name).WithDescription(cmd.Description);

                if (cmd is CommandGroup g && g.Callable != null)
                    helpbuilder.WithGroupExecutable();

                if (cmd.Aliases != null && cmd.Aliases.Any())
                    helpbuilder.WithAliases(cmd.Aliases.OrderBy(xs => xs));

                if (cmd.Arguments != null && cmd.Arguments.Any())
                    helpbuilder.WithArguments(cmd.Arguments);

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
                            scs.Add(sc);
                    }

                    if (scs.Any())
                        helpbuilder.WithSubcommands(scs.OrderBy(xc => xc.Name));
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
                        scs.Add(sc);
                }

                if (scs.Any())
                    helpbuilder.WithSubcommands(scs.OrderBy(xc => xc.Name));
            }

            var hmsg = helpbuilder.Build();
            await ctx.RespondAsync(hmsg.Content, embed: hmsg.Embed);
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
                Discord = this.Client,
                Author = user,
                ChannelId = channel.Id,
                Content = message,
                Id = ts << 22,
                Pinned = false,
                MentionEveryone = message.Contains("@everyone"),
                IsTTS = false,
                _attachments = new List<DiscordAttachment>(),
                _embeds = new List<DiscordEmbed>(),
                TimestampRaw = dtn.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                _reactions = new List<DiscordReaction>()
            };

            var mentioned_users = new List<DiscordUser>();
            var mentioned_roles = msg.Channel.Guild != null ? new List<DiscordRole>() : null;
            var mentioned_channels = msg.Channel.Guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(msg.Content))
            {
                if (msg.Channel.Guild != null)
                {
                    mentioned_users = Utilities.GetUserMentions(msg).Select(xid => msg.Channel.Guild._members.FirstOrDefault(xm => xm.Id == xid)).Cast<DiscordUser>().ToList();
                    mentioned_roles = Utilities.GetRoleMentions(msg).Select(xid => msg.Channel.Guild._roles.FirstOrDefault(xr => xr.Id == xid)).ToList();
                    mentioned_channels = Utilities.GetChannelMentions(msg).Select(xid => msg.Channel.Guild._channels.FirstOrDefault(xc => xc.Id == xid)).ToList();
                }
                else
                {
                    mentioned_users = Utilities.GetUserMentions(msg).Select(xid => this.Client.InternalGetCachedUser(xid)).ToList();
                }
            }

            msg._mentioned_users = mentioned_users;
            msg._mentioned_roles = mentioned_roles;
            msg._mentioned_channels = mentioned_channels;

            await this.HandleCommandsAsync(new MessageCreateEventArgs(this.Client) { Message = msg });
        }
        #endregion
    }
}
