using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands.EventArgs;
using Emzi0767.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// A class that handles slash commands for a client.
    /// </summary>
    public sealed class SlashCommandsExtension : BaseExtension
    {
        private static List<CommandMethod> _commandMethods { get; set; } = new List<CommandMethod>();
        private static List<GroupCommand> _groupCommands { get; set; } = new List<GroupCommand>();
        private static List<SubGroupCommand> _subGroupCommands { get; set; } = new List<SubGroupCommand>();
        private static List<ContextMenuCommand> _contextMenuCommands { get; set; } = new List<ContextMenuCommand>();

        private static List<object> _singletonModules { get; set; } = new List<object>();

        private List<KeyValuePair<ulong?, Type>> _updateList { get; set; } = new List<KeyValuePair<ulong?, Type>>();
        private readonly SlashCommandsConfiguration _configuration;
        private static bool _errored { get; set; } = false;

        /// <summary>
        /// Gets a list of registered commands. The key is the guild id (null if global).
        /// </summary>
        public IReadOnlyList<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> RegisteredCommands
            => _registeredCommands;
        private static List<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> _registeredCommands = new List<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>>();

        internal SlashCommandsExtension(SlashCommandsConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Runs setup. DO NOT RUN THIS MANUALLY. DO NOT DO ANYTHING WITH THIS.
        /// </summary>
        /// <param name="client">The client to setup on.</param>
        protected override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = client;

            _slashError = new AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs>("SLASHCOMMAND_ERRORED", TimeSpan.Zero, null);
            _slashExecuted = new AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs>("SLASHCOMMAND_EXECUTED", TimeSpan.Zero, null);
            _contextMenuErrored = new AsyncEvent<SlashCommandsExtension, ContextMenuErrorEventArgs>("CONTEXTMENU_ERRORED", TimeSpan.Zero, null);
            _contextMenuExecuted = new AsyncEvent<SlashCommandsExtension, ContextMenuExecutedEventArgs>("CONTEXTMENU_EXECUTED", TimeSpan.Zero, null);

            Client.Ready += Update;
            Client.InteractionCreated += InteractionHandler;
            Client.ContextMenuInteractionCreated += ContextMenuHandler;
        }

        /// <summary>
        /// Registers a command class.
        /// </summary>
        /// <typeparam name="T">The command class to register.</typeparam>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands<T>(ulong? guildId = null) where T : ApplicationCommandModule
        {
            if (Client.ShardId == 0)
                _updateList.Add(new KeyValuePair<ulong?, Type>(guildId, typeof(T)));
        }

        /// <summary>
        /// Registers a command class.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the command class to register.</param>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands(Type type, ulong? guildId = null)
        {
            if (!typeof(ApplicationCommandModule).IsAssignableFrom(type))
                throw new ArgumentException("Command classes have to inherit from ApplicationCommandModule", nameof(type));
            if (Client.ShardId == 0)
                _updateList.Add(new KeyValuePair<ulong?, Type>(guildId, type));
        }

        internal Task Update(DiscordClient client, ReadyEventArgs e)
            => Update();
 
        internal Task Update()
        {
            if (Client.ShardId == 0)
            {
                foreach (var key in _updateList.Select(x => x.Key).Distinct())
                {
                    RegisterCommands(_updateList.Where(x => x.Key == key).Select(x => x.Value), key);
                }
            }
            return Task.CompletedTask;
        }

        private void RegisterCommands(IEnumerable<Type> types, ulong? guildid)
        {
            var commandMethods = new List<CommandMethod>();
            var groupCommands = new List<GroupCommand>();
            var subGroupCommands = new List<SubGroupCommand>();
            var contextMenuCommands = new List<ContextMenuCommand>();
            var updateList = new List<DiscordApplicationCommand>();

            _ = Task.Run(async () =>
            {
                foreach (var type in types)
                {
                    try
                    {
                        var module = type.GetTypeInfo();

                        var classes = new List<TypeInfo>();

                        //Add module to classes list only if it's a group
                        if (module.GetCustomAttribute<SlashCommandGroupAttribute>() != null)
                        {
                            classes.Add(module);
                        }
                        else
                        {
                            classes = module.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null).ToList();
                        }

                        //Handles groups
                        foreach (var subclassinfo in classes)
                        {
                            var groupatt = subclassinfo.GetCustomAttribute<SlashCommandGroupAttribute>();
                            var submethods = subclassinfo.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);
                            var subclasses = subclassinfo.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null);
                            if (subclasses.Any() && submethods.Any())
                            {
                                throw new ArgumentException("Slash command groups cannot have both subcommands and subgroups!");
                            }
                            var payload = new DiscordApplicationCommand(groupatt.Name, groupatt.Description, defaultPermission: groupatt.DefaultPermission);

                            var commandmethods = new List<KeyValuePair<string, MethodInfo>>();
                            foreach (var submethod in submethods)
                            {
                                var commandattribute = submethod.GetCustomAttribute<SlashCommandAttribute>();

                                var parameters = submethod.GetParameters();
                                if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                    throw new ArgumentException($"The first argument must be an InteractionContext!");
                                parameters = parameters.Skip(1).ToArray();

                                var options = await ParseParameters(parameters);

                                var subpayload = new DiscordApplicationCommandOption(commandattribute.Name, commandattribute.Description, ApplicationCommandOptionType.SubCommand, null, null, options);

                                commandmethods.Add(new KeyValuePair<string, MethodInfo>(commandattribute.Name, submethod));

                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission);

                                groupCommands.Add(new GroupCommand { Name = groupatt.Name, Methods = commandmethods });
                            }
                            var command = new SubGroupCommand { Name = groupatt.Name };
                            foreach (var subclass in subclasses)
                            {
                                var subgroupatt = subclass.GetCustomAttribute<SlashCommandGroupAttribute>();
                                var subsubmethods = subclass.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

                                var options = new List<DiscordApplicationCommandOption>();

                                var currentMethods = new List<KeyValuePair<string, MethodInfo>>();

                                foreach (var subsubmethod in subsubmethods)
                                {
                                    var suboptions = new List<DiscordApplicationCommandOption>();
                                    var commatt = subsubmethod.GetCustomAttribute<SlashCommandAttribute>();
                                    var parameters = subsubmethod.GetParameters();
                                    if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                        throw new ArgumentException($"The first argument must be an InteractionContext!");
                                    parameters = parameters.Skip(1).ToArray();
                                    suboptions = suboptions.Concat(await ParseParameters(parameters)).ToList();

                                    var subsubpayload = new DiscordApplicationCommandOption(commatt.Name, commatt.Description, ApplicationCommandOptionType.SubCommand, null, null, suboptions);
                                    options.Add(subsubpayload);
                                    commandmethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
                                    currentMethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
                                }

                                var subpayload = new DiscordApplicationCommandOption(subgroupatt.Name, subgroupatt.Description, ApplicationCommandOptionType.SubCommandGroup, null, null, options);
                                command.SubCommands.Add(new GroupCommand { Name = subgroupatt.Name, Methods = currentMethods });
                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission);

                                if (subclass.GetCustomAttribute<SlashModuleLifespanAttribute>() != null)
                                {
                                    if (subclass.GetCustomAttribute<SlashModuleLifespanAttribute>().Lifespan == SlashModuleLifespan.Singleton)
                                    {
                                        _singletonModules.Add(CreateInstance(subclass, _configuration?.Services));
                                    }
                                }
                            }
                            if (command.SubCommands.Any()) subGroupCommands.Add(command);
                            updateList.Add(payload);


                            if (subclassinfo.GetCustomAttribute<SlashModuleLifespanAttribute>() != null)
                            {
                                if (subclassinfo.GetCustomAttribute<SlashModuleLifespanAttribute>().Lifespan == SlashModuleLifespan.Singleton)
                                {
                                    _singletonModules.Add(CreateInstance(subclassinfo, _configuration?.Services));
                                }
                            }
                        }

                        //Handles methods and context menus, only if the module isn't a group itself.
                        if (module.GetCustomAttribute<SlashCommandGroupAttribute>() == null)
                        {
                            //Slash commands
                            var methods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);
                            foreach (var method in methods)
                            {
                                var commandattribute = method.GetCustomAttribute<SlashCommandAttribute>();

                                var parameters = method.GetParameters();
                                if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(InteractionContext)))
                                    throw new ArgumentException($"The first argument must be an InteractionContext!");
                                parameters = parameters.Skip(1).ToArray();
                                var options = await ParseParameters(parameters);

                                commandMethods.Add(new CommandMethod { Method = method, Name = commandattribute.Name });

                                var payload = new DiscordApplicationCommand(commandattribute.Name, commandattribute.Description, options, commandattribute.DefaultPermission);
                                updateList.Add(payload);
                            }

                            //Context Menus
                            var contextMethods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null);
                            foreach (var contextMethod in contextMethods)
                            {
                                var contextAttribute = contextMethod.GetCustomAttribute<ContextMenuAttribute>();
                                var command = new DiscordApplicationCommand(contextAttribute.Name, null, type: contextAttribute.Type);

                                var parameters = contextMethod.GetParameters();
                                if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(ContextMenuContext)))
                                    throw new ArgumentException($"The first argument must be a ContextMenuContext!");
                                if (parameters.Length > 1)
                                    throw new ArgumentException($"A context menu cannot have parameters!");

                                contextMenuCommands.Add(new ContextMenuCommand { Method = contextMethod, Name = contextAttribute.Name });

                                updateList.Add(command);
                            }

                            if (module.GetCustomAttribute<SlashModuleLifespanAttribute>() != null)
                            {
                                if (module.GetCustomAttribute<SlashModuleLifespanAttribute>().Lifespan == SlashModuleLifespan.Singleton)
                                {
                                    _singletonModules.Add(CreateInstance(module, _configuration?.Services));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is BadRequestException brex)
                            Client.Logger.LogCritical(brex, $"There was an error registering application commands: {brex.JsonMessage}");
                        else
                            Client.Logger.LogCritical(ex, $"There was an error registering application commands");
                        _errored = true;
                    }
                }
                if (!_errored)
                {
                    try
                    {
                        IEnumerable<DiscordApplicationCommand> commands;
                        if (guildid == null)
                        {
                            commands = await Client.BulkOverwriteGlobalApplicationCommandsAsync(updateList);
                        }
                        else
                        {
                            commands = await Client.BulkOverwriteGuildApplicationCommandsAsync(guildid.Value, updateList);
                        }
                        foreach (var command in commands)
                        {
                            if (commandMethods.Any(x => x.Name == command.Name))
                                commandMethods.First(x => x.Name == command.Name).CommandId = command.Id;

                            else if (groupCommands.Any(x => x.Name == command.Name))
                                groupCommands.First(x => x.Name == command.Name).CommandId = command.Id;

                            else if (subGroupCommands.Any(x => x.Name == command.Name))
                                subGroupCommands.First(x => x.Name == command.Name).CommandId = command.Id;

                            else if (contextMenuCommands.Any(x => x.Name == command.Name))
                                contextMenuCommands.First(x => x.Name == command.Name).CommandId = command.Id;
                        }
                        _commandMethods.AddRange(commandMethods);
                        _groupCommands.AddRange(groupCommands);
                        _subGroupCommands.AddRange(subGroupCommands);
                        _contextMenuCommands.AddRange(contextMenuCommands);

                        _registeredCommands.Add(new KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>(guildid, commands.ToList()));
                    }
                    catch (Exception ex)
                    {
                        if (ex is BadRequestException brex)
                            Client.Logger.LogCritical(brex, $"There was an error registering application commands: {brex.JsonMessage}");
                        else
                            Client.Logger.LogCritical(ex, $"There was an error registering application commands");
                        _errored = true;
                    }
                }
            });
        }

        private Task InteractionHandler(DiscordClient client, InteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Interaction.Type == InteractionType.ApplicationCommand)
                {
                    InteractionContext context = new InteractionContext
                    {
                        Interaction = e.Interaction,
                        Channel = e.Interaction.Channel,
                        Guild = e.Interaction.Guild,
                        User = e.Interaction.User,
                        Client = client,
                        SlashCommandsExtension = this,
                        CommandName = e.Interaction.Data.Name,
                        InteractionId = e.Interaction.Id,
                        Token = e.Interaction.Token,
                        Services = this._configuration?.Services,
                        ResolvedUserMentions = e.Interaction.Data.Resolved?.Users?.Values.ToList(),
                        ResolvedRoleMentions = e.Interaction.Data.Resolved?.Roles?.Values.ToList(),
                        ResolvedChannelMentions = e.Interaction.Data.Resolved?.Channels?.Values.ToList(),
                        Type = ApplicationCommandType.SlashCommand
                    };

                    try
                    {
                        if (_errored)
                            throw new InvalidOperationException("Slash commands failed to register properly on startup.");
                        var methods = _commandMethods.Where(x => x.CommandId == e.Interaction.Data.Id);
                        var groups = _groupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                        var subgroups = _subGroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                        if (!methods.Any() && !groups.Any() && !subgroups.Any())
                            throw new InvalidOperationException("A slash command was executed, but no command was registered for it.");

                        if (methods.Any())
                        {
                            var method = methods.First().Method;

                            var args = await ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options);

                            await RunCommand(context, method, args);
                        }
                        else if (groups.Any())
                        {
                            var command = e.Interaction.Data.Options.First();
                            var method = groups.First().Methods.First(x => x.Key == command.Name).Value;

                            var args = await ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options.First().Options);

                            await RunCommand(context, method, args);
                        }
                        else if (subgroups.Any())
                        {
                            var command = e.Interaction.Data.Options.First();
                            var group = subgroups.First().SubCommands.First(x => x.Name == command.Name);

                            var method = group.Methods.First(x => x.Key == command.Options.First().Name).Value;

                            var args = await ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options.First().Options.First().Options);

                            await RunCommand(context, method, args);
                        }

                        await _slashExecuted.InvokeAsync(this, new SlashCommandExecutedEventArgs { Context = context });
                    }
                    catch (Exception ex)
                    {
                        await _slashError.InvokeAsync(this, new SlashCommandErrorEventArgs { Context = context, Exception = ex });
                    }
                }
            });
            return Task.CompletedTask;
        }

        private Task ContextMenuHandler(DiscordClient client, ContextMenuInteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                ContextMenuContext context = new ContextMenuContext
                {
                    Interaction = e.Interaction,
                    Channel = e.Interaction.Channel,
                    Client = client,
                    Services = this._configuration?.Services,
                    CommandName = e.Interaction.Data.Name,
                    SlashCommandsExtension = this,
                    Guild = e.Interaction.Guild,
                    InteractionId = e.Interaction.Id,
                    User = e.Interaction.User,
                    Token = e.Interaction.Token,
                    TargetUser = e.TargetUser,
                    TargetMessage = e.TargetMessage,
                    Type = e.Type
                };

                try
                {
                    if (_errored)
                        throw new InvalidOperationException("Context menus failed to register properly on startup.");

                    var method = _contextMenuCommands.FirstOrDefault(x => x.CommandId == e.Interaction.Data.Id);

                    if (method == null)
                        throw new InvalidOperationException("A context menu was executed, but no command was registered for it.");

                    await RunCommand(context, method.Method, new[] { context });

                    await _contextMenuExecuted.InvokeAsync(this, new ContextMenuExecutedEventArgs { Context = context });
                }
                catch (Exception ex)
                {
                    await _contextMenuErrored.InvokeAsync(this, new ContextMenuErrorEventArgs { Context = context, Exception = ex });
                }
            });

            return Task.CompletedTask;
        }

        internal async Task RunCommand(BaseContext context, MethodInfo method, IEnumerable<object> args)
        {
            object classinstance;
            SlashModuleLifespan moduleLifespan = (method.DeclaringType.GetCustomAttribute<SlashModuleLifespanAttribute>() != null ? method.DeclaringType.GetCustomAttribute<SlashModuleLifespanAttribute>()?.Lifespan : SlashModuleLifespan.Transient) ?? SlashModuleLifespan.Transient;

            switch (moduleLifespan)
            {
                case SlashModuleLifespan.Scoped:
                    classinstance = method.IsStatic ? ActivatorUtilities.CreateInstance(_configuration?.Services.CreateScope().ServiceProvider, method.DeclaringType) : CreateInstance(method.DeclaringType, _configuration?.Services.CreateScope().ServiceProvider);
                    break;
                
                case SlashModuleLifespan.Transient:
                    classinstance = method.IsStatic ? ActivatorUtilities.CreateInstance(_configuration?.Services, method.DeclaringType) : CreateInstance(method.DeclaringType, _configuration?.Services);
                    break;
                
                case SlashModuleLifespan.Singleton:
                    classinstance = _singletonModules.First(x => ReferenceEquals(x.GetType(), method.DeclaringType));
                    break;
                
                default:
                    throw new Exception($"An unknown {nameof(SlashModuleLifespanAttribute)} scope was specified on command {context.CommandName}");
            }
            
            ApplicationCommandModule module = null;
            if (classinstance is ApplicationCommandModule mod)
                module = mod;

            if (context is InteractionContext slashContext)
            {
                await RunPreexecutionChecksAsync(method, slashContext);

                var shouldExecute = await (module?.BeforeSlashExecutionAsync(slashContext) ?? Task.FromResult(true));

                if (shouldExecute)
                {
                    await (Task)method.Invoke(classinstance, args.ToArray());

                    await (module?.AfterSlashExecutionAsync(slashContext) ?? Task.CompletedTask);
                }
            }
            if (context is ContextMenuContext CMContext)
            {
                await RunPreexecutionChecksAsync(method, CMContext);

                var shouldExecute = await (module?.BeforeContextMenuExecutionAsync(CMContext) ?? Task.FromResult(true));

                if (shouldExecute)
                {
                    await (Task)method.Invoke(classinstance, args.ToArray());

                    await (module?.AfterContextMenuExecutionAsync(CMContext) ?? Task.CompletedTask);
                }
            }
        }

        internal object CreateInstance(Type t, IServiceProvider services)
        {
            var ti = t.GetTypeInfo();
            var constructors = ti.DeclaredConstructors
                .Where(xci => xci.IsPublic)
                .ToArray();

            if (constructors.Length != 1)
                throw new ArgumentException("Specified type does not contain a public constructor or contains more than one public constructor.");

            var constructor = constructors[0];
            var constructorArgs = constructor.GetParameters();
            var args = new object[constructorArgs.Length];

            if (constructorArgs.Length != 0 && services == null)
                throw new InvalidOperationException("Dependency collection needs to be specified for parameterized constructors.");

            // inject via constructor
            if (constructorArgs.Length != 0)
                for (var i = 0; i < args.Length; i++)
                    args[i] = services.GetRequiredService(constructorArgs[i].ParameterType);

            var moduleInstance = Activator.CreateInstance(t, args);

            // inject into properties
            var props = t.GetRuntimeProperties().Where(xp => xp.CanWrite && xp.SetMethod != null && !xp.SetMethod.IsStatic && xp.SetMethod.IsPublic);
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<DontInjectAttribute>() != null)
                    continue;

                var service = services.GetService(prop.PropertyType);
                if (service == null)
                    continue;

                prop.SetValue(moduleInstance, service);
            }

            // inject into fields
            var fields = t.GetRuntimeFields().Where(xf => !xf.IsInitOnly && !xf.IsStatic && xf.IsPublic);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<DontInjectAttribute>() != null)
                    continue;

                var service = services.GetService(field.FieldType);
                if (service == null)
                    continue;

                field.SetValue(moduleInstance, service);
            }

            return moduleInstance;
        }

        private async Task<List<object>> ResolveInteractionCommandParameters(InteractionCreateEventArgs e, InteractionContext context, MethodInfo method, IEnumerable<DiscordInteractionDataOption> options)
        {
            var args = new List<object> { context };
            var parameters = method.GetParameters().Skip(1);

            for (int i = 0; i < parameters.Count(); i++)
            {
                var parameter = parameters.ElementAt(i);
                if (parameter.IsOptional && (options == null ||
                                             (!options?.Any(x => x.Name == parameter.GetCustomAttribute<OptionAttribute>().Name.ToLower()) ?? true)))
                    args.Add(parameter.DefaultValue);
                else
                {
                    var option = options.Single(x => x.Name == parameter.GetCustomAttribute<OptionAttribute>().Name.ToLower());

                    if (ReferenceEquals(parameter.ParameterType, typeof(string)))
                        args.Add(option.Value.ToString());
                    else if (parameter.ParameterType.IsEnum)
                        args.Add(Enum.Parse(parameter.ParameterType, (string)option.Value));
                    else if (ReferenceEquals(parameter.ParameterType, typeof(long)) || ReferenceEquals(parameter.ParameterType, typeof(long?)))
                        args.Add((long?)option.Value);
                    else if (ReferenceEquals(parameter.ParameterType, typeof(bool)) || ReferenceEquals(parameter.ParameterType, typeof(bool?)))
                        args.Add((bool?)option.Value);
                    else if (ReferenceEquals(parameter.ParameterType, typeof(double)) || ReferenceEquals(parameter.ParameterType, typeof(double?)))
                        args.Add((double?)option.Value);
                    else if (ReferenceEquals(parameter.ParameterType, typeof(DiscordUser)))
                    {
                        if (e.Interaction.Data.Resolved.Members != null &&
                            e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                        {
                            args.Add(member);
                        }
                        else if (e.Interaction.Data.Resolved.Users != null &&
                                 e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
                        {
                            args.Add(user);
                        }
                        else
                        {
                            args.Add(await Client.GetUserAsync((ulong)option.Value));
                        }
                    }
                    else if (ReferenceEquals(parameter.ParameterType, typeof(DiscordChannel)))
                    {
                        if (e.Interaction.Data.Resolved.Channels != null &&
                            e.Interaction.Data.Resolved.Channels.TryGetValue((ulong)option.Value, out var channel))
                        {
                            args.Add(channel);
                        }
                        else
                        {
                            args.Add(e.Interaction.Guild.GetChannel((ulong)option.Value));
                        }
                    }
                    else if (ReferenceEquals(parameter.ParameterType, typeof(DiscordRole)))
                    {
                        if (e.Interaction.Data.Resolved.Roles != null &&
                            e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                        {
                            args.Add(role);
                        }
                        else
                        {
                            args.Add(e.Interaction.Guild.GetRole((ulong)option.Value));
                        }
                    }
                    else if (ReferenceEquals(parameter.ParameterType, typeof(SnowflakeObject)))
                    {
                        if (e.Interaction.Data.Resolved.Roles != null && e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                        {
                            args.Add(role);
                        }
                        else if (e.Interaction.Data.Resolved.Members != null && e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                        {
                            args.Add(member);
                        }
                        else if (e.Interaction.Data.Resolved.Users != null && e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
                        {
                            args.Add(user);
                        }
                        else
                        {
                            throw new ArgumentException("Error resolving mentionable option.");
                        }
                    }
                    else
                        throw new ArgumentException($"Error resolving interaction.");
                }
            }

            return args;
        }

        private async Task RunPreexecutionChecksAsync(MethodInfo method, BaseContext context)
        {
            if (context is InteractionContext ctx)
            {
                var attributes = new List<SlashCheckBaseAttribute>();
                attributes.AddRange(method.GetCustomAttributes<SlashCheckBaseAttribute>(true));
                attributes.AddRange(method.DeclaringType.GetCustomAttributes<SlashCheckBaseAttribute>());
                if (method.DeclaringType.DeclaringType != null)
                {
                    attributes.AddRange(method.DeclaringType.DeclaringType.GetCustomAttributes<SlashCheckBaseAttribute>());
                    if (method.DeclaringType.DeclaringType.DeclaringType != null)
                    {
                        attributes.AddRange(method.DeclaringType.DeclaringType.DeclaringType.GetCustomAttributes<SlashCheckBaseAttribute>());
                    }
                }

                var dict = new Dictionary<SlashCheckBaseAttribute, bool>();
                foreach (var att in attributes)
                {
                    var result = await att.ExecuteChecksAsync(ctx);
                    dict.Add(att, result);
                }

                if (dict.Any(x => x.Value == false))
                    throw new SlashExecutionChecksFailedException { FailedChecks = dict.Where(x => x.Value == false).Select(x => x.Key).ToList() };
            }
            if (context is ContextMenuContext CMctx)
            {
                var attributes = new List<ContextMenuCheckBaseAttribute>();
                attributes.AddRange(method.GetCustomAttributes<ContextMenuCheckBaseAttribute>(true));
                attributes.AddRange(method.DeclaringType.GetCustomAttributes<ContextMenuCheckBaseAttribute>());
                if (method.DeclaringType.DeclaringType != null)
                {
                    attributes.AddRange(method.DeclaringType.DeclaringType.GetCustomAttributes<ContextMenuCheckBaseAttribute>());
                    if (method.DeclaringType.DeclaringType.DeclaringType != null)
                    {
                        attributes.AddRange(method.DeclaringType.DeclaringType.DeclaringType.GetCustomAttributes<ContextMenuCheckBaseAttribute>());
                    }
                }

                var dict = new Dictionary<ContextMenuCheckBaseAttribute, bool>();
                foreach (var att in attributes)
                {
                    var result = await att.ExecuteChecksAsync(CMctx);
                    dict.Add(att, result);
                }

                if (dict.Any(x => x.Value == false))
                    throw new ContextMenuExecutionChecksFailedException { FailedChecks = dict.Where(x => x.Value == false).Select(x => x.Key).ToList() };
            }
        }

        private async Task<List<DiscordApplicationCommandOptionChoice>> GetChoiceAttributesFromProvider(IEnumerable<ChoiceProviderAttribute> customAttributes)
        {
            var choices = new List<DiscordApplicationCommandOptionChoice>();
            foreach (var choiceProviderAttribute in customAttributes)
            {
                var method = choiceProviderAttribute.ProviderType.GetMethod(nameof(IChoiceProvider.Provider));

                if (method == null)
                    throw new ArgumentException("ChoiceProviders must inherit from IChoiceProvider.");
                else
                {
                    var instance = Activator.CreateInstance(choiceProviderAttribute.ProviderType);
                    var result = await (Task<IEnumerable<DiscordApplicationCommandOptionChoice>>)method.Invoke(instance, null);

                    if (result.Any())
                    {
                        choices.AddRange(result);
                    }
                }
            }

            return choices;
        }

        private static List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromEnumParameter(Type enumParam)
        {
            var choices = new List<DiscordApplicationCommandOptionChoice>();
            foreach (Enum foo in Enum.GetValues(enumParam))
            {
                choices.Add(new DiscordApplicationCommandOptionChoice(foo.GetName(), foo.ToString()));
            }
            return choices;
        }

        private ApplicationCommandOptionType GetParameterType(Type type)
        {
            ApplicationCommandOptionType parametertype;
            if (ReferenceEquals(type, typeof(string)))
                parametertype = ApplicationCommandOptionType.String;
            else if (ReferenceEquals(type, typeof(long)) || ReferenceEquals(type, typeof(long?)))
                parametertype = ApplicationCommandOptionType.Integer;
            else if (ReferenceEquals(type, typeof(bool)) || ReferenceEquals(type, typeof(bool?)))
                parametertype = ApplicationCommandOptionType.Boolean;
            else if (ReferenceEquals(type, typeof(double)) || ReferenceEquals(type, typeof(double?)))
                parametertype = ApplicationCommandOptionType.Number;
            else if (ReferenceEquals(type, typeof(DiscordChannel)))
                parametertype = ApplicationCommandOptionType.Channel;
            else if (ReferenceEquals(type, typeof(DiscordUser)))
                parametertype = ApplicationCommandOptionType.User;
            else if (ReferenceEquals(type, typeof(DiscordRole)))
                parametertype = ApplicationCommandOptionType.Role;
            else if (ReferenceEquals(type, typeof(SnowflakeObject)))
                parametertype = ApplicationCommandOptionType.Mentionable;
            else if (type.IsEnum)
                parametertype = ApplicationCommandOptionType.String;

            else
                throw new ArgumentException("Cannot convert type! Argument types must be string, long, bool, double, DiscordChannel, DiscordUser, DiscordRole, SnowflakeObject or an Enum.");

            return parametertype;
        }

        private List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromParameter(IEnumerable<ChoiceAttribute> choiceattributes)
        {
            if (!choiceattributes.Any())
            {
                return null;
            }

            return choiceattributes.Select(att => new DiscordApplicationCommandOptionChoice(att.Name, att.Value)).ToList();
        }

        private async Task<List<DiscordApplicationCommandOption>> ParseParameters(ParameterInfo[] parameters)
        {
            var options = new List<DiscordApplicationCommandOption>();
            foreach (var parameter in parameters)
            {
                var optionattribute = parameter.GetCustomAttribute<OptionAttribute>();
                if (optionattribute == null)
                    throw new ArgumentException("Arguments must have the Option attribute!");

                var type = parameter.ParameterType;
                var parametertype = GetParameterType(type);

                var choices = GetChoiceAttributesFromParameter(parameter.GetCustomAttributes<ChoiceAttribute>());

                if (parameter.ParameterType.IsEnum)
                {
                    choices = GetChoiceAttributesFromEnumParameter(parameter.ParameterType);
                }

                var choiceProviders = parameter.GetCustomAttributes<ChoiceProviderAttribute>();

                if (choiceProviders.Any())
                {
                    choices = await GetChoiceAttributesFromProvider(choiceProviders);
                }

                options.Add(new DiscordApplicationCommandOption(optionattribute.Name, optionattribute.Description, parametertype, !parameter.IsOptional, choices));
            }

            return options;
        }

        /// <summary>
        /// <para>Refreshes your commands, used for refreshing choice providers or applying commands registered after the ready event on the discord client. 
        /// Should only be run on the slash command extension linked to shard 0 if sharding.</para>
        /// <para>Not recommended and should be avoided since it can make slash commands be unresponsive for a while.</para>
        /// </summary>
        public async Task RefreshCommands()
        {
            _commandMethods.Clear();
            _groupCommands.Clear();
            _subGroupCommands.Clear();
            _registeredCommands.Clear();

            await Update();
        }

        /// <summary>
        /// Fires when the execution of a slash command fails.
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandErrorEventArgs> SlashCommandErrored
        {
            add { _slashError.Register(value); }
            remove { _slashError.Unregister(value); }
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs> _slashError;

        /// <summary>
        /// Fires when the execution of a slash command is successful.
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandExecutedEventArgs> SlashCommandExecuted
        {
            add { _slashExecuted.Register(value); }
            remove { _slashExecuted.Unregister(value); }
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs> _slashExecuted;

        /// <summary>
        /// Fires when the execution of a context menu fails.
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, ContextMenuErrorEventArgs> ContextMenuErrored
        {
            add { _contextMenuErrored.Register(value); }
            remove { _contextMenuErrored.Unregister(value); }
        }
        private AsyncEvent<SlashCommandsExtension, ContextMenuErrorEventArgs> _contextMenuErrored;

        /// <summary>
        /// Fire when the execution of a context menu is successful.
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, ContextMenuExecutedEventArgs> ContextMenuExecuted
        {
            add { _contextMenuExecuted.Register(value); }
            remove { _contextMenuExecuted.Unregister(value); }
        }
        private AsyncEvent<SlashCommandsExtension, ContextMenuExecutedEventArgs> _contextMenuExecuted;
    }

    internal class CommandMethod
    {
        public ulong CommandId { get; set; }
        public string Name { get; set; }
        public MethodInfo Method { get; set; }
    }

    internal class GroupCommand
    {
        public ulong CommandId { get; set; }
        public string Name { get; set; }
        public List<KeyValuePair<string, MethodInfo>> Methods { get; set; } = null;
    }

    internal class SubGroupCommand
    {
        public ulong CommandId { get; set; }
        public string Name { get; set; }
        public List<GroupCommand> SubCommands { get; set; } = new List<GroupCommand>();
    }

    internal class ContextMenuCommand
    {
        public ulong CommandId { get; set; }
        public string Name { get; set; }
        public MethodInfo Method { get; set; }
    }
}
