using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        private AsyncEvent<CommandExecutedEventArgs> _executed = new AsyncEvent<CommandExecutedEventArgs>();

        /// <summary>
        /// Triggered whenever a command throws an exception during execution.
        /// </summary>
        public event AsyncEventHandler<CommandErrorEventArgs> CommandErrored
        {
            add { this._error.Register(value); }
            remove { this._error.Unregister(value); }
        }
        private AsyncEvent<CommandErrorEventArgs> _error = new AsyncEvent<CommandErrorEventArgs>();

        private async Task OnCommandExecuted(CommandExecutedEventArgs e) =>
            await this._executed.InvokeAsync(e).ConfigureAwait(false);

        private async Task OnCommandErrored(CommandErrorEventArgs e) =>
            await this._error.InvokeAsync(e).ConfigureAwait(false);
        #endregion

        private CommandsNextConfiguration Config { get; set; }

        public CommandsNextModule(CommandsNextConfiguration cfg)
        {
            this.Config = cfg;
            this.RegisteredCommands = new Dictionary<string, Command>();
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
            this.Client.MessageCreated += this.HandleCommands;
        }
        #endregion

        #region Command Handler
        private async Task HandleCommands(MessageCreateEventArgs e)
        {
            // Let the bot do its things
            await Task.Yield();

            var mpos = -1;
            if (!this.Config.EnableMentionPrefix || (mpos = e.Message.HasMentionPrefix(this.Client.Me)) == -1)
                mpos = e.Message.HasStringPrefix(this.Config.Prefix);

            if (mpos == -1)
                return;

            var cnt = e.Message.Content;
            var cmi = cnt.IndexOf(' ', mpos);
            var cms = cmi != -1 ? cnt.Substring(mpos, cmi - mpos) : cnt.Substring(mpos);
            var rrg = cmi != -1 ? cnt.Substring(cmi + 1) : "";
            var arg = CommandsNextUtilities.SplitArguments(rrg);

            var cmd = this.RegisteredCommandList.FirstOrDefault(xc => xc.Name == cms || (xc.Aliases != null && xc.Aliases.Contains(cms)));
            var ctx = new CommandContext
            {
                Client = this.Client,
                Command = cmd,
                Message = e.Message,
                RawArguments = new ReadOnlyCollection<string>(arg.ToList())
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
                    await cmd.Execute(ctx);
                    await this._executed.InvokeAsync(new CommandExecutedEventArgs { Context = ctx });
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
        private Dictionary<string, Command> RegisteredCommands { get; set; }
        public IEnumerable<Command> RegisteredCommandList => this.RegisteredCommands.Select(xkvp => xkvp.Value);

        public void RegisterCommands<T>() where T : new()
        {
            var t = typeof(T);
            RegisterCommands(t, new T(), null, out var tres, out var tcmds);

            // register
            if (tres != null)
                this.RegisteredCommands.Add(tres.QualifiedName, tres);
            foreach (var xc in tcmds)
            {
                this.RegisteredCommands.Add(xc.QualifiedName, xc);
            }
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
            var mdl_chks = new List<ConditionBaseAttribute>();
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

                    case ConditionBaseAttribute c:
                        mdl_chks.Add(c);
                        break;

                    case CanExecuteAttribute x:
                        this.MakeCallableModule(ti, inst, out mdl_cbl, out mdl_args);
                        break;
                }
            }

            if (is_mdl)
                mdl = new CommandGroup
                {
                    Name = mdl_name,
                    Aliases = mdl_aliases,
                    Description = mdl_desc,
                    ExecutionChecks = new ReadOnlyCollection<ConditionBaseAttribute>(mdl_chks),
                    IsHidden = mdl_hidden,
                    Parent = null,
                    Callable = mdl_cbl,
                    Arguments = mdl_args,
                    Children = null
                };

            // candidate methods
            var ms = ti.DeclaredMethods
                .Where(xm => xm.IsPublic && !xm.IsStatic && xm.Name != "ModuleCommand");
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

                var cbas = new List<ConditionBaseAttribute>();
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

                        case ConditionBaseAttribute p:
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
                cmd.ExecutionChecks = new ReadOnlyCollection<ConditionBaseAttribute>(cbas);
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
                this.RegisterCommands(xt.AsType(), Activator.CreateInstance(xt.AsType()), mdl, out var tmdl, out var tcmds);

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
            if (mi == null || mi.IsStatic || !mi.IsPublic)
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
                if (i > 1 && !ca.IsOptional && argsl[i - 2].IsOptional)
                    throw new InvalidOperationException("Non-optional argument cannot appear after an optional one");

                var attrs = xp.GetCustomAttributes();
                foreach (var xa in attrs)
                {
                    switch (xa)
                    {
                        case DescriptionAttribute d:
                            ca.Description = d.Description;
                            break;

                        case ParamArrayAttribute p:
                            ca.IsCatchAll = true;
                            ca.Type = xp.ParameterType.GetElementType();
                            break;
                    }
                }

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
            this.MakeCallable(ti.GetDeclaredMethod("ModuleCommand"), inst, out cbl, out args);
        }

        public void RegisterCommand(Delegate dlg)
        {
            var mi = dlg.GetMethodInfo();
            this.MakeCallable(mi, null, out var cbl, out var args);

            var attrs = mi.GetCustomAttributes();
            if (!attrs.Any(xa => xa.GetType() == typeof(CommandAttribute)))
                return;

            var cmd = new Command();

            var cbas = new List<ConditionBaseAttribute>();
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

                    case ConditionBaseAttribute p:
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
            cmd.ExecutionChecks = new ReadOnlyCollection<ConditionBaseAttribute>(cbas);

            this.RegisteredCommands.Add(cmd.QualifiedName, cmd);
        }
        #endregion
    }
}
