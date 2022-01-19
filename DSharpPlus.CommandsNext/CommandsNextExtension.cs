// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Builders;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Executors;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// This is the class which handles command registration, management, and execution. 
    /// </summary>
    public class CommandsNextExtension : BaseExtension, IDisposable
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
            => this.Config.Services;

        internal CommandsNextExtension(CommandsNextConfiguration cfg)
        {
            this.Config = new CommandsNextConfiguration(cfg);
            this.TopLevelCommands = new Dictionary<string, Command>();
            this._registeredCommandsLazy = new Lazy<IReadOnlyDictionary<string, Command>>(() => new ReadOnlyDictionary<string, Command>(this.TopLevelCommands));
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

            var ncvt = typeof(NullableConverter<>);
            var nt = typeof(Nullable<>);
            var cvts = this.ArgumentConverters.Keys.ToArray();
            foreach (var xt in cvts)
            {
                var xti = xt.GetTypeInfo();
                if (!xti.IsValueType)
                    continue;

                var xcvt = ncvt.MakeGenericType(xt);
                var xnt = nt.MakeGenericType(xt);
                if (this.ArgumentConverters.ContainsKey(xcvt))
                    continue;

                var xcv = Activator.CreateInstance(xcvt) as IArgumentConverter;
                this.ArgumentConverters[xnt] = xcv;
                this.UserFriendlyTypeNames[xnt] = this.UserFriendlyTypeNames[xt];
            }

            var t = typeof(CommandsNextExtension);
            var ms = t.GetTypeInfo().DeclaredMethods;
            var m = ms.FirstOrDefault(xm => xm.Name == "ConvertArgument" && xm.ContainsGenericParameters && !xm.IsStatic && xm.IsPublic);
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
            => this.Config.CommandExecutor.Dispose();

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

            this._executed = new AsyncEvent<CommandsNextExtension, CommandExecutionEventArgs>("COMMAND_EXECUTED", TimeSpan.Zero, this.Client.EventErrorHandler);
            this._error = new AsyncEvent<CommandsNextExtension, CommandErrorEventArgs>("COMMAND_ERRORED", TimeSpan.Zero, this.Client.EventErrorHandler);

            if (this.Config.UseDefaultCommandHandler)
                this.Client.MessageCreated += this.HandleCommandsAsync;
            else
                this.Client.Logger.LogWarning(CommandsNextEvents.Misc, "Not attaching default command handler - if this is intentional, you can ignore this message");

            if (this.Config.EnableDefaultHelp)
            {
                this.RegisterCommands(typeof(DefaultHelpModule), null, null, out var tcmds);

                if (this.Config.DefaultHelpChecks != null)
                {
                    var checks = this.Config.DefaultHelpChecks.ToArray();

                    for (var i = 0; i < tcmds.Count; i++)
                        tcmds[i].WithExecutionChecks(checks);
                }

                if (tcmds != null)
                    foreach (var xc in tcmds)
                        this.AddToCommandDictionary(xc.Build(null));
            }

            if (this.Config.CommandExecutor is ParallelQueuedCommandExecutor pqce)
                this.Client.Logger.LogDebug(CommandsNextEvents.Misc, "Using parallel executor with degree {0}", pqce.Parallelism);
        }
        #endregion

        #region Command Handling
        private async Task HandleCommandsAsync(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) // bad bot
                return;

            if (!this.Config.EnableDms && e.Channel.IsPrivate)
                return;

            var mpos = -1;
            if (this.Config.EnableMentionPrefix)
                mpos = e.Message.GetMentionPrefixLength(this.Client.CurrentUser);

            if (this.Config.StringPrefixes?.Any() == true)
                foreach (var pfix in this.Config.StringPrefixes)
                    if (mpos == -1 && !string.IsNullOrWhiteSpace(pfix))
                        mpos = e.Message.GetStringPrefixLength(pfix, this.Config.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

            if (mpos == -1 && this.Config.PrefixResolver != null)
                mpos = await this.Config.PrefixResolver(e.Message).ConfigureAwait(false);

            if (mpos == -1)
                return;

            var pfx = e.Message.Content.Substring(0, mpos);
            var cnt = e.Message.Content.Substring(mpos);

            var __ = 0;
            var fname = cnt.ExtractNextArgument(ref __);

            var cmd = this.FindCommand(cnt, out var args);
            var ctx = this.CreateContext(e.Message, pfx, cmd, args);
            if (cmd == null)
            {
                await this._error.InvokeAsync(this, new CommandErrorEventArgs { Context = ctx, Exception = new CommandNotFoundException(fname) }).ConfigureAwait(false);
                return;
            }

            await this.Config.CommandExecutor.ExecuteAsync(ctx).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds a specified command by its qualified name, then separates arguments.
        /// </summary>
        /// <param name="commandString">Qualified name of the command, optionally with arguments.</param>
        /// <param name="rawArguments">Separated arguments.</param>
        /// <returns>Found command or null if none was found.</returns>
        public Command FindCommand(string commandString, out string rawArguments)
        {
            rawArguments = null;

            var ignoreCase = !this.Config.CaseSensitive;
            var pos = 0;
            var next = commandString.ExtractNextArgument(ref pos);
            if (next == null)
                return null;

            if (!this.RegisteredCommands.TryGetValue(next, out var cmd))
            {
                if (!ignoreCase)
                    return null;

                next = next.ToLowerInvariant();
                var cmdKvp = this.RegisteredCommands.FirstOrDefault(x => x.Key.ToLowerInvariant() == next);
                if (cmdKvp.Value == null)
                    return null;

                cmd = cmdKvp.Value;
            }

            if (!(cmd is CommandGroup))
            {
                rawArguments = commandString.Substring(pos).Trim();
                return cmd;
            }

            while (cmd is CommandGroup)
            {
                var cm2 = cmd as CommandGroup;
                var oldPos = pos;
                next = commandString.ExtractNextArgument(ref pos);
                if (next == null)
                    break;

                if (ignoreCase)
                {
                    next = next.ToLowerInvariant();
                    cmd = cm2.Children.FirstOrDefault(x => x.Name.ToLowerInvariant() == next || x.Aliases?.Any(xx => xx.ToLowerInvariant() == next) == true);
                }
                else
                {
                    cmd = cm2.Children.FirstOrDefault(x => x.Name == next || x.Aliases?.Contains(next) == true);
                }

                if (cmd == null)
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
        public CommandContext CreateContext(DiscordMessage msg, string prefix, Command cmd, string rawArguments = null)
        {
            var ctx = new CommandContext
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

            if (cmd != null && (cmd.Module is TransientCommandModule || cmd.Module == null))
            {
                var scope = ctx.Services.CreateScope();
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
                var cmd = ctx.Command;
                await this.RunAllChecksAsync(cmd, ctx).ConfigureAwait(false);

                var res = await cmd.ExecuteAsync(ctx).ConfigureAwait(false);

                if (res.IsSuccessful)
                    await this._executed.InvokeAsync(this, new CommandExecutionEventArgs { Context = res.Context }).ConfigureAwait(false);
                else
                    await this._error.InvokeAsync(this, new CommandErrorEventArgs { Context = res.Context, Exception = res.Exception }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this._error.InvokeAsync(this, new CommandErrorEventArgs { Context = ctx, Exception = ex }).ConfigureAwait(false);
            }
            finally
            {
                if (ctx.ServiceScopeContext.IsInitialized)
                    ctx.ServiceScopeContext.Dispose();
            }
        }

        private async Task RunAllChecksAsync(Command cmd, CommandContext ctx)
        {
            if (cmd.Parent != null)
                await this.RunAllChecksAsync(cmd.Parent, ctx).ConfigureAwait(false);

            var fchecks = await cmd.RunChecksAsync(ctx, false).ConfigureAwait(false);
            if (fchecks.Any())
                throw new ChecksFailedException(cmd, ctx, fchecks);
        }
        #endregion

        #region Command Registration
        /// <summary>
        /// Gets a dictionary of registered top-level commands.
        /// </summary>
        public IReadOnlyDictionary<string, Command> RegisteredCommands
            => this._registeredCommandsLazy.Value;

        private Dictionary<string, Command> TopLevelCommands { get; set; }
        private readonly Lazy<IReadOnlyDictionary<string, Command>> _registeredCommandsLazy;

        /// <summary>
        /// Registers all commands from a given assembly. The command classes need to be public to be considered for registration.
        /// </summary>
        /// <param name="assembly">Assembly to register commands from.</param>
        public void RegisterCommands(Assembly assembly)
        {
            var types = assembly.ExportedTypes.Where(xt =>
            {
                var xti = xt.GetTypeInfo();
                return xti.IsModuleCandidateType() && !xti.IsNested;
            });
            foreach (var xt in types)
                this.RegisterCommands(xt);
        }

        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <typeparam name="T">Class which holds commands to register.</typeparam>
        public void RegisterCommands<T>() where T : BaseCommandModule
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
                throw new ArgumentNullException(nameof(t), "Type cannot be null.");

            if (!t.IsModuleCandidateType())
                throw new ArgumentNullException(nameof(t), "Type must be a class, which cannot be abstract or static.");

            this.RegisterCommands(t, null, null, out var tempCommands);

            if (tempCommands != null)
                foreach (var command in tempCommands)
                    this.AddToCommandDictionary(command.Build(null));
        }

        private void RegisterCommands(Type t, CommandGroupBuilder currentParent, IEnumerable<CheckBaseAttribute> inheritedChecks, out List<CommandBuilder> foundCommands)
        {
            var ti = t.GetTypeInfo();

            var lifespan = ti.GetCustomAttribute<ModuleLifespanAttribute>();
            var moduleLifespan = lifespan != null ? lifespan.Lifespan : ModuleLifespan.Singleton;

            var module = new CommandModuleBuilder()
                .WithType(t)
                .WithLifespan(moduleLifespan)
                .Build(this.Services);

            // restrict parent lifespan to more or equally restrictive
            if (currentParent?.Module is TransientCommandModule && moduleLifespan != ModuleLifespan.Transient)
                throw new InvalidOperationException("In a transient module, child modules can only be transient.");

            // check if we are anything
            var groupBuilder = new CommandGroupBuilder(module);
            var isModule = false;
            var moduleAttributes = ti.GetCustomAttributes();
            var moduleHidden = false;
            var moduleChecks = new List<CheckBaseAttribute>();

            foreach (var xa in moduleAttributes)
            {
                switch (xa)
                {
                    case GroupAttribute g:
                        isModule = true;
                        var moduleName = g.Name;
                        if (moduleName == null)
                        {
                            moduleName = ti.Name;

                            if (moduleName.EndsWith("Group") && moduleName != "Group")
                                moduleName = moduleName.Substring(0, moduleName.Length - 5);
                            else if (moduleName.EndsWith("Module") && moduleName != "Module")
                                moduleName = moduleName.Substring(0, moduleName.Length - 6);
                            else if (moduleName.EndsWith("Commands") && moduleName != "Commands")
                                moduleName = moduleName.Substring(0, moduleName.Length - 8);
                        }

                        if (!this.Config.CaseSensitive)
                            moduleName = moduleName.ToLowerInvariant();

                        groupBuilder.WithName(moduleName);

                        if (inheritedChecks != null)
                            foreach (var chk in inheritedChecks)
                                groupBuilder.WithExecutionCheck(chk);

                        foreach (var mi in ti.DeclaredMethods.Where(x => x.IsCommandCandidate(out _) && x.GetCustomAttribute<GroupCommandAttribute>() != null))
                            groupBuilder.WithOverload(new CommandOverloadBuilder(mi));
                        break;

                    case AliasesAttribute a:
                        foreach (var xalias in a.Aliases)
                            groupBuilder.WithAlias(this.Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
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
                if (inheritedChecks != null)
                    moduleChecks.AddRange(inheritedChecks);
            }

            // candidate methods
            var methods = ti.DeclaredMethods;
            var commands = new List<CommandBuilder>();
            var commandBuilders = new Dictionary<string, CommandBuilder>();
            foreach (var m in methods)
            {
                if (!m.IsCommandCandidate(out _))
                    continue;

                var attrs = m.GetCustomAttributes();
                if (attrs.FirstOrDefault(xa => xa is CommandAttribute) is not CommandAttribute cattr)
                    continue;

                var commandName = cattr.Name;
                if (commandName == null)
                {
                    commandName = m.Name;
                    if (commandName.EndsWith("Async") && commandName != "Async")
                        commandName = commandName.Substring(0, commandName.Length - 5);
                }

                if (!this.Config.CaseSensitive)
                    commandName = commandName.ToLowerInvariant();

                if (!commandBuilders.TryGetValue(commandName, out var commandBuilder))
                {
                    commandBuilders.Add(commandName, commandBuilder = new CommandBuilder(module).WithName(commandName));

                    if (!isModule)
                        if (currentParent != null)
                            currentParent.WithChild(commandBuilder);
                        else
                            commands.Add(commandBuilder);
                    else
                        groupBuilder.WithChild(commandBuilder);
                }

                commandBuilder.WithOverload(new CommandOverloadBuilder(m));

                if (!isModule && moduleChecks.Any())
                    foreach (var chk in moduleChecks)
                        commandBuilder.WithExecutionCheck(chk);

                foreach (var xa in attrs)
                {
                    switch (xa)
                    {
                        case AliasesAttribute a:
                            foreach (var xalias in a.Aliases)
                                commandBuilder.WithAlias(this.Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
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
                    commandBuilder.WithHiddenStatus(true);
            }

            // candidate types
            var types = ti.DeclaredNestedTypes
                .Where(xt => xt.IsModuleCandidateType() && xt.DeclaredConstructors.Any(xc => xc.IsPublic));
            foreach (var type in types)
            {
                this.RegisterCommands(type.AsType(),
                    groupBuilder,
                    !isModule ? moduleChecks : null,
                    out var tempCommands);

                if (isModule)
                    foreach (var chk in moduleChecks)
                        groupBuilder.WithExecutionCheck(chk);

                if (isModule && tempCommands != null)
                    foreach (var xtcmd in tempCommands)
                        groupBuilder.WithChild(xtcmd);
                else if (tempCommands != null)
                    commands.AddRange(tempCommands);
            }

            if (isModule && currentParent == null)
                commands.Add(groupBuilder);
            else if (isModule)
                currentParent.WithChild(groupBuilder);
            foundCommands = commands;
        }

        /// <summary>
        /// Builds and registers all supplied commands.
        /// </summary>
        /// <param name="cmds">Commands to build and register.</param>
        public void RegisterCommands(params CommandBuilder[] cmds)
        {
            foreach (var cmd in cmds)
                this.AddToCommandDictionary(cmd.Build(null));
        }

        /// <summary>
        /// Unregisters specified commands from CommandsNext.
        /// </summary>
        /// <param name="cmds">Commands to unregister.</param>
        public void UnregisterCommands(params Command[] cmds)
        {
            if (cmds.Any(x => x.Parent != null))
                throw new InvalidOperationException("Cannot unregister nested commands.");

            var keys = this.RegisteredCommands.Where(x => cmds.Contains(x.Value)).Select(x => x.Key).ToList();
            foreach (var key in keys)
                this.TopLevelCommands.Remove(key);
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
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class DefaultHelpModule : BaseCommandModule
        {
            [Command("help"), Description("Displays command help.")]
            public async Task DefaultHelpAsync(CommandContext ctx, [Description("Command to provide help for.")] params string[] command)
            {
                var topLevel = ctx.CommandsNext.TopLevelCommands.Values.Distinct();
                var helpBuilder = ctx.CommandsNext.HelpFormatter.Create(ctx);

                if (command != null && command.Any())
                {
                    Command cmd = null;
                    var searchIn = topLevel;
                    foreach (var c in command)
                    {
                        if (searchIn == null)
                        {
                            cmd = null;
                            break;
                        }

                        cmd = ctx.Config.CaseSensitive
                            ? searchIn.FirstOrDefault(xc => xc.Name == c || (xc.Aliases != null && xc.Aliases.Contains(c)))
                            : searchIn.FirstOrDefault(xc => xc.Name.ToLowerInvariant() == c.ToLowerInvariant() || (xc.Aliases != null && xc.Aliases.Select(xs => xs.ToLowerInvariant()).Contains(c.ToLowerInvariant())));

                        if (cmd == null)
                            break;

                        var failedChecks = await cmd.RunChecksAsync(ctx, true).ConfigureAwait(false);
                        if (failedChecks.Any())
                            throw new ChecksFailedException(cmd, ctx, failedChecks);

                        searchIn = cmd is CommandGroup ? (cmd as CommandGroup).Children : null;
                    }

                    if (cmd == null)
                        throw new CommandNotFoundException(string.Join(" ", command));

                    helpBuilder.WithCommand(cmd);

                    if (cmd is CommandGroup group)
                    {
                        var commandsToSearch = group.Children.Where(xc => !xc.IsHidden);
                        var eligibleCommands = new List<Command>();
                        foreach (var candidateCommand in commandsToSearch)
                        {
                            if (candidateCommand.ExecutionChecks == null || !candidateCommand.ExecutionChecks.Any())
                            {
                                eligibleCommands.Add(candidateCommand);
                                continue;
                            }

                            var candidateFailedChecks = await candidateCommand.RunChecksAsync(ctx, true).ConfigureAwait(false);
                            if (!candidateFailedChecks.Any())
                                eligibleCommands.Add(candidateCommand);
                        }

                        if (eligibleCommands.Any())
                            helpBuilder.WithSubcommands(eligibleCommands.OrderBy(xc => xc.Name));
                    }
                }
                else
                {
                    var commandsToSearch = topLevel.Where(xc => !xc.IsHidden);
                    var eligibleCommands = new List<Command>();
                    foreach (var sc in commandsToSearch)
                    {
                        if (sc.ExecutionChecks == null || !sc.ExecutionChecks.Any())
                        {
                            eligibleCommands.Add(sc);
                            continue;
                        }

                        var candidateFailedChecks = await sc.RunChecksAsync(ctx, true).ConfigureAwait(false);
                        if (!candidateFailedChecks.Any())
                            eligibleCommands.Add(sc);
                    }

                    if (eligibleCommands.Any())
                        helpBuilder.WithSubcommands(eligibleCommands.OrderBy(xc => xc.Name));
                }

                var helpMessage = helpBuilder.Build();

                var builder = new DiscordMessageBuilder().WithContent(helpMessage.Content).WithEmbed(helpMessage.Embed);

                if (!ctx.Config.DmHelp || ctx.Channel is DiscordDmChannel || ctx.Guild == null)
                    await ctx.RespondAsync(builder).ConfigureAwait(false);
                else
                    await ctx.Member.SendMessageAsync(builder).ConfigureAwait(false);

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
        public CommandContext CreateFakeContext(DiscordUser actor, DiscordChannel channel, string messageContents, string prefix, Command cmd, string rawArguments = null)
        {
            var epoch = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var now = DateTimeOffset.UtcNow;
            var timeSpan = (ulong)(now - epoch).TotalMilliseconds;

            // create fake message
            var msg = new DiscordMessage
            {
                Discord = this.Client,
                Author = actor,
                ChannelId = channel.Id,
                Content = messageContents,
                Id = timeSpan << 22,
                Pinned = false,
                MentionEveryone = messageContents.Contains("@everyone"),
                IsTTS = false,
                _attachments = new List<DiscordAttachment>(),
                _embeds = new List<DiscordEmbed>(),
                TimestampRaw = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                _reactions = new List<DiscordReaction>()
            };

            var mentionedUsers = new List<DiscordUser>();
            var mentionedRoles = msg.Channel.Guild != null ? new List<DiscordRole>() : null;
            var mentionedChannels = msg.Channel.Guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(msg.Content))
            {
                if (msg.Channel.Guild != null)
                {
                    mentionedUsers = Utilities.GetUserMentions(msg).Select(xid => msg.Channel.Guild._members.TryGetValue(xid, out var member) ? member : null).Cast<DiscordUser>().ToList();
                    mentionedRoles = Utilities.GetRoleMentions(msg).Select(xid => msg.Channel.Guild.GetRole(xid)).ToList();
                    mentionedChannels = Utilities.GetChannelMentions(msg).Select(xid => msg.Channel.Guild.GetChannel(xid)).ToList();
                }
                else
                {
                    mentionedUsers = Utilities.GetUserMentions(msg).Select(this.Client.GetCachedOrEmptyUserInternal).ToList();
                }
            }

            msg._mentionedUsers = mentionedUsers;
            msg._mentionedRoles = mentionedRoles;
            msg._mentionedChannels = mentionedChannels;

            var ctx = new CommandContext
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

            if (cmd != null && (cmd.Module is TransientCommandModule || cmd.Module == null))
            {
                var scope = ctx.Services.CreateScope();
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
            var t = typeof(T);
            if (!this.ArgumentConverters.ContainsKey(t))
                throw new ArgumentException("There is no converter specified for given type.", nameof(T));

            if (this.ArgumentConverters[t] is not IArgumentConverter<T> cv)
                throw new ArgumentException("Invalid converter registered for this type.", nameof(T));

            var cvr = await cv.ConvertAsync(value, ctx).ConfigureAwait(false);
            return !cvr.HasValue ? throw new ArgumentException("Could not convert specified value to given type.", nameof(value)) : cvr.Value;
        }

        /// <summary>
        /// Converts a string to specified type.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="ctx">Context in which to convert to.</param>
        /// <param name="type">Type to convert to.</param>
        /// <returns>Converted object.</returns>
        public async Task<object> ConvertArgument(string value, CommandContext ctx, Type type)
        {
            var m = this.ConvertGeneric.MakeGenericMethod(type);
            try
            {
                return await (m.Invoke(this, new object[] { value, ctx }) as Task<object>).ConfigureAwait(false);
            }
            catch (TargetInvocationException ex)
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
            if (converter == null)
                throw new ArgumentNullException(nameof(converter), "Converter cannot be null.");

            var t = typeof(T);
            var ti = t.GetTypeInfo();
            this.ArgumentConverters[t] = converter;

            if (!ti.IsValueType)
                return;

            var nullableConverterType = typeof(NullableConverter<>).MakeGenericType(t);
            var nullableType = typeof(Nullable<>).MakeGenericType(t);
            if (this.ArgumentConverters.ContainsKey(nullableType))
                return;

            var nullableConverter = Activator.CreateInstance(nullableConverterType) as IArgumentConverter;
            this.ArgumentConverters[nullableType] = nullableConverter;
        }

        /// <summary>
        /// Unregisters an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to unregister the converter.</typeparam>
        public void UnregisterConverter<T>()
        {
            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (this.ArgumentConverters.ContainsKey(t))
                this.ArgumentConverters.Remove(t);

            if (this.UserFriendlyTypeNames.ContainsKey(t))
                this.UserFriendlyTypeNames.Remove(t);

            if (!ti.IsValueType)
                return;

            var nullableType = typeof(Nullable<>).MakeGenericType(t);
            if (!this.ArgumentConverters.ContainsKey(nullableType))
                return;

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
                throw new ArgumentNullException(nameof(value), "Name cannot be null or empty.");

            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (!this.ArgumentConverters.ContainsKey(t))
                throw new InvalidOperationException("Cannot register a friendly name for a type which has no associated converter.");

            this.UserFriendlyTypeNames[t] = value;

            if (!ti.IsValueType)
                return;

            var nullableType = typeof(Nullable<>).MakeGenericType(t);
            this.UserFriendlyTypeNames[nullableType] = value;
        }

        /// <summary>
        /// Converts a type into user-friendly type name.
        /// </summary>
        /// <param name="t">Type to convert.</param>
        /// <returns>User-friendly type name.</returns>
        public string GetUserFriendlyTypeName(Type t)
        {
            if (this.UserFriendlyTypeNames.ContainsKey(t))
                return this.UserFriendlyTypeNames[t];

            var ti = t.GetTypeInfo();
            if (ti.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var tn = ti.GenericTypeArguments[0];
                return this.UserFriendlyTypeNames.ContainsKey(tn) ? this.UserFriendlyTypeNames[tn] : tn.Name;
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
            add { this._executed.Register(value); }
            remove { this._executed.Unregister(value); }
        }
        private AsyncEvent<CommandsNextExtension, CommandExecutionEventArgs> _executed;

        /// <summary>
        /// Triggered whenever a command throws an exception during execution.
        /// </summary>
        public event AsyncEventHandler<CommandsNextExtension, CommandErrorEventArgs> CommandErrored
        {
            add { this._error.Register(value); }
            remove { this._error.Unregister(value); }
        }
        private AsyncEvent<CommandsNextExtension, CommandErrorEventArgs> _error;

        private Task OnCommandExecuted(CommandExecutionEventArgs e)
            => this._executed.InvokeAsync(this, e);

        private Task OnCommandErrored(CommandErrorEventArgs e)
            => this._error.InvokeAsync(this, e);
        #endregion
    }
}
