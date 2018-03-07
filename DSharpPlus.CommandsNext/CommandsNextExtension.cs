﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Builders;
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
        private CommandsNextConfiguration Config { get; }
        private HelpFormatterFactory HelpFormatter { get; }

        private MethodInfo ConvertGeneric { get; }
        private Dictionary<Type, string> UserFriendlyTypeNames { get; }
        internal Dictionary<Type, IArgumentConverter> ArgumentConverters { get; }

        /// <summary>
        /// Gets the service provider this CommandsNext module was configured with.
        /// </summary>
        public IServiceProvider Services
            => this.Config.Services;

        internal CommandsNextExtension(CommandsNextConfiguration cfg)
        {
            this.Config = new CommandsNextConfiguration(cfg);
            this.TopLevelCommands = new Dictionary<string, Command>();
            this._registered_commands_lazy = new Lazy<IReadOnlyDictionary<string, Command>>(() => new ReadOnlyDictionary<string, Command>(this.TopLevelCommands));
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
                [typeof(DiscordUser)] = new DiscordUserConverter(),
                [typeof(DiscordMember)] = new DiscordMemberConverter(),
                [typeof(DiscordRole)] = new DiscordRoleConverter(),
                [typeof(DiscordChannel)] = new DiscordChannelConverter(),
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
                if (ArgumentConverters.ContainsKey(xcvt))
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
        public void SetHelpFormatter<T>() where T : BaseHelpFormatter
        {
            this.HelpFormatter.SetFormatterType<T>();
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
                this.RegisterCommands<DefaultHelpModule>();
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

            if (this.Config.Selfbot && e.Author.Id != this.Client.CurrentUser.Id)
                return;

            var mpos = -1;
            if (this.Config.EnableMentionPrefix)
                mpos = e.Message.GetMentionPrefixLength(this.Client.CurrentUser);

            if (this.Config.StringPrefixes?.Any() == true)
                foreach (var pfix in this.Config.StringPrefixes)
                    if (mpos == -1 && !string.IsNullOrWhiteSpace(pfix))
                        mpos = e.Message.GetStringPrefixLength(pfix);

            if (mpos == -1 && this.Config.PrefixResolver != null)
                mpos = await this.Config.PrefixResolver(e.Message).ConfigureAwait(false);

            if (mpos == -1)
                return;

            var pfx = e.Message.Content.Substring(0, mpos);
            var cnt = e.Message.Content.Substring(mpos);
            int sp = 0;
            var cms = CommandsNextUtilities.ExtractNextArgument(cnt, ref sp);

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
                RawArgumentString = cnt.Substring(sp),
                Prefix = pfx,
                CommandsNext = this,
                Services = this.Services
            };

            if (cmd == null)
            {
                await this._error.InvokeAsync(new CommandErrorEventArgs { Context = ctx, Exception = new CommandNotFoundException(cms) }).ConfigureAwait(false);
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var fchecks = await cmd.RunChecksAsync(ctx, false).ConfigureAwait(false);
                    if (fchecks.Any())
                        throw new ChecksFailedException(cmd, ctx, fchecks);

                    var res = await cmd.ExecuteAsync(ctx).ConfigureAwait(false);

                    if (res.IsSuccessful)
                        await this._executed.InvokeAsync(new CommandExecutionEventArgs { Context = res.Context }).ConfigureAwait(false);
                    else
                        await this._error.InvokeAsync(new CommandErrorEventArgs { Context = res.Context, Exception = res.Exception }).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await this._error.InvokeAsync(new CommandErrorEventArgs { Context = ctx, Exception = ex }).ConfigureAwait(false);
                }
                finally
                {
                    if (ctx.ServiceScopeContext.IsInitialized)
                        ctx.ServiceScopeContext.Dispose();
                }
            });
        }
        #endregion

        #region Command Registration
        /// <summary>
        /// Gets a dictionary of registered top-level commands.
        /// </summary>
        public IReadOnlyDictionary<string, Command> RegisteredCommands
            => this._registered_commands_lazy.Value;

        private Dictionary<string, Command> TopLevelCommands { get; set; }
        private Lazy<IReadOnlyDictionary<string, Command>> _registered_commands_lazy;

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

            this.RegisterCommands(t, null, out var tcmds);

            if (tcmds != null)
                foreach (var xc in tcmds)
                    this.AddToCommandDictionary(xc.Build(null));
        }

        private void RegisterCommands(Type t, CommandGroupBuilder currentParent, out List<CommandBuilder> commands)
        {
            var ti = t.GetTypeInfo();

            var lifespan = ti.GetCustomAttribute<ModuleLifespanAttribute>();
            var module = new CommandModuleBuilder()
                .WithType(t)
                .WithLifespan(lifespan != null ? lifespan.Lifespan : ModuleLifespan.Singleton)
                .Build(this.Services);

            // check if we are anything
            var cgbldr = new CommandGroupBuilder(module);
            var is_mdl = false;
            var mdl_attrs = ti.GetCustomAttributes();
            var mdl_hidden = false;
            var mdl_chks = new List<CheckBaseAttribute>();
            foreach (var xa in mdl_attrs)
            {
                switch (xa)
                {
                    case GroupAttribute g:
                        is_mdl = true;
                        var mdl_name = g.Name;
                        if (mdl_name == null)
                        {
                            mdl_name = ti.Name;

                            if (mdl_name.EndsWith("Group") && mdl_name != "Group")
                                mdl_name = mdl_name.Substring(0, mdl_name.Length - 5);
                            else if (mdl_name.EndsWith("Module") && mdl_name != "Module")
                                mdl_name = mdl_name.Substring(0, mdl_name.Length - 6);
                            else if (mdl_name.EndsWith("Commands") && mdl_name != "Commands")
                                mdl_name = mdl_name.Substring(0, mdl_name.Length - 8);
                        }

                        if (!this.Config.CaseSensitive)
                            mdl_name = mdl_name.ToLowerInvariant();

                        cgbldr.WithName(mdl_name);
                        
                        foreach (var mi in ti.DeclaredMethods.Where(x => x.IsCommandCandidate(out _) && x.GetCustomAttribute<GroupCommandAttribute>() != null))
                            cgbldr.WithOverload(new CommandOverloadBuilder(mi));
                        break;

                    case AliasesAttribute a:
                        foreach (var xalias in a.Aliases)
                            cgbldr.WithAlias(this.Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
                        break;

                    case HiddenAttribute h:
                        cgbldr.WithHiddenStatus(true);
                        break;

                    case DescriptionAttribute d:
                        cgbldr.WithDescription(d.Description);
                        break;

                    case CheckBaseAttribute c:
                        mdl_chks.Add(c);
                        cgbldr.WithExecutionCheck(c);
                        break;

                    default:
                        cgbldr.WithCustomAttribute(xa);
                        break;
                }
            }

            if (!is_mdl)
                cgbldr = null;

            // candidate methods
            var ms = ti.DeclaredMethods;
            var cmds = new List<CommandBuilder>();
            var cblds = new Dictionary<string, CommandBuilder>();
            foreach (var m in ms)
            {
                if (!m.IsCommandCandidate(out _))
                    continue;

                var attrs = m.GetCustomAttributes();
                var cattr = attrs.FirstOrDefault(xa => xa is CommandAttribute) as CommandAttribute;
                if (cattr == null)
                    continue;

                var cname = cattr.Name;
                if (cname == null)
                {
                    cname = m.Name;
                    if (cname.EndsWith("Async") && cname != "Async")
                        cname = cname.Substring(0, cname.Length - 5);
                }

                if (!this.Config.CaseSensitive)
                    cname = cname.ToLowerInvariant();

                if (!cblds.TryGetValue(cname, out var cmdbld))
                {
                    cblds.Add(cname, cmdbld = new CommandBuilder(module).WithName(cname));

                    if (!is_mdl)
                        if (currentParent != null)
                            currentParent.WithChild(cmdbld);
                        else
                            cmds.Add(cmdbld);
                    else
                        cgbldr.WithChild(cmdbld);
                }

                cmdbld.WithOverload(new CommandOverloadBuilder(m));

                if (!is_mdl && mdl_chks.Any())
                    foreach (var chk in mdl_chks)
                        cmdbld.WithExecutionCheck(chk);

                foreach (var xa in attrs)
                {
                    switch (xa)
                    {
                        case AliasesAttribute a:
                            foreach (var xalias in a.Aliases)
                                cmdbld.WithAlias(this.Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
                            break;

                        case CheckBaseAttribute p:
                            cmdbld.WithExecutionCheck(p);
                            break;

                        case DescriptionAttribute d:
                            cmdbld.WithDescription(d.Description);
                            break;

                        case HiddenAttribute h:
                            cmdbld.WithHiddenStatus(true);
                            break;

                        default:
                            cmdbld.WithCustomAttribute(xa);
                            break;
                    }
                }

                if (!is_mdl && mdl_hidden)
                    cmdbld.WithHiddenStatus(true);
            }

            // candidate types
            var ts = ti.DeclaredNestedTypes
                .Where(xt => xt.IsModuleCandidateType() && xt.DeclaredConstructors.Any(xc => xc.IsPublic));
            foreach (var xt in ts)
            {
                this.RegisterCommands(xt.AsType(), cgbldr, out var tcmds);

                if (is_mdl && tcmds != null)
                        foreach (var xtcmd in tcmds)
                            cgbldr.WithChild(xtcmd);
                else if (tcmds != null)
                    cmds.AddRange(tcmds);
            }

            if (is_mdl && currentParent == null)
                cmds.Add(cgbldr);
            else if (is_mdl)
                currentParent.WithChild(cgbldr);
            commands = cmds;
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
        public class DefaultHelpModule : BaseCommandModule
        {
            [Command("help"), Description("Displays command help.")]
            public async Task DefaultHelpAsync(CommandContext ctx, [Description("Command to provide help for.")] params string[] command)
            {
                var toplevel = ctx.CommandsNext.TopLevelCommands.Values.Distinct();
                var helpbuilder = ctx.CommandsNext.HelpFormatter.Create(ctx.Services, ctx.CommandsNext);

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

                        if (ctx.Config.CaseSensitive)
                            cmd = search_in.FirstOrDefault(xc => xc.Name == c || (xc.Aliases != null && xc.Aliases.Contains(c)));
                        else
                            cmd = search_in.FirstOrDefault(xc => xc.Name.ToLowerInvariant() == c.ToLowerInvariant() || (xc.Aliases != null && xc.Aliases.Select(xs => xs.ToLowerInvariant()).Contains(c.ToLowerInvariant())));

                        if (cmd == null)
                            break;

                        var cfl = await cmd.RunChecksAsync(ctx, true).ConfigureAwait(false);
                        if (cfl.Any())
                            throw new ChecksFailedException(cmd, ctx, cfl);

                        if (cmd is CommandGroup)
                            search_in = (cmd as CommandGroup).Children;
                        else
                            search_in = null;
                    }

                    if (cmd == null)
                        throw new CommandNotFoundException(string.Join(" ", command));

                    helpbuilder.WithCommand(cmd);

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

                            var cfl = await sc.RunChecksAsync(ctx, true).ConfigureAwait(false);
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

                        var cfl = await sc.RunChecksAsync(ctx, true).ConfigureAwait(false);
                        if (!cfl.Any())
                            scs.Add(sc);
                    }

                    if (scs.Any())
                        helpbuilder.WithSubcommands(scs.OrderBy(xc => xc.Name));
                }

                var hmsg = helpbuilder.Build();
                await ctx.RespondAsync(hmsg.Content, embed: hmsg.Embed).ConfigureAwait(false);
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
                    mentioned_users = Utilities.GetUserMentions(msg).Select(this.Client.InternalGetCachedUser).ToList();
                }
            }

            msg._mentionedUsers = mentioned_users;
            msg._mentionedRoles = mentioned_roles;
            msg._mentionedChannels = mentioned_channels;

            await this.HandleCommandsAsync(new MessageCreateEventArgs(this.Client) { Message = msg }).ConfigureAwait(false);
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

            var cv = this.ArgumentConverters[t] as IArgumentConverter<T>;
            if (cv == null)
                throw new ArgumentException("Invalid converter registered for this type.", nameof(T));

            var cvr = await cv.ConvertAsync(value, ctx).ConfigureAwait(false);
            if (!cvr.HasValue)
                throw new ArgumentException("Could not convert specified value to given type.", nameof(value));

            return cvr.Value;
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

            var ncvt = typeof(NullableConverter<>).MakeGenericType(t);
            var nt = typeof(Nullable<>).MakeGenericType(t);
            if (this.ArgumentConverters.ContainsKey(nt))
                return;

            var ncv = Activator.CreateInstance(ncvt) as IArgumentConverter;
            this.ArgumentConverters[nt] = ncv;
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

            var nt = typeof(Nullable<>).MakeGenericType(t);
            if (!this.ArgumentConverters.ContainsKey(nt))
                return;

            this.ArgumentConverters.Remove(nt);
            this.UserFriendlyTypeNames.Remove(nt);
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

            var ncvt = typeof(NullableConverter<>).MakeGenericType(t);
            var nt = typeof(Nullable<>).MakeGenericType(t);
            this.UserFriendlyTypeNames[nt] = value;
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
                if (this.UserFriendlyTypeNames.ContainsKey(tn))
                    return this.UserFriendlyTypeNames[tn];

                return tn.Name;
            }

            return t.Name;
        }
        #endregion

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

        private Task OnCommandExecuted(CommandExecutionEventArgs e)
            => this._executed.InvokeAsync(e);

        private Task OnCommandErrored(CommandErrorEventArgs e)
            => this._error.InvokeAsync(e);
        #endregion
    }
}
