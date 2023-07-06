using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands.EventArgs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// A class that handles slash commands for a client.
    /// </summary>
    public sealed class SlashCommandsExtension : BaseExtension
    {
        //A list of methods for top level commands
        private static List<CommandMethod> _commandMethods { get; set; } = new();
        //List of groups
        private static List<GroupCommand> _groupCommands { get; set; } = new();
        //List of groups with subgroups
        private static List<SubGroupCommand> _subGroupCommands { get; set; } = new();
        //List of context menus
        private static List<ContextMenuCommand> _contextMenuCommands { get; set; } = new();

        //Singleton modules
        private static List<object> _singletonModules { get; set; } = new();

        //List of modules to register
        private List<KeyValuePair<ulong?, Type>> _updateList { get; set; } = new();
        //Configuration for DI
        private readonly SlashCommandsConfiguration _configuration;
        //Set to true if anything fails when registering
        private static bool _errored { get; set; } = false;

        /// <summary>
        /// Gets a list of registered commands. The key is the guild id (null if global).
        /// </summary>
        public IReadOnlyList<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> RegisteredCommands => _registeredCommands;
        private static readonly List<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> _registeredCommands = new();

        internal SlashCommandsExtension(SlashCommandsConfiguration configuration)
        {
            this._configuration = configuration ?? new SlashCommandsConfiguration();;
        }

        /// <summary>
        /// Runs setup. DO NOT RUN THIS MANUALLY. DO NOT DO ANYTHING WITH THIS.
        /// </summary>
        /// <param name="client">The client to setup on.</param>
        protected internal override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = client;

            this._slashError = new AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs>("SLASHCOMMAND_ERRORED", this.Client.EventErrorHandler);
            this._slashInvoked = new AsyncEvent<SlashCommandsExtension, SlashCommandInvokedEventArgs>("SLASHCOMMAND_RECEIVED", this.Client.EventErrorHandler);
            this._slashExecuted = new AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs>("SLASHCOMMAND_EXECUTED", this.Client.EventErrorHandler);
            this._contextMenuErrored = new AsyncEvent<SlashCommandsExtension, ContextMenuErrorEventArgs>("CONTEXTMENU_ERRORED", this.Client.EventErrorHandler);
            this._contextMenuExecuted = new AsyncEvent<SlashCommandsExtension, ContextMenuExecutedEventArgs>("CONTEXTMENU_EXECUTED", this.Client.EventErrorHandler);
            this._contextMenuInvoked = new AsyncEvent<SlashCommandsExtension, ContextMenuInvokedEventArgs>("CONTEXTMENU_RECEIVED", this.Client.EventErrorHandler);
            this._autocompleteErrored = new AsyncEvent<SlashCommandsExtension, AutocompleteErrorEventArgs>("AUTOCOMPLETE_ERRORED", this.Client.EventErrorHandler);
            this._autocompleteExecuted = new AsyncEvent<SlashCommandsExtension, AutocompleteExecutedEventArgs>("AUTOCOMPLETE_EXECUTED", this.Client.EventErrorHandler);

            this.Client.SessionCreated += this.Update;
            this.Client.InteractionCreated += this.InteractionHandler;
            this.Client.ContextMenuInteractionCreated += this.ContextMenuHandler;
        }

        /// <summary>
        /// Registers a command class.
        /// </summary>
        /// <typeparam name="T">The command class to register.</typeparam>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands<T>(ulong? guildId = null) where T : ApplicationCommandModule
        {
            if (this.Client.ShardId is 0)
                this._updateList.Add(new(guildId, typeof(T)));
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
            //If sharding, only register for shard 0
            if (this.Client.ShardId is 0)
                this._updateList.Add(new(guildId, type));
        }

        /// <summary>
        /// Registers all command classes from a given assembly.
        /// </summary>
        /// <param name="assembly">Assembly to register command classes from.</param>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands(Assembly assembly, ulong? guildId = null)
        {
            var types = assembly.ExportedTypes.Where(xt =>
                typeof(ApplicationCommandModule).IsAssignableFrom(xt) &&
                !xt.GetTypeInfo().IsNested);

            foreach (var xt in types)
                this.RegisterCommands(xt, guildId);
        }

        //To be run on ready
        internal Task Update(DiscordClient client, SessionReadyEventArgs e) => this.Update();

        //Actual method for registering, used for RegisterCommands and on Ready
        internal Task Update()
        {
            //Only update for shard 0
            if (this.Client.ShardId is 0)
            {
                //Groups commands by guild id or global
                foreach (var key in this._updateList.Select(x => x.Key).Distinct())
                {
                    this.RegisterCommands(this._updateList.Where(x => x.Key == key).Select(x => x.Value), key);
                }
            }
            return Task.CompletedTask;
        }

        #region Registering

        //Method for registering commands for a target from modules
        private void RegisterCommands(IEnumerable<Type> types, ulong? guildId)
        {
            //Initialize empty lists to be added to the global ones at the end
            var commandMethods = new List<CommandMethod>();
            var groupCommands = new List<GroupCommand>();
            var subGroupCommands = new List<SubGroupCommand>();
            var contextMenuCommands = new List<ContextMenuCommand>();
            var updateList = new List<DiscordApplicationCommand>();

            _ = Task.Run(async () =>
            {
                //Iterates over all the modules
                foreach (var type in types)
                {
                    try
                    {
                        var module = type.GetTypeInfo();
                        var classes = new List<TypeInfo>();

                        //Add module to classes list if it's a group
                        if (module.GetCustomAttribute<SlashCommandGroupAttribute>() != null)
                        {
                            classes.Add(module);
                        }
                        else
                        {
                            //Otherwise add the nested groups
                            classes = module.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null).ToList();
                        }

                        //Handles groups
                        foreach (var subclassinfo in classes)
                        {
                            //Gets the attribute and methods in the group

                            var allowDMs = subclassinfo.GetCustomAttribute<GuildOnlyAttribute>() is null;
                            var v2Permissions = subclassinfo.GetCustomAttribute<SlashCommandPermissionsAttribute>()?.Permissions;

                            var groupAttribute = subclassinfo.GetCustomAttribute<SlashCommandGroupAttribute>();
                            var submethods = subclassinfo.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);
                            var subclasses = subclassinfo.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null);
                            if (subclasses.Any() && submethods.Any())
                            {
                                throw new ArgumentException("Slash command groups cannot have both subcommands and subgroups!");
                            }

                            //Group context menus
                            var contextMethods = subclassinfo.DeclaredMethods.Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null);
                            AddContextMenus(contextMethods);

                            //Initializes the command
                            var payload = new DiscordApplicationCommand(groupAttribute.Name, groupAttribute.Description, defaultPermission: groupAttribute.DefaultPermission, allowDMUsage: allowDMs, defaultMemberPermissions: v2Permissions, nsfw: groupAttribute.NSFW);

                            var commandmethods = new List<KeyValuePair<string, MethodInfo>>();
                            //Handles commands in the group
                            foreach (var submethod in submethods)
                            {
                                var commandAttribute = submethod.GetCustomAttribute<SlashCommandAttribute>();

                                //Gets the paramaters and accounts for InteractionContext
                                var parameters = submethod.GetParameters();
                                if (parameters?.Length is null or 0 || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                    throw new ArgumentException($"The first argument must be an InteractionContext!");
                                parameters = parameters.Skip(1).ToArray();

                                //Check if the ReturnType can be safely casted to a Task later on execution
                                if (!typeof(Task).IsAssignableFrom(submethod.ReturnType))
                                    throw new InvalidOperationException("The method has to return a Task or Task<> value");

                                var options = await this.ParseParameters(parameters, guildId);

                                var nameLocalizations = this.GetNameLocalizations(submethod);
                                var descriptionLocalizations = this.GetDescriptionLocalizations(submethod);

                                //Creates the subcommand and adds it to the main command
                                var subpayload = new DiscordApplicationCommandOption(commandAttribute.Name, commandAttribute.Description, ApplicationCommandOptionType.SubCommand, null, null, options, name_localizations: nameLocalizations, description_localizations: descriptionLocalizations);
                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission, allowDMUsage: allowDMs, defaultMemberPermissions: v2Permissions, nsfw: payload.NSFW);

                                //Adds it to the method lists
                                commandmethods.Add(new(commandAttribute.Name, submethod));
                                groupCommands.Add(new() { Name = groupAttribute.Name, Methods = commandmethods });
                            }

                            var command = new SubGroupCommand { Name = groupAttribute.Name };
                            //Handles subgroups
                            foreach (var subclass in subclasses)
                            {
                                var subGroupAttribute = subclass.GetCustomAttribute<SlashCommandGroupAttribute>();
                                //I couldn't think of more creative naming
                                var subsubmethods = subclass.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

                                var options = new List<DiscordApplicationCommandOption>();

                                var currentMethods = new List<KeyValuePair<string, MethodInfo>>();

                                //Similar to the one for regular groups
                                foreach (var subsubmethod in subsubmethods)
                                {
                                    var suboptions = new List<DiscordApplicationCommandOption>();
                                    var commatt = subsubmethod.GetCustomAttribute<SlashCommandAttribute>();
                                    var parameters = subsubmethod.GetParameters();
                                    if (parameters?.Length is null or 0 || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                        throw new ArgumentException($"The first argument must be an InteractionContext!");
                                    parameters = parameters.Skip(1).ToArray();
                                    suboptions = suboptions.Concat(await this.ParseParameters(parameters, guildId)).ToList();

                                    var nameLocalizations = this.GetNameLocalizations(subsubmethod);
                                    var descriptionLocalizations = this.GetDescriptionLocalizations(subsubmethod);

                                    var subsubpayload = new DiscordApplicationCommandOption(commatt.Name, commatt.Description, ApplicationCommandOptionType.SubCommand, null, null, suboptions, name_localizations: nameLocalizations, description_localizations: descriptionLocalizations);
                                    options.Add(subsubpayload);


                                    commandmethods.Add(new(commatt.Name, subsubmethod));
                                    currentMethods.Add(new(commatt.Name, subsubmethod));
                                }

                                //Subgroups Context Menus
                                var subContextMethods = subclass.DeclaredMethods.Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null);
                                AddContextMenus(subContextMethods);

                                //Adds the group to the command and method lists
                                var subpayload = new DiscordApplicationCommandOption(subGroupAttribute.Name, subGroupAttribute.Description, ApplicationCommandOptionType.SubCommandGroup, null, null, options);
                                command.SubCommands.Add(new() { Name = subGroupAttribute.Name, Methods = currentMethods });
                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission, allowDMUsage: allowDMs, defaultMemberPermissions: v2Permissions, nsfw: payload.NSFW);

                                //Accounts for lifespans for the sub group
                                if (subclass.GetCustomAttribute<SlashModuleLifespanAttribute>() is not null and { Lifespan: SlashModuleLifespan.Singleton })
                                {
                                    _singletonModules.Add(this.CreateInstance(subclass, this._configuration?.Services));
                                }
                            }

                            if (command.SubCommands.Any())
                                subGroupCommands.Add(command);

                            updateList.Add(payload);

                            //Accounts for lifespans
                            if (subclassinfo.GetCustomAttribute<SlashModuleLifespanAttribute>() is not null and { Lifespan: SlashModuleLifespan.Singleton })
                            {
                                _singletonModules.Add(this.CreateInstance(subclassinfo, this._configuration?.Services));
                            }
                        }

                        //Handles methods, only if the module isn't a group itself
                        if (module.GetCustomAttribute<SlashCommandGroupAttribute>() is null)
                        {
                            //Slash commands (again, similar to the one for groups)
                            var methods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

                            foreach (var method in methods)
                            {
                                var commandattribute = method.GetCustomAttribute<SlashCommandAttribute>();

                                var parameters = method.GetParameters();
                                if (parameters?.Length is null or 0 || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(InteractionContext)))
                                    throw new ArgumentException($"The first argument must be an InteractionContext!");
                                parameters = parameters.Skip(1).ToArray();
                                var options = await this.ParseParameters(parameters, guildId);

                                commandMethods.Add(new() { Method = method, Name = commandattribute.Name });

                                var nameLocalizations = this.GetNameLocalizations(method);
                                var descriptionLocalizations = this.GetDescriptionLocalizations(method);

                                var allowDMs = (method.GetCustomAttribute<GuildOnlyAttribute>() ?? method.DeclaringType.GetCustomAttribute<GuildOnlyAttribute>()) is null;
                                var v2Permissions = (method.GetCustomAttribute<SlashCommandPermissionsAttribute>() ?? method.DeclaringType.GetCustomAttribute<SlashCommandPermissionsAttribute>())?.Permissions;

                                var payload = new DiscordApplicationCommand(commandattribute.Name, commandattribute.Description, options, commandattribute.DefaultPermission, name_localizations: nameLocalizations, description_localizations: descriptionLocalizations, allowDMUsage: allowDMs, defaultMemberPermissions: v2Permissions, nsfw:commandattribute.NSFW);
                                updateList.Add(payload);
                            }

                            //Context Menus
                            var contextMethods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null);
                            AddContextMenus(contextMethods);

                            //Accounts for lifespans
                            if (module.GetCustomAttribute<SlashModuleLifespanAttribute>() is not null and { Lifespan: SlashModuleLifespan.Singleton })
                            {
                                _singletonModules.Add(this.CreateInstance(module, this._configuration?.Services));
                            }
                        }

                        void AddContextMenus(IEnumerable<MethodInfo> contextMethods)
                        {
                            foreach (var contextMethod in contextMethods)
                            {
                                var contextAttribute = contextMethod.GetCustomAttribute<ContextMenuAttribute>();
                                var allowDMUsage = (contextMethod.GetCustomAttribute<GuildOnlyAttribute>() ?? contextMethod.DeclaringType.GetCustomAttribute<GuildOnlyAttribute>()) is null;
                                var permissions = (contextMethod.GetCustomAttribute<SlashCommandPermissionsAttribute>() ?? contextMethod.DeclaringType.GetCustomAttribute<SlashCommandPermissionsAttribute>())?.Permissions;
                                var command = new DiscordApplicationCommand(contextAttribute.Name, null, type: contextAttribute.Type, defaultPermission: contextAttribute.DefaultPermission, allowDMUsage: allowDMUsage, defaultMemberPermissions: permissions, nsfw: contextAttribute.NSFW);

                                var parameters = contextMethod.GetParameters();
                                if (parameters?.Length is null or 0 || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(ContextMenuContext)))
                                    throw new ArgumentException($"The first argument must be a ContextMenuContext!");
                                if (parameters.Length > 1)
                                    throw new ArgumentException($"A context menu cannot have parameters!");

                                contextMenuCommands.Add(new ContextMenuCommand { Method = contextMethod, Name = contextAttribute.Name });

                                updateList.Add(command);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //This isn't really much more descriptive but I added a separate case for it anyway
                        if (ex is BadRequestException brex)
                            this.Client.Logger.LogCritical(brex, $"There was an error registering application commands: {brex.JsonMessage}");
                        else
                            this.Client.Logger.LogCritical(ex, $"There was an error registering application commands");

                        _errored = true;
                    }
                }

                if (!_errored)
                {
                    try
                    {
                        IEnumerable<DiscordApplicationCommand> commands;
                        //Creates a guild command if a guild id is specified, otherwise global
                        commands = guildId is null
                            ? await this.Client.BulkOverwriteGlobalApplicationCommandsAsync(updateList)
                            : await this.Client.BulkOverwriteGuildApplicationCommandsAsync(guildId.Value, updateList);

                        //Checks against the ids and adds them to the command method lists
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
                        //Adds to the global lists finally
                        _commandMethods.AddRange(commandMethods);
                        _groupCommands.AddRange(groupCommands);
                        _subGroupCommands.AddRange(subGroupCommands);
                        _contextMenuCommands.AddRange(contextMenuCommands);

                        _registeredCommands.Add(new(guildId, commands.ToList()));
                    }
                    catch (Exception ex)
                    {
                        if (ex is BadRequestException brex)
                            this.Client.Logger.LogCritical(brex, $"There was an error registering application commands: {brex.JsonMessage}");
                        else
                            this.Client.Logger.LogCritical(ex, $"There was an error registering application commands");

                        _errored = true;
                    }
                }
            });
        }

        //Handles the parameters for a slash command
        private async Task<List<DiscordApplicationCommandOption>> ParseParameters(ParameterInfo[] parameters, ulong? guildId)
        {
            var options = new List<DiscordApplicationCommandOption>();
            foreach (var parameter in parameters)
            {
                //Gets the attribute
                var optionattribute = parameter.GetCustomAttribute<OptionAttribute>();
                if (optionattribute == null)
                    throw new ArgumentException("Arguments must have the Option attribute!");

                //Sets the type
                var type = parameter.ParameterType;
                var commandName = parameter.Member.GetCustomAttribute<SlashCommandAttribute>()?.Name ?? parameter.Member.GetCustomAttribute<ContextMenuAttribute>().Name;
                var parametertype = this.GetParameterType(commandName, type);

                //Handles choices
                //From attributes
                var choices = this.GetChoiceAttributesFromParameter(parameter.GetCustomAttributes<ChoiceAttribute>());
                //From enums
                if (parameter.ParameterType.IsEnum || Nullable.GetUnderlyingType(parameter.ParameterType)?.IsEnum == true)
                {
                    choices = GetChoiceAttributesFromEnumParameter(parameter.ParameterType);
                }
                //From choice provider
                var choiceProviders = parameter.GetCustomAttributes<ChoiceProviderAttribute>();
                if (choiceProviders.Any())
                {
                    choices = await this.GetChoiceAttributesFromProvider(choiceProviders, guildId);
                }

                var channelTypes = parameter.GetCustomAttribute<ChannelTypesAttribute>()?.ChannelTypes ?? null;

                var minimumValue = parameter.GetCustomAttribute<MinimumAttribute>()?.Value ?? null;
                var maximumValue = parameter.GetCustomAttribute<MaximumAttribute>()?.Value ?? null;

                var minimumLength = parameter.GetCustomAttribute<MinimumLengthAttribute>()?.Value ?? null;
                var maximumLength = parameter.GetCustomAttribute<MaximumLengthAttribute>()?.Value ?? null;

                var nameLocalizations = this.GetNameLocalizations(parameter);
                var descriptionLocalizations = this.GetDescriptionLocalizations(parameter);

                var autocompleteAttribute = parameter.GetCustomAttribute<AutocompleteAttribute>();
                if (autocompleteAttribute != null && autocompleteAttribute.Provider.GetMethod(nameof(IAutocompleteProvider.Provider)) == null)
                    throw new ArgumentException("Autocomplete providers must inherit from IAutocompleteProvider.");

                options.Add(new DiscordApplicationCommandOption(optionattribute.Name, optionattribute.Description, parametertype, !parameter.IsOptional, choices, null, channelTypes, (autocompleteAttribute != null || optionattribute.Autocomplete), minimumValue, maximumValue, nameLocalizations, descriptionLocalizations, minimumLength, maximumLength));
            }

            return options;
        }

        private IReadOnlyDictionary<string, string> GetNameLocalizations(ICustomAttributeProvider method)
        {
            var nameAttributes = (NameLocalizationAttribute[])method.GetCustomAttributes(typeof(NameLocalizationAttribute), false);
            return nameAttributes.ToDictionary(nameAttribute => nameAttribute.Locale, nameAttribute => nameAttribute.Name);
        }

        private IReadOnlyDictionary<string, string> GetDescriptionLocalizations(ICustomAttributeProvider method)
        {
            var descriptionAttributes = (DescriptionLocalizationAttribute[])method.GetCustomAttributes(typeof(DescriptionLocalizationAttribute), false);
            return descriptionAttributes.ToDictionary(descriptionAttribute => descriptionAttribute.Locale, descriptionAttribute => descriptionAttribute.Description);
        }


        //Gets the choices from a choice provider
        private async Task<List<DiscordApplicationCommandOptionChoice>> GetChoiceAttributesFromProvider(
            IEnumerable<ChoiceProviderAttribute> customAttributes,
            ulong? guildId
        )
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

                    // Abstract class offers more properties that can be set
                    if (choiceProviderAttribute.ProviderType.IsSubclassOf(typeof(ChoiceProvider)))
                    {
                        choiceProviderAttribute.ProviderType.GetProperty(nameof(ChoiceProvider.GuildId))
                            ?.SetValue(instance, guildId);

                        choiceProviderAttribute.ProviderType.GetProperty(nameof(ChoiceProvider.Services))
                            ?.SetValue(instance, this._configuration.Services);
                    }

                    //Gets the choices from the method
                    var result = await (Task<IEnumerable<DiscordApplicationCommandOptionChoice>>)method.Invoke(instance, null);

                    if (result.Any())
                    {
                        choices.AddRange(result);
                    }
                }
            }

            return choices;
        }

        //Gets choices from an enum
        private static List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromEnumParameter(Type enumParam)
        {
            var choices = new List<DiscordApplicationCommandOptionChoice>();
            if (enumParam.IsGenericType && enumParam.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                enumParam = Nullable.GetUnderlyingType(enumParam);
            }
            foreach (Enum enumValue in Enum.GetValues(enumParam))
            {
                choices.Add(new DiscordApplicationCommandOptionChoice(enumValue.GetName(), enumValue.ToString()));
            }
            return choices;
        }

        //Small method to get the parameter's type from its type
        private ApplicationCommandOptionType GetParameterType(string commandName, Type type)
        {
            if (type == typeof(string))
                return ApplicationCommandOptionType.String;
            if (type == typeof(long) || type == typeof(long?))
                return ApplicationCommandOptionType.Integer;
            if (type == typeof(bool) || type == typeof(bool?))
                return ApplicationCommandOptionType.Boolean;
            if (type == typeof(double) || type == typeof(double?))
                return ApplicationCommandOptionType.Number;
            if (type == typeof(DiscordChannel))
                return ApplicationCommandOptionType.Channel;
            if (type == typeof(DiscordUser))
                return ApplicationCommandOptionType.User;
            if (type == typeof(DiscordRole))
                return ApplicationCommandOptionType.Role;
            if (type == typeof(DiscordEmoji))
                return ApplicationCommandOptionType.String;
            if (type == typeof(TimeSpan?))
                return ApplicationCommandOptionType.String;
            if (type == typeof(SnowflakeObject))
                return ApplicationCommandOptionType.Mentionable;
            if (type.IsEnum || Nullable.GetUnderlyingType(type)?.IsEnum == true)
                return ApplicationCommandOptionType.String;
            if (type == typeof(DiscordAttachment))
                return ApplicationCommandOptionType.Attachment;
            throw new ArgumentException($"Cannot convert type! (Command: {commandName}) Argument types must be string, long, bool, double, TimeSpan?, DiscordChannel, DiscordUser, DiscordRole, DiscordEmoji, DiscordAttachment, SnowflakeObject, or an Enum.");
        }

        //Gets choices from choice attributes
        private List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromParameter(IEnumerable<ChoiceAttribute> choiceattributes)
        {
            if (!choiceattributes.Any())
            {
                return null;
            }

            return choiceattributes.Select(att => new DiscordApplicationCommandOptionChoice(att.Name, att.Value)).ToList();
        }

        #endregion

        #region Handling

        private Task InteractionHandler(DiscordClient client, InteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Interaction.Type == InteractionType.ApplicationCommand)
                {
                    var qualifiedName = new StringBuilder(e.Interaction.Data.Name);
                    var options = e.Interaction.Data.Options?.ToArray() ?? Array.Empty<DiscordInteractionDataOption>();
                    while (options.Any())
                    {
                        var firstOption = options[0];
                        if (firstOption.Type is not ApplicationCommandOptionType.SubCommandGroup and not ApplicationCommandOptionType.SubCommand)
                        {
                            break;
                        }

                        _ = qualifiedName.AppendFormat(" {0}", firstOption.Name);
                        options = firstOption.Options?.ToArray() ?? Array.Empty<DiscordInteractionDataOption>();
                    }

                    //Creates the context
                    var context = new InteractionContext
                    {
                        Interaction = e.Interaction,
                        Channel = e.Interaction.Channel,
                        Guild = e.Interaction.Guild,
                        User = e.Interaction.User,
                        Client = client,
                        SlashCommandsExtension = this,
                        CommandName = e.Interaction.Data.Name,
                        QualifiedName = qualifiedName.ToString(),
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

                        //Gets the method for the command
                        var methods = _commandMethods.Where(x => x.CommandId == e.Interaction.Data.Id);
                        var groups = _groupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                        var subgroups = _subGroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                        if (!methods.Any() && !groups.Any() && !subgroups.Any())
                            throw new InvalidOperationException("A slash command was executed, but no command was registered for it.");

                        //Just read the code you'll get it
                        if (methods.Any())
                        {
                            var method = methods.First().Method;

                            var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options);

                            await this.RunCommandAsync(context, method, args);
                        }
                        else if (groups.Any())
                        {
                            var command = e.Interaction.Data.Options.First();
                            var method = groups.First().Methods.First(x => x.Key == command.Name).Value;

                            var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options.First().Options);

                            await this.RunCommandAsync(context, method, args);
                        }
                        else if (subgroups.Any())
                        {
                            var command = e.Interaction.Data.Options.First();
                            var group = subgroups.First().SubCommands.First(x => x.Name == command.Name);
                            var method = group.Methods.First(x => x.Key == command.Options.First().Name).Value;

                            var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options.First().Options.First().Options);

                            await this.RunCommandAsync(context, method, args);
                        }

                        await this._slashExecuted.InvokeAsync(this, new SlashCommandExecutedEventArgs { Context = context });
                    }
                    catch (Exception ex)
                    {
                        await this._slashError.InvokeAsync(this, new SlashCommandErrorEventArgs { Context = context, Exception = ex });
                    }
                }

                //Handles autcomplete interactions
                if (e.Interaction.Type == InteractionType.AutoComplete)
                {
                    if (_errored)
                        throw new InvalidOperationException("Slash commands failed to register properly on startup.");

                    //Gets the method for the command
                    var methods = _commandMethods.Where(x => x.CommandId == e.Interaction.Data.Id);
                    var groups = _groupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                    var subgroups = _subGroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                    if (!methods.Any() && !groups.Any() && !subgroups.Any())
                        throw new InvalidOperationException("An autocomplete interaction was created, but no command was registered for it.");

                    if (methods.Any())
                    {
                        var method = methods.First().Method;

                        var options = e.Interaction.Data.Options;
                        //Gets the focused option
                        var focusedOption = options.First(o => o.Focused);
                        var parameter = method.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
                        await this.RunAutocomplete(e.Interaction, parameter, options, focusedOption);
                    }

                    if (groups.Any())
                    {
                        var command = e.Interaction.Data.Options.First();
                        var method = groups.First().Methods.First(x => x.Key == command.Name).Value;

                        var options = command.Options;
                        var focusedOption = options.First(o => o.Focused);
                        var parameter = method.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
                        await this.RunAutocomplete(e.Interaction, parameter, options, focusedOption);
                    }

                    if (subgroups.Any())
                    {
                        var command = e.Interaction.Data.Options.First();
                        var group = subgroups.First().SubCommands.First(x => x.Name == command.Name);
                        var method = group.Methods.First(x => x.Key == command.Options.First().Name).Value;

                        var options = command.Options.First().Options;
                        var focusedOption = options.First(o => o.Focused);
                        var parameter = method.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
                        await this.RunAutocomplete(e.Interaction, parameter, options, focusedOption);
                    }
                }
            });
            return Task.CompletedTask;
        }

        private Task ContextMenuHandler(DiscordClient client, ContextMenuInteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                //Creates the context
                var context = new ContextMenuContext
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

                if (e.Interaction.Guild != null && e.TargetUser != null && e.Interaction.Guild.Members.TryGetValue(e.TargetUser.Id, out var member))
                {
                    context.TargetMember = member;
                }

                try
                {
                    if (_errored)
                        throw new InvalidOperationException("Context menus failed to register properly on startup.");

                    //Gets the method for the command
                    var method = _contextMenuCommands.FirstOrDefault(x => x.CommandId == e.Interaction.Data.Id);

                    if (method == null)
                        throw new InvalidOperationException("A context menu was executed, but no command was registered for it.");

                    await this.RunCommandAsync(context, method.Method, new[] { context });

                    await this._contextMenuExecuted.InvokeAsync(this, new ContextMenuExecutedEventArgs { Context = context });
                }
                catch (Exception ex)
                {
                    await this._contextMenuErrored.InvokeAsync(this, new ContextMenuErrorEventArgs { Context = context, Exception = ex });
                }
            });

            return Task.CompletedTask;
        }

        internal async Task RunCommandAsync(BaseContext context, MethodInfo method, IEnumerable<object> args)
        {

            //Accounts for lifespans
            var moduleLifespan = (method.DeclaringType.GetCustomAttribute<SlashModuleLifespanAttribute>() != null ? method.DeclaringType.GetCustomAttribute<SlashModuleLifespanAttribute>()?.Lifespan : SlashModuleLifespan.Transient) ?? SlashModuleLifespan.Transient;
            var classInstance = moduleLifespan switch //Accounts for static methods and adds DI
            {
                // Accounts for static methods and adds DI
                SlashModuleLifespan.Scoped => method.IsStatic ? ActivatorUtilities.CreateInstance(this._configuration?.Services.CreateScope().ServiceProvider, method.DeclaringType) : this.CreateInstance(method.DeclaringType, this._configuration?.Services.CreateScope().ServiceProvider),
                // Accounts for static methods and adds DI
                SlashModuleLifespan.Transient => method.IsStatic ? ActivatorUtilities.CreateInstance(this._configuration?.Services, method.DeclaringType) : this.CreateInstance(method.DeclaringType, this._configuration?.Services),
                // If singleton, gets it from the singleton list
                SlashModuleLifespan.Singleton => _singletonModules.First(x => ReferenceEquals(x.GetType(), method.DeclaringType)),
                // TODO: Use a more specific exception type
                _ => throw new Exception($"An unknown {nameof(SlashModuleLifespanAttribute)} scope was specified on command {context.CommandName}"),
            };

            ApplicationCommandModule module = null;
            if (classInstance is ApplicationCommandModule mod)
                module = mod;

            //Slash commands
            if (context is InteractionContext slashContext)
            {
                await this._slashInvoked.InvokeAsync(this, new SlashCommandInvokedEventArgs { Context = slashContext });

                await this.RunPreexecutionChecksAsync(method, slashContext);

                //Runs BeforeExecution and accounts for groups that don't inherit from ApplicationCommandModule
                var shouldExecute = await (module?.BeforeSlashExecutionAsync(slashContext) ?? Task.FromResult(true));

                if (shouldExecute)
                {
                    await (Task)method.Invoke(classInstance, args.ToArray());

                    //Runs AfterExecution and accounts for groups that don't inherit from ApplicationCommandModule
                    await (module?.AfterSlashExecutionAsync(slashContext) ?? Task.CompletedTask);
                }
            }
            //Context menus
            if (context is ContextMenuContext CMContext)
            {
                await this._contextMenuInvoked.InvokeAsync(this, new ContextMenuInvokedEventArgs() { Context = CMContext });

                await this.RunPreexecutionChecksAsync(method, CMContext);

                //This null check actually shouldn't be necessary for context menus but I'll keep it in just in case
                var shouldExecute = await (module?.BeforeContextMenuExecutionAsync(CMContext) ?? Task.FromResult(true));

                if (shouldExecute)
                {
                    await (Task)method.Invoke(classInstance, args.ToArray());

                    await (module?.AfterContextMenuExecutionAsync(CMContext) ?? Task.CompletedTask);
                }
            }
        }

        //Property injection copied over from CommandsNext
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

        //Parses slash command parameters
        private async Task<List<object>> ResolveInteractionCommandParameters(InteractionCreateEventArgs e, InteractionContext context, MethodInfo method, IEnumerable<DiscordInteractionDataOption> options)
        {
            var args = new List<object> { context };
            var parameters = method.GetParameters().Skip(1);

            for (var i = 0; i < parameters.Count(); i++)
            {
                var parameter = parameters.ElementAt(i);

                //Accounts for optional arguments without values given
                if (parameter.IsOptional && (!options?.Any(x =>
                        x.Name.Equals(parameter.GetCustomAttribute<OptionAttribute>().Name, StringComparison.InvariantCultureIgnoreCase)) ?? true))
                    args.Add(parameter.DefaultValue);
                else
                {
                    var option = options.Single(x =>
                        x.Name.Equals(parameter.GetCustomAttribute<OptionAttribute>().Name, StringComparison.InvariantCultureIgnoreCase));

                    //Checks the type and casts/references resolved and adds the value to the list
                    //This can probably reference the slash command's type property that didn't exist when I wrote this and it could use a cleaner switch instead, but if it works it works
                    if (parameter.ParameterType == typeof(string))
                        args.Add(option.Value.ToString());
                    else if (parameter.ParameterType.IsEnum)
                        args.Add(Enum.Parse(parameter.ParameterType, (string)option.Value));
                    else if (Nullable.GetUnderlyingType(parameter.ParameterType)?.IsEnum == true)
                        args.Add(Enum.Parse(Nullable.GetUnderlyingType(parameter.ParameterType), (string)option.Value));
                    else if (parameter.ParameterType == typeof(long) || parameter.ParameterType == typeof(long?))
                        args.Add((long?)option.Value);
                    else if (parameter.ParameterType == typeof(bool) || parameter.ParameterType == typeof(bool?))
                        args.Add((bool?)option.Value);
                    else if (parameter.ParameterType == typeof(double) || parameter.ParameterType == typeof(double?))
                        args.Add((double?)option.Value);
                    else if (parameter.ParameterType == typeof(TimeSpan?))
                    {
                        var timeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript);
                        var value = option.Value.ToString();
                        if (value == "0")
                        {
                            args.Add(TimeSpan.Zero);
                            continue;
                        }
                        if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                        {
                            args.Add(null);
                            continue;
                        }
                        value = value.ToLowerInvariant();

                        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
                        {
                            args.Add(result);
                            continue;
                        }
                        var gps = new string[] { "days", "hours", "minutes", "seconds" };
                        var mtc = timeSpanRegex.Match(value);
                        if (!mtc.Success)
                        {
                            args.Add(null);
                            continue;
                        }

                        var d = 0;
                        var h = 0;
                        var m = 0;
                        var s = 0;
                        foreach (var gp in gps)
                        {
                            var gpc = mtc.Groups[gp].Value;
                            if (string.IsNullOrWhiteSpace(gpc))
                                continue;
                            gpc = gpc.Trim();

                            var gpt = gpc[gpc.Length - 1];
                            int.TryParse(gpc.Substring(0, gpc.Length - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var val);
                            switch (gpt)
                            {
                                case 'd':
                                    d = val;
                                    break;

                                case 'h':
                                    h = val;
                                    break;

                                case 'm':
                                    m = val;
                                    break;

                                case 's':
                                    s = val;
                                    break;
                            }
                        }
                        result = new TimeSpan(d, h, m, s);
                        args.Add(result);
                    }
                    else if (parameter.ParameterType == typeof(DiscordUser))
                    {
                        //Checks through resolved
                        if (e.Interaction.Data.Resolved.Members != null &&
                            e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                            args.Add(member);
                        else if (e.Interaction.Data.Resolved.Users != null &&
                                 e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
                            args.Add(user);
                        else
                            args.Add(await this.Client.GetUserAsync((ulong)option.Value));
                    }
                    else if (parameter.ParameterType == typeof(DiscordChannel))
                    {
                        //Checks through resolved
                        if (e.Interaction.Data.Resolved.Channels != null &&
                            e.Interaction.Data.Resolved.Channels.TryGetValue((ulong)option.Value, out var channel))
                            args.Add(channel);
                        else
                            args.Add(e.Interaction.Guild.GetChannel((ulong)option.Value));
                    }
                    else if (parameter.ParameterType == typeof(DiscordRole))
                    {
                        //Checks through resolved
                        if (e.Interaction.Data.Resolved.Roles != null &&
                            e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                            args.Add(role);
                        else
                            args.Add(e.Interaction.Guild.GetRole((ulong)option.Value));
                    }
                    else if (parameter.ParameterType == typeof(SnowflakeObject))
                    {
                        //Checks through resolved
                        if (e.Interaction.Data.Resolved.Roles != null && e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                            args.Add(role);
                        else if (e.Interaction.Data.Resolved.Members != null && e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                            args.Add(member);
                        else if (e.Interaction.Data.Resolved.Users != null && e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
                            args.Add(user);
                        else
                            throw new ArgumentException("Error resolving mentionable option.");
                    }
                    else if (parameter.ParameterType == typeof(DiscordEmoji))
                    {
                        var value = option.Value.ToString();

                        if (DiscordEmoji.TryFromUnicode(this.Client, value, out var emoji) || DiscordEmoji.TryFromName(this.Client, value, out emoji))
                            args.Add(emoji);
                        else
                            throw new ArgumentException("Error parsing emoji parameter.");
                    }
                    else if (parameter.ParameterType == typeof(DiscordAttachment))
                    {
                        if (e.Interaction.Data.Resolved.Attachments?.ContainsKey((ulong)option.Value) ?? false)
                        {
                            var attachment = e.Interaction.Data.Resolved.Attachments[(ulong)option.Value];
                            args.Add(attachment);
                        }
                        else
                            this.Client.Logger.LogError("Missing attachment in resolved data. This is an issue with Discord.");
                    }

                    else
                        throw new ArgumentException("Error resolving interaction.");
                }
            }

            return args;
        }

        //Runs pre-execution checks
        private async Task RunPreexecutionChecksAsync(MethodInfo method, BaseContext context)
        {
            if (context is InteractionContext ctx)
            {
                //Gets all attributes from parent classes as well and stuff
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
                    //Runs the check and adds the result to a list
                    var result = await att.ExecuteChecksAsync(ctx);
                    dict.Add(att, result);
                }

                //Checks if any failed, and throws an exception
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
                    //Runs the check and adds the result to a list
                    var result = await att.ExecuteChecksAsync(CMctx);
                    dict.Add(att, result);
                }

                //Checks if any failed, and throws an exception
                if (dict.Any(x => x.Value == false))
                    throw new ContextMenuExecutionChecksFailedException { FailedChecks = dict.Where(x => x.Value == false).Select(x => x.Key).ToList() };
            }
        }

        //Actually handles autocomplete interactions
        private async Task RunAutocomplete(DiscordInteraction interaction, ParameterInfo parameter, IEnumerable<DiscordInteractionDataOption> options, DiscordInteractionDataOption focusedOption)
        {
            var context = new AutocompleteContext
            {
                Interaction = interaction,
                Client = this.Client,
                Services = this._configuration?.Services,
                SlashCommandsExtension = this,
                Guild = interaction.Guild,
                Channel = interaction.Channel,
                User = interaction.User,
                Options = options.ToList(),
                FocusedOption = focusedOption
            };

            try
            {
                //Gets the provider
                var provider = parameter.GetCustomAttribute<AutocompleteAttribute>()?.Provider;
                if (provider == null) return;

                var providerMethod = provider.GetMethod(nameof(IAutocompleteProvider.Provider));
                var providerInstance = ActivatorUtilities.CreateInstance(this._configuration.Services, provider);

                var choices = await (Task<IEnumerable<DiscordAutoCompleteChoice>>) providerMethod.Invoke(providerInstance, new[] { context });
                await interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));

                await this._autocompleteExecuted.InvokeAsync(this,
                    new()
                    {
                        Context = context,
                        ProviderType = provider
                    });
            }
            catch (Exception ex)
            {
                await this._autocompleteErrored.InvokeAsync(this,
                    new AutocompleteErrorEventArgs()
                    {
                        Exception = ex,
                        Context = context,
                        ProviderType = parameter.GetCustomAttribute<AutocompleteAttribute>()?.Provider
                    });
            }
        }

        #endregion

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

            await this.Update();
        }

        /// <summary>
        /// Fires when the execution of a slash command fails.
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandErrorEventArgs> SlashCommandErrored
        {
            add => this._slashError.Register(value);
            remove => this._slashError.Unregister(value);
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs> _slashError;

        /// <summary>
        /// Fired when a slash command has been received and is to be executed
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandInvokedEventArgs> SlashCommandInvoked
        {
            add => this._slashInvoked.Register(value);
            remove => this._slashInvoked.Unregister(value);
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandInvokedEventArgs> _slashInvoked;

        /// <summary>
        /// Fires when the execution of a slash command is successful.
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandExecutedEventArgs> SlashCommandExecuted
        {
            add => this._slashExecuted.Register(value);
            remove => this._slashExecuted.Unregister(value);
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs> _slashExecuted;

        /// <summary>
        /// Fires when the execution of a context menu fails.
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, ContextMenuErrorEventArgs> ContextMenuErrored
        {
            add => this._contextMenuErrored.Register(value);
            remove => this._contextMenuErrored.Unregister(value);
        }
        private AsyncEvent<SlashCommandsExtension, ContextMenuErrorEventArgs> _contextMenuErrored;

        /// <summary>
        /// Fired when a context menu has been received and is to be executed
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, ContextMenuInvokedEventArgs> ContextMenuInvoked
        {
            add => this._contextMenuInvoked.Register(value);
            remove => this._contextMenuInvoked.Unregister(value);
        }
        private AsyncEvent<SlashCommandsExtension, ContextMenuInvokedEventArgs> _contextMenuInvoked;

        /// <summary>
        /// Fire when the execution of a context menu is successful.
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, ContextMenuExecutedEventArgs> ContextMenuExecuted
        {
            add => this._contextMenuExecuted.Register(value);
            remove => this._contextMenuExecuted.Unregister(value);
        }
        private AsyncEvent<SlashCommandsExtension, ContextMenuExecutedEventArgs> _contextMenuExecuted;

        public event AsyncEventHandler<SlashCommandsExtension, AutocompleteErrorEventArgs> AutocompleteErrored
        {
            add => this._autocompleteErrored.Register(value);
            remove => this._autocompleteErrored.Register(value);
        }
        private AsyncEvent<SlashCommandsExtension, AutocompleteErrorEventArgs> _autocompleteErrored;

        public event AsyncEventHandler<SlashCommandsExtension, AutocompleteExecutedEventArgs> AutocompleteExecuted
        {
            add => this._autocompleteExecuted.Register(value);
            remove => this._autocompleteExecuted.Register(value);
        }
        private AsyncEvent<SlashCommandsExtension, AutocompleteExecutedEventArgs> _autocompleteExecuted;


        public override void Dispose()
        {
            this._slashError?.UnregisterAll();
            this._slashInvoked?.UnregisterAll();
            this._slashExecuted?.UnregisterAll();
            this._contextMenuErrored?.UnregisterAll();
            this._contextMenuExecuted?.UnregisterAll();
            this._contextMenuInvoked?.UnregisterAll();
            this._autocompleteErrored?.UnregisterAll();
            this._autocompleteExecuted?.UnregisterAll();

            if (this.Client != null)
            {
                this.Client.SessionCreated -= this.Update;
                this.Client.InteractionCreated -= this.InteractionHandler;
                this.Client.ContextMenuInteractionCreated -= this.ContextMenuHandler;
            }

            // Satisfy rule CA1816. Can be removed if this class is sealed.
            GC.SuppressFinalize(this);
        }
    }

    //I'm not sure if creating separate classes is the cleanest thing here but I can't think of anything else so these stay

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
