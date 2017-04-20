using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;

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

        /// <summary>
        /// Triggered whenever a new command is registered.
        /// </summary>
        public event AsyncEventHandler<CommandEventArgs> CommandRegistered
        {
            add { this._registered.Register(value); }
            remove { this._registered.Unregister(value); }
        }
        private AsyncEvent<CommandEventArgs> _registered = new AsyncEvent<CommandEventArgs>();

        private async Task OnCommandExecuted(CommandExecutedEventArgs e) =>
            await this._executed.InvokeAsync(e).ConfigureAwait(false);

        private async Task OnCommandErrored(CommandErrorEventArgs e) =>
            await this._error.InvokeAsync(e).ConfigureAwait(false);

        private async Task OnCommandRegistered(CommandEventArgs e) =>
            await this._registered.InvokeAsync(e).ConfigureAwait(false);
        #endregion

        private CommandsNextConfig Config { get; set; }

        public CommandsNextModule(CommandsNextConfig cfg)
        {
            this.Config = cfg;
        }

        #region DiscordClient Registration
        /// <summary>
        /// Gets the instance of <see cref="DiscordClient"/> for which this module is registered.
        /// </summary>
        public DiscordClient Client { get { return this.Client; } }
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
            this.Client.AddModule(this);
            this.Client.MessageCreated += this.HandleCommands;
        }
        #endregion

        #region Command Handler
        private async Task HandleCommands(MessageCreateEventArgs e)
        {
            // Let the bot do its things
            await Task.Yield();


        }
        #endregion

        #region Command Registration
        public void RegisterCommands<T>() where T : new()
        {
            var t = typeof(T);
            RegisterCommands(t, new T(), null);
        }

        private void RegisterCommands(Type t, object inst, CommandGroup currentparent)
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
            var mdl_args = (IReadOnlyCollection<CommandArgument>)null;
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
            
            
            // candidate types
        }

        private void MakeCallable(MethodInfo mi, object inst, out Delegate cbl, out IReadOnlyCollection<CommandArgument> args)
        {
            if (mi == null || mi.IsStatic || !mi.IsPublic)
                throw new InvalidOperationException("Specified method is invalid, static, or not public.");

            var ps = mi.GetParameters();
            if (ps.First().ParameterType != typeof(CommandContext) || mi.ReturnType != typeof(Task))
                throw new InvalidOperationException("Specified method has an invalid signature.");

            var ei = Expression.Constant(inst);

            var ea = new ParameterExpression[ps.Length];
            ea[0] = Expression.Parameter(typeof(CommandContext), "ctx");

            var i = 1;
            var ps1 = ps.Skip(1);
            var argsl = new List<CommandArgument>();
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

        private void MakeCallableModule(TypeInfo ti, object inst, out Delegate cbl, out IReadOnlyCollection<CommandArgument> args)
        {
            this.MakeCallable(ti.GetDeclaredMethod("ModuleCommand"), inst, out cbl, out args);
        }

        public void RegisterCommand(Delegate cmd)
        {
            var mi = cmd.GetMethodInfo();
            this.MakeCallable(mi, null, out var cbl, out var args);

            // do other things
        }
        #endregion
    }
}
