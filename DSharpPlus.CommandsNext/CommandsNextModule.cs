using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// This is the class which handles command registration, management, and execution. 
    /// </summary>
    public class CommandsNextModule : IModule
    {
        #region Events
        /// <summary>
        /// Triggered whenever a command executes successfully.
        /// </summary>
        public event AsyncEventHandler<CommandExecutedEventArgs> CommandExecuted
        {
            add { this._executed.Register(value); }
            remove { this._executed.Unregister(value); }
        }
        private AsyncEvent<CommandExecutedEventArgs> _executed;

        /// <summary>
        /// Triggered whenever a command throws an exception during execution.
        /// </summary>
        public event AsyncEventHandler<CommandErrorEventArgs> CommandErrored
        {
            add { this._error.Register(value); }
            remove { this._error.Unregister(value); }
        }
        private AsyncEvent<CommandErrorEventArgs> _error;

        private async Task OnCommandExecuted(CommandExecutedEventArgs e) =>
            await this._executed.InvokeAsync(e).ConfigureAwait(false);

        private async Task OnCommandErrored(CommandErrorEventArgs e) =>
            await this._error.InvokeAsync(e).ConfigureAwait(false);
        #endregion

        private CommandsNextConfiguration Config { get; set; }
        private const string GROUP_COMMAND_METHOD_NAME = "ExecuteGroupAsync";

        public CommandsNextModule(CommandsNextConfiguration cfg)
        {
            this.Config = cfg;
            this.TopLevelCommands = new Dictionary<string, Command>();
            this._registered_commands_lazy = new Lazy<IReadOnlyDictionary<string, Command>>(() => new ReadOnlyDictionary<string, Command>(this.TopLevelCommands));
        }

        #region DiscordClient Registration
        /// <summary>
        /// Gets the instance of <see cref="DiscordClient"/> for which this module is registered.
        /// </summary>
        public DiscordClient Client { get { return this._client; } }
        private DiscordClient _client;

        /// <summary>
        /// DO NOT USE THIS MANUALLY.
        /// </summary>
        /// <param name="client">DO NOT USE THIS MANUALLY.</param>
        /// <exception cref="InvalidOperationException"/>
        public void Setup(DiscordClient client)
        {
            if (this._client != null)
                throw new InvalidOperationException("What did I tell you?");

            this._client = client;

            this._executed = new AsyncEvent<CommandExecutedEventArgs>(this.Client.EventErrorHandler, "COMMAND_EXECUTED");
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

            var cnt = e.Message.Content;
            var cmi = cnt.IndexOf(' ', mpos);
            var cms = cmi != -1 ? cnt.Substring(mpos, cmi - mpos) : cnt.Substring(mpos);
            var rrg = cmi != -1 ? cnt.Substring(cmi + 1) : "";
            //var arg = CommandsNextUtilities.SplitArguments(rrg);

            var cmd = this.TopLevelCommands.ContainsKey(cms) ? this.TopLevelCommands[cms] : null;
            if (cmd == null && !this.Config.CaseSensitive)
                cmd = this.TopLevelCommands.FirstOrDefault(xkvp => xkvp.Key.ToLower() == cms.ToLower()).Value;

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
                await this._error.InvokeAsync(new CommandErrorEventArgs { Context = ctx, Exception = new CommandNotFoundException("Specified command was not found.", cms) });
                return;
            }

#pragma warning disable 4014
            Task.Run(async () =>
            {
                try
                {
                    var fchecks = new List<CheckBaseAttribute>();
                    if (cmd.ExecutionChecks != null && cmd.ExecutionChecks.Any())
                        foreach (var ec in cmd.ExecutionChecks)
                            if (!(await ec.CanExecute(ctx)))
                                fchecks.Add(ec);
                    if (fchecks.Any())
                        throw new ChecksFailedException("One or more pre-execution checks failed.", cmd, ctx, fchecks);

                    var res = await cmd.ExecuteAsync(ctx);
                    
                    if (res.IsSuccessful)
                        await this._executed.InvokeAsync(new CommandExecutedEventArgs { Context = res.Context });
                    else
                        await this._error.InvokeAsync(new CommandErrorEventArgs { Context = res.Context, Exception = res.Exception });
                }
                catch (Exception ex)
                {
                    await this._error.InvokeAsync(new CommandErrorEventArgs { Context = ctx, Exception = ex });
                }
            });
#pragma warning restore 4014
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
                return xti.IsClass && xti.IsPublic && !xti.IsNested && !xti.IsAbstract;
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

            var ti = t.GetTypeInfo();
            if (!ti.IsClass || ti.IsAbstract)
                throw new ArgumentNullException("Type must be a class, which cannot be abstract or static.", nameof(t));

            RegisterCommands(t, CreateInstance(t), null, out var tres, out var tcmds);
            
            if (tres != null)
                this.AddToCommandDictionary(tres);

            if (tcmds != null)
                foreach (var xc in tcmds)
                    this.AddToCommandDictionary(xc);
        }

        private void RegisterCommands(Type t, object inst, CommandGroup currentparent, out CommandGroup result, out IReadOnlyCollection<Command> commands)
        {
            var ti = t.GetTypeInfo();

            // check if we are anything
            var mdl_attrs = ti.GetCustomAttributes();
            var is_mdl = false;
            var mdl_name = "";
            var mdl_aliases = (IReadOnlyCollection<string>)null;
            var mdl_hidden = false;
            var mdl_desc = "";
            var mdl_chks = new List<CheckBaseAttribute>();
            var mdl_cbl = (Delegate)null;
            var mdl_args = (IReadOnlyList<CommandArgument>)null;
            var mdl = (CommandGroup)null;
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

                cmds.Add(cmd);
            }

            // candidate types
            var ts = ti.DeclaredNestedTypes
                .Where(xt => xt.DeclaredConstructors.Any(xc => !xc.GetParameters().Any() || xc.IsPublic));
            foreach (var xt in ts)
            {
                this.RegisterCommands(xt.AsType(), this.CreateInstance(xt.AsType()), mdl, out var tmdl, out var tcmds);

                if (tmdl != null)
                    cmds.Add(tmdl);
                cmds.AddRange(tcmds);
            }

            commands = new ReadOnlyCollection<Command>(cmds);
            if (mdl != null)
                mdl.Children = commands;
            result = mdl;
        }

        private void MakeCallable(MethodInfo mi, object inst, out Delegate cbl, out IReadOnlyList<CommandArgument> args)
        {
            if (mi == null)
                throw new MissingMethodException("Specified method does not exist.");

            if (mi.IsStatic || !mi.IsPublic)
                throw new InvalidOperationException("Specified method is invalid, static, or not public.");

            var ps = mi.GetParameters();
            if (!ps.Any() || ps.First().ParameterType != typeof(CommandContext) || mi.ReturnType != typeof(Task))
                throw new InvalidOperationException("Specified method has an invalid signature.");

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
                throw new CommandExistsException("Given command name is already registered.", cmd.Name);

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
            var embed = new DiscordEmbed()
            {
                Color = 0x007FFF,
                Title = "Help",
                Fields = new List<DiscordEmbedField>()
            };

            if (command != null && command.Any())
            {
                var cmd = (Command)null;
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
                        cmd = search_in.FirstOrDefault(xc => xc.Name.ToLower() == c.ToLower() || (xc.Aliases != null && xc.Aliases.Select(xs => xs.ToLower()).Contains(c.ToLower())));

                    if (cmd == null)
                        break;
                    
                    var cfl = new List<CheckBaseAttribute>();
                    foreach (var ec in cmd.ExecutionChecks)
                        if (!(await ec.CanExecute(ctx)))
                            cfl.Add(ec);
                    if (cfl.Any())
                        throw new ChecksFailedException("You cannot access that command!", cmd, ctx, cfl);

                    if (cmd is CommandGroup)
                        search_in = (cmd as CommandGroup).Children;
                    else
                        search_in = null;
                }

                if (cmd == null)
                    throw new CommandNotFoundException("Specified command was not found!", string.Join(" ", command));

                embed.Description = string.Concat("`", cmd.QualifiedName, "`: ", string.IsNullOrWhiteSpace(cmd.Description) ? "No description provided." : cmd.Description);

                if (cmd is CommandGroup g && g.Callable != null)
                    embed.Description = string.Concat(embed.Description, "\n\nThis group can be executed as a standalone command.");

                if (cmd.Aliases != null && cmd.Aliases.Any())
                    embed.Fields.Add(new DiscordEmbedField
                    {
                        Inline = false,
                        Name = "Aliases",
                        Value = string.Join(", ", cmd.Aliases.Select(xs => string.Concat("`", xs, "`")))
                    });

                if (cmd.Arguments != null && cmd.Arguments.Any())
                {
                    var args = string.Empty;
                    var sb = new StringBuilder();

                    foreach (var arg in cmd.Arguments)
                    {
                        if (arg.IsOptional || arg.IsCatchAll)
                            sb.Append("`[");
                        else
                            sb.Append("`<");

                        sb.Append(arg.Name);

                        if (arg.IsCatchAll)
                            sb.Append("...");

                        if (arg.IsOptional || arg.IsCatchAll)
                            sb.Append("]: ");
                        else
                            sb.Append(">: ");

                        sb.Append(arg.Type.ToUserFriendlyName()).Append("`: ");

                        sb.Append(string.IsNullOrWhiteSpace(arg.Description) ? "No description provided." : arg.Description);

                        if (arg.IsOptional)
                            sb.Append(" Default value: ").Append(arg.DefaultValue);

                        sb.AppendLine();
                    }
                    args = sb.ToString();

                    embed.Fields.Add(new DiscordEmbedField
                    {
                        Inline = false,
                        Name = "Arguments",
                        Value = args
                    });
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

                        var cfl = new List<CheckBaseAttribute>();
                        foreach (var ec in sc.ExecutionChecks)
                            if (!(await ec.CanExecute(ctx)))
                                cfl.Add(ec);
                        if (!cfl.Any())
                            scs.Add(sc);
                    }

                    if (scs.Any())
                        embed.Fields.Add(new DiscordEmbedField
                        {
                            Inline = false,
                            Name = "Subcommands",
                            Value = string.Join(", ", scs.OrderBy(xc => xc.QualifiedName).Select(xc => string.Concat("`", xc.QualifiedName, "`")))
                        });
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
                    
                    var cfl = new List<CheckBaseAttribute>();
                    foreach (var ec in sc.ExecutionChecks)
                        if (!(await ec.CanExecute(ctx)))
                            cfl.Add(ec);
                    if (!cfl.Any())
                        scs.Add(sc);
                }

                embed.Description = "Listing all top-level commands and groups. Specify a command to see more information.";
                if (scs.Any())
                    embed.Fields.Add(new DiscordEmbedField
                    {
                        Inline = false,
                        Name = "Commands",
                        Value = string.Join(", ", scs.OrderBy(xc => xc.QualifiedName).Select(xc => string.Concat("`", xc.QualifiedName, "`")))
                    });
            }

            await ctx.RespondAsync("", embed: embed);
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
            var ts = (ulong)(eph - dtn).TotalMilliseconds;

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
                    mentioned_users = Utils.GetUserMentions(msg).Select(xid => msg.Channel.Guild._members.FirstOrDefault(xm => xm.Id == xid)).Cast<DiscordUser>().ToList();
                    mentioned_roles = Utils.GetRoleMentions(msg).Select(xid => msg.Channel.Guild._roles.FirstOrDefault(xr => xr.Id == xid)).ToList();
                    mentioned_channels = Utils.GetChannelMentions(msg).Select(xid => msg.Channel.Guild._channels.FirstOrDefault(xc => xc.Id == xid)).ToList();
                }
                else
                {
                    mentioned_users = Utils.GetUserMentions(msg).Select(xid => this.Client.InternalGetCachedUser(xid)).ToList();
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
