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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
        /// <summary>
        /// Top level slash commands.
        /// </summary>
        private static List<CommandMethod> _commandMethods { get; set; } = new();

        /// <summary>
        /// Group commands, linked with the top level commands.
        /// </summary>
        private static List<GroupCommand> _groupCommands { get; set; } = new();

        /// <summary>
        /// Sub group commands, linked with the group commands.
        /// </summary>
        private static List<SubGroupCommand> _subGroupCommands { get; set; } = new();

        /// <summary>
        /// A list of context menu commands.
        /// </summary>
        private static List<ContextMenuCommand> _contextMenuCommands { get; set; } = new();

        /// <summary>
        /// A list of singleton modules from the provided service provider. In theory, this doesn't need to exist and can be removed if the service provider is used instead.
        /// </summary>
        private static List<object> _singletonModules { get; set; } = new();

        /// <summary>
        /// List of modules to register on startup/update. INTENTIONALLY not a dictionary because a dictionary cannot have null or duplicate keys (where the key represents guild ids or global commands).
        /// </summary>
        private Dictionary<ulong?, List<Type>> _updateList { get; set; } = new();

        /// <summary>
        /// The configuration for Slash Commands. Currently only contains an IServiceProvider for dependency injection (DI).
        /// </summary>
        private readonly SlashCommandsConfiguration _configuration;

        /// <summary>
        /// If slash commands errored on startup.
        /// </summary>
        /// <remarks>
        /// TODO: This should be removed. Instead, errors should be thrown on startup and the event listeners from <see cref="DiscordClient"/> should only be registered when the slash commands are successfully loaded.
        /// </remarks>
        private static bool _errored { get; set; } = false;

        /// <summary>
        /// A dictionary linking C# types to Discord's command option types.
        /// </summary>
        /// <remarks>
        /// TODO: Use this to add converter support later.
        /// </remarks>
        private static readonly ReadOnlyDictionary<Type, ApplicationCommandOptionType> _validOptionTypes = new(new Dictionary<Type, ApplicationCommandOptionType>()
        {
            [typeof(bool)] = ApplicationCommandOptionType.Boolean,
            [typeof(long)] = ApplicationCommandOptionType.Integer,
            [typeof(double)] = ApplicationCommandOptionType.Number,
            [typeof(string)] = ApplicationCommandOptionType.String,
            [typeof(TimeSpan)] = ApplicationCommandOptionType.String,
            [typeof(Enum)] = ApplicationCommandOptionType.String,
            [typeof(DiscordChannel)] = ApplicationCommandOptionType.Channel,
            [typeof(DiscordUser)] = ApplicationCommandOptionType.User,
            [typeof(DiscordRole)] = ApplicationCommandOptionType.Role,
            [typeof(DiscordEmoji)] = ApplicationCommandOptionType.String,
            [typeof(DiscordAttachment)] = ApplicationCommandOptionType.Attachment,
            [typeof(SnowflakeObject)] = ApplicationCommandOptionType.Mentionable
        });

        /// <summary>
        /// A list of registered application commands. The keys represent guild ids or global commands if the key is null.
        /// </summary>
        public IReadOnlyList<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> RegisteredCommands => _registeredCommands;

        /// <summary>
        /// INTENTIONALLY not a dictionary because a dictionary cannot have null or duplicate keys (where the key represents guild ids or global commands).
        /// </summary>
        private static readonly List<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> _registeredCommands = new();

        internal SlashCommandsExtension(SlashCommandsConfiguration configuration)
        {
            this._configuration = configuration;
        }

        /// <summary>
        /// Runs setup. DO NOT RUN THIS MANUALLY. DO NOT DO ANYTHING WITH THIS.
        /// </summary>
        /// <param name="client">The client to setup on.</param>
        protected internal override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("The Slash Commands extension has already been setup.");

            this.Client = client;

            this._slashError = new AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs>("SLASHCOMMAND_ERRORED", TimeSpan.Zero, this.Client.EventErrorHandler);
            this._slashInvoked = new AsyncEvent<SlashCommandsExtension, SlashCommandInvokedEventArgs>("SLASHCOMMAND_RECEIVED", TimeSpan.Zero, this.Client.EventErrorHandler);
            this._slashExecuted = new AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs>("SLASHCOMMAND_EXECUTED", TimeSpan.Zero, this.Client.EventErrorHandler);
            this._contextMenuErrored = new AsyncEvent<SlashCommandsExtension, ContextMenuErrorEventArgs>("CONTEXTMENU_ERRORED", TimeSpan.Zero, this.Client.EventErrorHandler);
            this._contextMenuExecuted = new AsyncEvent<SlashCommandsExtension, ContextMenuExecutedEventArgs>("CONTEXTMENU_EXECUTED", TimeSpan.Zero, this.Client.EventErrorHandler);
            this._contextMenuInvoked = new AsyncEvent<SlashCommandsExtension, ContextMenuInvokedEventArgs>("CONTEXTMENU_RECEIVED", TimeSpan.Zero, this.Client.EventErrorHandler);
            this._autocompleteErrored = new AsyncEvent<SlashCommandsExtension, AutocompleteErrorEventArgs>("AUTOCOMPLETE_ERRORED", TimeSpan.Zero, this.Client.EventErrorHandler);
            this._autocompleteExecuted = new AsyncEvent<SlashCommandsExtension, AutocompleteExecutedEventArgs>("AUTOCOMPLETE_EXECUTED", TimeSpan.Zero, this.Client.EventErrorHandler);

            // TODO: This should not be done on setup, but instead when slash commands are successfully loaded.
            this.Client.Ready += (_, _) => this.UpdateApplicationCommandsAsync();
            this.Client.InteractionCreated += this.InteractionHandler;
            this.Client.ContextMenuInteractionCreated += this.ContextMenuHandler;
        }

        /// <summary>
        /// Makes the Discord rest request that updates application commands.
        /// </summary>
        internal Task UpdateApplicationCommandsAsync()
        {
            // Only update for shard 0
            if (this.Client.ShardId is 0)
            {
                foreach (var applicationCommand in this._updateList)
                {
                    this.RegisterCommands(applicationCommand.Value, applicationCommand.Key);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Registers an application command class.
        /// </summary>
        /// <typeparam name="T">The application command class to register to Discord.</typeparam>
        /// <param name="guildId">The guild id to register it on. <see langword="null"/> for a global command.</param>
        public void RegisterCommands<T>(ulong? guildId = null) where T : ApplicationCommandModule
        {
            if (this.Client.ShardId != 0)
                return;
            // Create a new list for the guild commands if it doesn't exist
            else if (!this._updateList.TryGetValue(guildId, out var possibleCommands))
                this._updateList[guildId] = new() { typeof(T) };
            // Add onto the existing list of application commands for this guild
            else
                possibleCommands.Add(typeof(T));
        }

        /// <summary>
        /// Registers a command class.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the command class to register.</param>
        /// <param name="guildId">The guild id to register it on. <see langword="null"/> for a global command.</param>
        public void RegisterCommands(Type type, ulong? guildId = null)
        {
            if (!typeof(ApplicationCommandModule).IsAssignableFrom(type))
                throw new ArgumentException("Command classes have to inherit from ApplicationCommandModule", nameof(type));
            // If sharding, only register for shard 0
            else if (this.Client.ShardId != 0)
                return;
            // Create a new list for the guild commands if it doesn't exist
            else if (!this._updateList.TryGetValue(guildId, out var possibleCommands))
                this._updateList[guildId] = new() { type };
            // Add onto the existing list of application commands for this guild
            else
                possibleCommands.Add(type);
        }

        /// <summary>
        /// Registers all application command classes from a given assembly.
        /// </summary>
        /// <param name="assembly">Assembly to register command classes from.</param>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands(Assembly assembly, ulong? guildId = null)
        {
            // Intentionally don't register nested classes similarly from the top-level classes due to application command nesting.
            foreach (var xt in assembly.ExportedTypes.Where(xt => xt.IsSubclassOf(typeof(ApplicationCommandModule)) && !xt.IsNested))
                this.RegisterCommands(xt, guildId);
        }

        /// <summary>
        /// Walks an array of types looking for valid application commands and constructing them.
        /// </summary>
        /// <param name="types">The types to search for valid application commands. Should all be <see cref="ApplicationCommandModule"/>.</param>
        /// <param name="guildId">The guild to register the application command to. <see langword="null"/> to register the command globally.</param>
        private void RegisterCommands(IEnumerable<Type> types, ulong? guildId)
        {
            // The rest of the method has not been documented by OoLunar.

            // Initialize empty lists to be added to the global ones at the end
            var commandMethods = new List<CommandMethod>();
            var groupCommands = new List<GroupCommand>();
            var subGroupCommands = new List<SubGroupCommand>();
            var contextMenuCommands = new List<ContextMenuCommand>();
            var updateList = new List<DiscordApplicationCommand>();

            _ = Task.Run(async () =>
            {
                // Iterates over all the modules
                foreach (var type in types)
                {
                    try
                    {
                        var classes = new List<Type>();

                        // Add module to classes list if it's a group
                        if (type.GetCustomAttribute<SlashCommandGroupAttribute>() != null)
                        {
                            classes.Add(type);
                        }
                        else
                        {
                            // Otherwise add the nested groups
                            classes = type.GetNestedTypes().Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null).ToList();
                        }

                        // Handles groups
                        foreach (var subclassInfo in classes)
                        {
                            // Gets the attribute and methods in the group
                            var allowDMs = subclassInfo.GetCustomAttribute<GuildOnlyAttribute>() is null;
                            var v2Permissions = subclassInfo.GetCustomAttribute<SlashCommandPermissionsAttribute>()?.Permissions;

                            var groupAttribute = subclassInfo.GetCustomAttribute<SlashCommandGroupAttribute>();
                            var subMethods = subclassInfo.GetRuntimeMethods().Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);
                            var subClasses = subclassInfo.GetNestedTypes().Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null);

                            // Group context menus
                            var contextMethods = subclassInfo.GetRuntimeMethods().Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null);
                            AddContextMenus(contextMethods);

                            // Initializes the command
                            var payload = new DiscordApplicationCommand(groupAttribute.Name, groupAttribute.Description, defaultPermission: groupAttribute.DefaultPermission, allowDMUsage: allowDMs, defaultMemberPermissions: v2Permissions);

                            var commandMethods = new List<KeyValuePair<string, MethodInfo>>();
                            // Handles commands in the group
                            foreach (var submethod in subMethods)
                            {
                                var commandAttribute = submethod.GetCustomAttribute<SlashCommandAttribute>();

                                // Gets the paramaters and accounts for InteractionContext
                                var parameters = submethod.GetParameters();
                                if (parameters?.Length is null or 0 || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                    throw new ArgumentException($"The first argument must be an InteractionContext!");
                                parameters = parameters.Skip(1).ToArray();

                                var options = await this.ParseParameters(parameters, guildId);

                                var nameLocalizations = this.GetNameLocalizations(submethod);
                                var descriptionLocalizations = this.GetDescriptionLocalizations(submethod);

                                // Creates the subcommand and adds it to the main command
                                var subPayload = new DiscordApplicationCommandOption(commandAttribute.Name, commandAttribute.Description, ApplicationCommandOptionType.SubCommand, null, null, options, name_localizations: nameLocalizations, description_localizations: descriptionLocalizations);
                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subPayload) ?? new[] { subPayload }, payload.DefaultPermission, allowDMUsage: allowDMs, defaultMemberPermissions: v2Permissions);

                                // Adds it to the method lists
                                commandMethods.Add(new(commandAttribute.Name, submethod));
                                groupCommands.Add(new() { Name = groupAttribute.Name, Methods = commandMethods });
                            }

                            var command = new SubGroupCommand { Name = groupAttribute.Name };
                            // Handles subgroups
                            foreach (var subclass in subClasses)
                            {
                                var subGroupAttribute = subclass.GetCustomAttribute<SlashCommandGroupAttribute>();
                                // I couldn't think of more creative naming
                                var moreSubMethods = subclass.GetRuntimeMethods().Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

                                var options = new List<DiscordApplicationCommandOption>();

                                var currentMethods = new List<KeyValuePair<string, MethodInfo>>();

                                // Similar to the one for regular groups
                                foreach (var moreSubMethod in moreSubMethods)
                                {
                                    var subOptions = new List<DiscordApplicationCommandOption>();
                                    var customAttribute = moreSubMethod.GetCustomAttribute<SlashCommandAttribute>();
                                    var parameters = moreSubMethod.GetParameters();
                                    if (parameters?.Length is null or 0 || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                        throw new ArgumentException($"The first argument must be an InteractionContext!");
                                    parameters = parameters.Skip(1).ToArray();
                                    subOptions = subOptions.Concat(await this.ParseParameters(parameters, guildId)).ToList();

                                    var nameLocalizations = this.GetNameLocalizations(moreSubMethod);
                                    var descriptionLocalizations = this.GetDescriptionLocalizations(moreSubMethod);
                                    var moreSubPayloads = new DiscordApplicationCommandOption(customAttribute.Name, customAttribute.Description, ApplicationCommandOptionType.SubCommand, null, null, subOptions, name_localizations: nameLocalizations, description_localizations: descriptionLocalizations);

                                    options.Add(moreSubPayloads);
                                    commandMethods.Add(new(customAttribute.Name, moreSubMethod));
                                    currentMethods.Add(new(customAttribute.Name, moreSubMethod));
                                }

                                // Subgroups Context Menus
                                var subContextMethods = subclass.GetRuntimeMethods().Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null);
                                AddContextMenus(subContextMethods);

                                // Adds the group to the command and method lists
                                var subPayload = new DiscordApplicationCommandOption(subGroupAttribute.Name, subGroupAttribute.Description, ApplicationCommandOptionType.SubCommandGroup, null, null, options);
                                command.SubCommands.Add(new() { Name = subGroupAttribute.Name, Methods = currentMethods });
                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subPayload) ?? new[] { subPayload }, payload.DefaultPermission, allowDMUsage: allowDMs, defaultMemberPermissions: v2Permissions);

                                // Accounts for lifespans for the sub group
                                if (subclass.GetCustomAttribute<SlashModuleLifespanAttribute>() is not null and { Lifespan: SlashModuleLifespan.Singleton })
                                {
                                    _singletonModules.Add(this.CreateInstance(subclass, this._configuration?.Services));
                                }
                            }

                            if (command.SubCommands.Any())
                                subGroupCommands.Add(command);

                            updateList.Add(payload);

                            // Accounts for lifespans
                            if (subclassInfo.GetCustomAttribute<SlashModuleLifespanAttribute>() is not null and { Lifespan: SlashModuleLifespan.Singleton })
                            {
                                _singletonModules.Add(this.CreateInstance(subclassInfo, this._configuration?.Services));
                            }
                        }

                        // Handles methods, only if the module isn't a group itself
                        if (type.GetCustomAttribute<SlashCommandGroupAttribute>() is null)
                        {
                            // Slash commands (again, similar to the one for groups)
                            var methods = type.GetRuntimeMethods().Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

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

                                var payload = new DiscordApplicationCommand(commandattribute.Name, commandattribute.Description, options, commandattribute.DefaultPermission, name_localizations: nameLocalizations, description_localizations: descriptionLocalizations, allowDMUsage: allowDMs, defaultMemberPermissions: v2Permissions);
                                updateList.Add(payload);
                            }

                            // Context Menus
                            var contextMethods = type.GetRuntimeMethods().Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null);
                            AddContextMenus(contextMethods);

                            // Accounts for lifespans
                            if (type.GetCustomAttribute<SlashModuleLifespanAttribute>() is not null and { Lifespan: SlashModuleLifespan.Singleton })
                            {
                                _singletonModules.Add(this.CreateInstance(type, this._configuration?.Services));
                            }
                        }

                        void AddContextMenus(IEnumerable<MethodInfo> contextMethods)
                        {
                            foreach (var contextMethod in contextMethods)
                            {
                                var contextAttribute = contextMethod.GetCustomAttribute<ContextMenuAttribute>();
                                var allowDMUsage = (contextMethod.GetCustomAttribute<GuildOnlyAttribute>() ?? contextMethod.DeclaringType.GetCustomAttribute<GuildOnlyAttribute>()) is null;
                                var permissions = (contextMethod.GetCustomAttribute<SlashCommandPermissionsAttribute>() ?? contextMethod.DeclaringType.GetCustomAttribute<SlashCommandPermissionsAttribute>())?.Permissions;
                                var command = new DiscordApplicationCommand(contextAttribute.Name, null, type: contextAttribute.Type, defaultPermission: contextAttribute.DefaultPermission, allowDMUsage: allowDMUsage, defaultMemberPermissions: permissions);

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
                        // This isn't really much more descriptive but I added a separate case for it anyway
                        if (ex is BadRequestException brex)
                            this.Client.Logger.LogCritical(brex, $"There was an error registering application commands: {brex.JsonMessage}");
                        else
                            this.Client.Logger.LogCritical(ex, $"There was an error registering application commands");

                        _errored = true;
                    }
                }

                if (_errored)
                {
                    // TODO: Fix this shit
                    throw new InvalidOperationException("There was an error registering application commands.");
                }

                try
                {
                    IEnumerable<DiscordApplicationCommand> commands;
                    // Creates a guild command if a guild id is specified, otherwise global
                    commands = guildId is null
                        ? await this.Client.BulkOverwriteGlobalApplicationCommandsAsync(updateList)
                        : await this.Client.BulkOverwriteGuildApplicationCommandsAsync(guildId.Value, updateList);

                    // Checks against the ids and adds them to the command method lists
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
                    // Adds to the global lists finally
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
            });
        }

        /// <summary>
        /// Parses and converts a method's parameters from a MethodInfo to a list of <see cref="DiscordApplicationCommandOption"/>.
        /// </summary>
        /// <param name="parameters">The parameters to convert.</param>
        /// <param name="guildId">The guild id to register the commands to. <see langword="null"/> to register the slash command globally.</param>
        /// <returns>A list of <see cref="DiscordApplicationCommandOption"/>, options Discord can parse in slash commands.</returns>
        private async Task<List<DiscordApplicationCommandOption>> ParseParameters(ParameterInfo[] parameters, ulong? guildId)
        {
            var options = new List<DiscordApplicationCommandOption>();
            foreach (var parameter in parameters)
            {
                // Retrieves the attribute
                var optionAttribute = parameter.GetCustomAttribute<OptionAttribute>() ?? throw new ArgumentException($"Argument {parameter.Name} on method {this.GetFullname(parameter.Member)} requires an {this.GetFullname(typeof(OptionAttribute))}!");

                // Remove the type nullability for retrieving the option type from the dictionary.
                var parameterType = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;
                if (!_validOptionTypes.TryGetValue(parameterType, out var parameterOptionType))
                {
                    // `string.Join` to prevent hardcoding the types. Could lead to extremely an long error message when custom converters are implemented.
                    throw new ArgumentException($"Argument {parameter.Name} on method {this.GetFullname(parameter.Member)} has an invalid type! Acceptable types are: {string.Join(", ", _validOptionTypes.Keys.Select(type => type.Name))}");
                }

                IEnumerable<DiscordApplicationCommandOptionChoice> choices;
                IEnumerable<ChoiceProviderAttribute> choiceProviders = null;
                // *Try* to get the enum choices, then from a choice provider, then from the choice attributes in that order.
                if (parameterType.IsEnum)
                {
                    choices = GetChoiceAttributesFromEnumParameter(parameter.ParameterType);
                }
                // Apparently the assignment operator returns the value. Thought that was a Rust only feature. Thanks Velvet :D - Lunar
                else if ((choiceProviders = parameter.GetCustomAttributes<ChoiceProviderAttribute>()) != null)
                {
                    // Looked into https://stackoverflow.com/questions/22628087/calling-async-method-synchronously,
                    // Then was met with https://discord.com/channels/143867839282020352/169726586931773440/1001926549576102058 (https://discord.gg/csharp)
                    choices = await this.GetChoiceAttributesFromProvider(choiceProviders, guildId);
                }
                else
                {
                    choices = this.GetChoiceAttributesFromParameter(parameter.GetCustomAttributes<ChoiceAttribute>());
                }

                var channelTypes = parameter.GetCustomAttribute<ChannelTypesAttribute>()?.ChannelTypes;
                var minimumValue = parameter.GetCustomAttribute<MinimumAttribute>()?.Value;
                var maximumValue = parameter.GetCustomAttribute<MaximumAttribute>()?.Value;
                var minimumLength = parameter.GetCustomAttribute<MinimumLengthAttribute>()?.Value;
                var maximumLength = parameter.GetCustomAttribute<MaximumLengthAttribute>()?.Value;

                var nameLocalizations = this.GetNameLocalizations(parameter);
                var descriptionLocalizations = this.GetDescriptionLocalizations(parameter);

                var autocompleteAttribute = parameter.GetCustomAttribute<AutocompleteAttribute>();
                if (autocompleteAttribute != null && autocompleteAttribute.Provider.IsSubclassOf(typeof(IAutocompleteProvider)))
                    throw new ArgumentException($"Type {this.GetFullname(autocompleteAttribute.Provider)} on parameter {parameter.Name} in method {this.GetFullname(parameter.Member)} was used as an autocomplete provider, however it does not inherit from {nameof(IAutocompleteProvider)}!");

                options.Add(new DiscordApplicationCommandOption(optionAttribute.Name, optionAttribute.Description, parameterOptionType, !parameter.IsOptional, choices, null, channelTypes, autocompleteAttribute != null || optionAttribute.Autocomplete, minimumValue, maximumValue, nameLocalizations, descriptionLocalizations, minimumLength, maximumLength));
            }

            return options;
        }

        /// <summary>
        /// Retrieves localizations for an application command's name.
        /// </summary>
        /// <remarks>
        /// TODO: Implement localization provider and use that instead of raw attributes.
        /// </remarks>
        /// <param name="customAttributeProvider">The method or parameter to fetch the localizations for. Should have a <see cref="NameLocalizationAttribute"/> attached.</param>
        /// <returns>A list of localizations for the command's name.</returns>
        private IReadOnlyDictionary<string, string> GetNameLocalizations(ICustomAttributeProvider customAttributeProvider)
        {
            var nameAttributes = (NameLocalizationAttribute[])customAttributeProvider.GetCustomAttributes(typeof(NameLocalizationAttribute), false);
            return nameAttributes.ToDictionary(nameAttribute => nameAttribute.Locale, nameAttribute => nameAttribute.Name);
        }

        /// <summary>
        /// Retrieves localizations for an application command's description.
        /// </summary>
        /// <remarks>
        /// TODO: Implement localization provider and use that instead of raw attributes.
        /// </remarks>
        /// <param name="customAttributeProvider">The method or parameter to fetch the localizations for. Should have a <see cref="NameLocalizationAttribute"/> attached.</param>
        /// <returns>A list of localizations for the command's description.</returns>
        private IReadOnlyDictionary<string, string> GetDescriptionLocalizations(ICustomAttributeProvider customAttributeProvider)
        {
            var descriptionAttributes = (DescriptionLocalizationAttribute[])customAttributeProvider.GetCustomAttributes(typeof(DescriptionLocalizationAttribute), false);
            return descriptionAttributes.ToDictionary(descriptionAttribute => descriptionAttribute.Locale, descriptionAttribute => descriptionAttribute.Description);
        }

        // Gets the choices from a choice provider
        private async Task<List<DiscordApplicationCommandOptionChoice>> GetChoiceAttributesFromProvider(IEnumerable<ChoiceProviderAttribute> customAttributes, ulong? guildId)
        {
            var choices = new List<DiscordApplicationCommandOptionChoice>();
            foreach (var choiceProviderAttribute in customAttributes)
            {
                if (!choiceProviderAttribute.ProviderType.IsSubclassOf(typeof(IChoiceProvider)))
                    throw new ArgumentException($"{this.GetFullname(choiceProviderAttribute.ProviderType)} was used as a choice provider, but does not inherit from {nameof(IChoiceProvider)}.");

                IChoiceProvider instance = null;
                try
                {
                    instance = (IChoiceProvider)Activator.CreateInstance(choiceProviderAttribute.ProviderType);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException($"Failed to create instance of {choiceProviderAttribute.ProviderType}. Is a public parameterless constructor available?", exception);
                }

                // Abstract class offers more properties that can be set
                if (choiceProviderAttribute.ProviderType.IsSubclassOf(typeof(ChoiceProvider)))
                {
                    choiceProviderAttribute.ProviderType.GetProperty(nameof(ChoiceProvider.GuildId))?.SetValue(instance, guildId);
                    choiceProviderAttribute.ProviderType.GetProperty(nameof(ChoiceProvider.Services))?.SetValue(instance, this._configuration.Services);
                }

                // Gets the choices from the method
                var result = await instance.Provider();
                if (result.Any())
                {
                    choices.AddRange(result);
                }
            }

            return choices;
        }

        // Gets choices from an enum
        private static IEnumerable<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromEnumParameter(Type enumParam)
        {
            foreach (Enum enumValue in Enum.GetValues(Nullable.GetUnderlyingType(enumParam) ?? enumParam))
            {
                yield return new DiscordApplicationCommandOptionChoice(enumValue.GetName(), enumValue.ToString());
            }
        }

        // Gets choices from choice attributes
        private IEnumerable<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromParameter(IEnumerable<ChoiceAttribute> choiceAttributes)
        {
            return !choiceAttributes.Any()
                ? null
                : choiceAttributes.Select(attribute => new DiscordApplicationCommandOptionChoice(attribute.Name, attribute.Value));
        }

        /// <summary>
        /// Returns a member info's name and, if available, fullname prepended.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> to get the fullname of.</param>
        private string GetFullname(MemberInfo memberInfo) => (memberInfo.DeclaringType == null ? null : memberInfo.DeclaringType.FullName + '.') + memberInfo.Name;

        /// <summary>
        /// Executes application commands.
        /// </summary>
        private async Task InteractionHandler(DiscordClient client, InteractionCreateEventArgs eventArgs)
        {
            if (eventArgs.Interaction.Type == InteractionType.ApplicationCommand)
            {
                // Creates the context
                var context = new InteractionContext
                {
                    Interaction = eventArgs.Interaction,
                    Channel = eventArgs.Interaction.Channel,
                    Guild = eventArgs.Interaction.Guild,
                    User = eventArgs.Interaction.User,
                    Client = client,
                    SlashCommandsExtension = this,
                    CommandName = eventArgs.Interaction.Data.Name,
                    InteractionId = eventArgs.Interaction.Id,
                    Token = eventArgs.Interaction.Token,
                    Services = this._configuration?.Services,
                    ResolvedUserMentions = eventArgs.Interaction.Data.Resolved?.Users?.Values.ToList(),
                    ResolvedRoleMentions = eventArgs.Interaction.Data.Resolved?.Roles?.Values.ToList(),
                    ResolvedChannelMentions = eventArgs.Interaction.Data.Resolved?.Channels?.Values.ToList(),
                    Type = ApplicationCommandType.SlashCommand
                };

                try
                {
                    // TODO: Fix this shit
                    if (_errored)
                        throw new InvalidOperationException("Slash commands failed to register properly on startup.");

                    // Gets the method for the command
                    var methods = _commandMethods.Where(x => x.CommandId == eventArgs.Interaction.Data.Id);
                    var groups = _groupCommands.Where(x => x.CommandId == eventArgs.Interaction.Data.Id);
                    var subgroups = _subGroupCommands.Where(x => x.CommandId == eventArgs.Interaction.Data.Id);
                    if (!methods.Any() && !groups.Any() && !subgroups.Any())
                        throw new InvalidOperationException("A slash command was executed, but no command was registered for it.");

                    // Just read the code you'll get it
                    // "Just read the code you'll get it"
                    // Fun fact: I do not get it
                    // CoPilot, do you understand the code?
                    // "No, I do not."
                    // See Cabbage?
                    if (methods.Any())
                    {
                        var method = methods.First().Method;
                        var args = await this.ResolveInteractionCommandParameters(eventArgs, context, method, eventArgs.Interaction.Data.Options);

                        await this.RunCommandAsync(context, method, args);
                    }
                    else if (groups.Any())
                    {
                        var command = eventArgs.Interaction.Data.Options.First();
                        var method = groups.First().Methods.First(x => x.Key == command.Name).Value;

                        var args = await this.ResolveInteractionCommandParameters(eventArgs, context, method, eventArgs.Interaction.Data.Options.First().Options);

                        await this.RunCommandAsync(context, method, args);
                    }
                    else if (subgroups.Any())
                    {
                        var command = eventArgs.Interaction.Data.Options.First();
                        var group = subgroups.First().SubCommands.First(x => x.Name == command.Name);
                        var method = group.Methods.First(x => x.Key == command.Options.First().Name).Value;

                        var args = await this.ResolveInteractionCommandParameters(eventArgs, context, method, eventArgs.Interaction.Data.Options.First().Options.First().Options);
                        await this.RunCommandAsync(context, method, args);
                    }

                    await this._slashExecuted.InvokeAsync(this, new SlashCommandExecutedEventArgs { Context = context });
                }
                catch (Exception ex)
                {
                    await this._slashError.InvokeAsync(this, new SlashCommandErrorEventArgs { Context = context, Exception = ex });
                }
            }

            // Handles autcomplete interactions
            if (eventArgs.Interaction.Type == InteractionType.AutoComplete)
            {
                if (_errored)
                    throw new InvalidOperationException("Slash commands failed to register properly on startup.");

                // Gets the method for the command
                var methods = _commandMethods.Where(x => x.CommandId == eventArgs.Interaction.Data.Id);
                var groups = _groupCommands.Where(x => x.CommandId == eventArgs.Interaction.Data.Id);
                var subgroups = _subGroupCommands.Where(x => x.CommandId == eventArgs.Interaction.Data.Id);
                if (!methods.Any() && !groups.Any() && !subgroups.Any())
                    throw new InvalidOperationException("An autocomplete interaction was created, but no command was registered for it.");

                if (methods.Any())
                {
                    var method = methods.First().Method;
                    var options = eventArgs.Interaction.Data.Options;

                    // Gets the focused option
                    var focusedOption = options.First(o => o.Focused);
                    var parameter = method.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
                    await this.RunAutocomplete(eventArgs.Interaction, parameter, options, focusedOption);
                }

                if (groups.Any())
                {
                    var command = eventArgs.Interaction.Data.Options.First();
                    var method = groups.First().Methods.First(x => x.Key == command.Name).Value;

                    var options = command.Options;
                    var focusedOption = options.First(o => o.Focused);
                    var parameter = method.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
                    await this.RunAutocomplete(eventArgs.Interaction, parameter, options, focusedOption);
                }

                if (subgroups.Any())
                {
                    var command = eventArgs.Interaction.Data.Options.First();
                    var group = subgroups.First().SubCommands.First(x => x.Name == command.Name);
                    var method = group.Methods.First(x => x.Key == command.Options.First().Name).Value;

                    var options = command.Options.First().Options;
                    var focusedOption = options.First(o => o.Focused);
                    var parameter = method.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
                    await this.RunAutocomplete(eventArgs.Interaction, parameter, options, focusedOption);
                }
            }
        }

        private async Task ContextMenuHandler(DiscordClient client, ContextMenuInteractionCreateEventArgs eventArgs)
        {
            // Creates the context
            var context = new ContextMenuContext
            {
                Interaction = eventArgs.Interaction,
                Channel = eventArgs.Interaction.Channel,
                Client = client,
                Services = this._configuration?.Services,
                CommandName = eventArgs.Interaction.Data.Name,
                SlashCommandsExtension = this,
                Guild = eventArgs.Interaction.Guild,
                InteractionId = eventArgs.Interaction.Id,
                User = eventArgs.Interaction.User,
                Token = eventArgs.Interaction.Token,
                TargetUser = eventArgs.TargetUser,
                TargetMessage = eventArgs.TargetMessage,
                Type = eventArgs.Type
            };

            // TODO: Implement e.interaction.TargetMember
            if (eventArgs.Interaction.Guild != null && eventArgs.TargetUser != null && eventArgs.Interaction.Guild.Members.TryGetValue(eventArgs.TargetUser.Id, out var member))
                context.TargetMember = member;

            try
            {
                if (_errored)
                    throw new InvalidOperationException("Context menus failed to register properly on startup.");

                // Gets the method for the command
                var method = _contextMenuCommands.FirstOrDefault(x => x.CommandId == eventArgs.Interaction.Data.Id);

                if (method == null)
                    throw new InvalidOperationException("A context menu was executed, but no command was registered for it.");

                await this.RunCommandAsync(context, method.Method, new[] { context });
                await this._contextMenuExecuted.InvokeAsync(this, new ContextMenuExecutedEventArgs { Context = context });
            }
            catch (Exception ex)
            {
                await this._contextMenuErrored.InvokeAsync(this, new ContextMenuErrorEventArgs { Context = context, Exception = ex });
            }
        }

        internal async Task RunCommandAsync(BaseContext context, MethodInfo method, IEnumerable<object> args)
        {
            // Accounts for lifespans
            var moduleLifespan = (method.DeclaringType.GetCustomAttribute<SlashModuleLifespanAttribute>() != null ? method.DeclaringType.GetCustomAttribute<SlashModuleLifespanAttribute>()?.Lifespan : SlashModuleLifespan.Transient) ?? SlashModuleLifespan.Transient;
            var classInstance = moduleLifespan switch // Accounts for static methods and adds DI
            {
                // Accounts for static methods and adds DI
                SlashModuleLifespan.Scoped => method.IsStatic ? ActivatorUtilities.CreateInstance(this._configuration?.Services.CreateScope().ServiceProvider, method.DeclaringType) : this.CreateInstance(method.DeclaringType, this._configuration?.Services.CreateScope().ServiceProvider),
                // Accounts for static methods and adds DI
                SlashModuleLifespan.Transient => method.IsStatic ? ActivatorUtilities.CreateInstance(this._configuration?.Services, method.DeclaringType) : this.CreateInstance(method.DeclaringType, this._configuration?.Services),
                // If singleton, gets it from the singleton list
                SlashModuleLifespan.Singleton => _singletonModules.First(x => ReferenceEquals(x.GetType(), method.DeclaringType)),
                // A new lifespan type was introduced.
                _ => throw new NotImplementedException($"An unknown {nameof(SlashModuleLifespanAttribute)} scope was specified on command {context.CommandName}")
            };

            var applicationCommand = classInstance as ApplicationCommandModule;

            // Slash commands
            if (context is InteractionContext slashContext)
            {
                await this._slashInvoked.InvokeAsync(this, new SlashCommandInvokedEventArgs { Context = slashContext }, AsyncEventExceptionMode.ThrowAll);
                await this.RunPreexecutionChecksAsync(method, slashContext);

                // Runs BeforeExecution and accounts for groups that don't inherit from ApplicationCommandModule
                var shouldExecute = await (applicationCommand?.BeforeSlashExecutionAsync(slashContext) ?? Task.FromResult(true));

                if (shouldExecute)
                {
                    await (Task)method.Invoke(classInstance, args.ToArray());

                    // Runs AfterExecution and accounts for groups that don't inherit from ApplicationCommandModule
                    await (applicationCommand?.AfterSlashExecutionAsync(slashContext) ?? Task.CompletedTask);
                }
            }
            // Context menus
            if (context is ContextMenuContext contextMenuContext)
            {
                await this._contextMenuInvoked.InvokeAsync(this, new ContextMenuInvokedEventArgs() { Context = contextMenuContext }, AsyncEventExceptionMode.ThrowAll);

                await this.RunPreexecutionChecksAsync(method, contextMenuContext);

                // This null check actually shouldn't be necessary for context menus but I'll keep it in just in case
                var shouldExecute = await (applicationCommand?.BeforeContextMenuExecutionAsync(contextMenuContext) ?? Task.FromResult(true));

                if (shouldExecute)
                {
                    await (Task)method.Invoke(classInstance, args.ToArray());

                    await (applicationCommand?.AfterContextMenuExecutionAsync(contextMenuContext) ?? Task.CompletedTask);
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the specified type using dependency injection. Attempts to perform constructor injection first, falling back on property and field injection.
        /// </summary>
        /// <param name="type">The type to initialize.</param>
        /// <param name="serviceProvider">The services provided to create the type.</param>
        /// <returns>The initialized type.</returns>
        internal object CreateInstance(Type type, IServiceProvider serviceProvider)
        {
            var constructors = type.GetConstructors().Where(x => x.IsPublic && !x.IsStatic);
            var properties = type.GetRuntimeProperties().Where(x => x.CanWrite && x.SetMethod.IsPublic && !x.SetMethod.IsStatic && !x.IsDefined(typeof(DontInjectAttribute)));
            var fields = type.GetRuntimeFields().Where(x => x.IsPublic && !x.IsStatic && !x.IsInitOnly && !x.IsDefined(typeof(DontInjectAttribute)));

            // Static constructor?
            if (!constructors.Any() && !properties.Any() && !fields.Any())
            {
                return Activator.CreateInstance(type);
            }

            // Constructor injection.
            var constructor = constructors.FirstOrDefault();
            if (constructor != null)
            {
                return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type);
            }

            // Property and field injection.
            var typeInstance = Activator.CreateInstance(type);
            if (properties.Any())
            {
                foreach (var property in properties)
                {
                    property.SetValue(typeInstance, serviceProvider.GetService(property.PropertyType));
                }
            }
            if (fields.Any())
            {
                foreach (var field in fields)
                {
                    field.SetValue(typeInstance, serviceProvider.GetService(field.FieldType));
                }
            }

            return typeInstance;
        }

        // Parses slash command parameters
        private async Task<List<object>> ResolveInteractionCommandParameters(InteractionCreateEventArgs eventArgs, InteractionContext context, MethodInfo method, IEnumerable<DiscordInteractionDataOption> options)
        {
            var args = new List<object> { context };
            var parameters = method.GetParameters().Skip(1);

            for (var i = 0; i < parameters.Count(); i++)
            {
                var parameter = parameters.ElementAt(i);
                var parameterOption = parameter.GetCustomAttribute<OptionAttribute>();

                // Accounts for optional arguments without values given
                if (parameter.IsOptional && (!options?.Any(x => x.Name.Equals(parameterOption.Name, StringComparison.InvariantCultureIgnoreCase)) ?? true))
                    args.Add(parameter.DefaultValue);
                else
                {
                    var option = options.Single(x => x.Name.Equals(parameterOption.Name, StringComparison.InvariantCultureIgnoreCase));

                    // Checks the type and casts/references resolved and adds the value to the list
                    // This can probably reference the slash command's type property that didn't exist when I wrote this and it could use a cleaner switch instead, but if it works it works
                    // TODO: Custom converter support w/ _validOptionTypes
                    // TODO: Move these converters to their own file.
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
                        // Checks through resolved
                        if (eventArgs.Interaction.Data.Resolved.Members != null &&
                            eventArgs.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                            args.Add(member);
                        else if (eventArgs.Interaction.Data.Resolved.Users != null &&
                                 eventArgs.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
                            args.Add(user);
                        else
                            args.Add(await this.Client.GetUserAsync((ulong)option.Value));
                    }
                    else if (parameter.ParameterType == typeof(DiscordChannel))
                    {
                        // Checks through resolved
                        if (eventArgs.Interaction.Data.Resolved.Channels != null &&
                            eventArgs.Interaction.Data.Resolved.Channels.TryGetValue((ulong)option.Value, out var channel))
                            args.Add(channel);
                        else
                            args.Add(eventArgs.Interaction.Guild.GetChannel((ulong)option.Value));
                    }
                    else if (parameter.ParameterType == typeof(DiscordRole))
                    {
                        // Checks through resolved
                        if (eventArgs.Interaction.Data.Resolved.Roles != null &&
                            eventArgs.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                            args.Add(role);
                        else
                            args.Add(eventArgs.Interaction.Guild.GetRole((ulong)option.Value));
                    }
                    else if (parameter.ParameterType == typeof(SnowflakeObject))
                    {
                        // Checks through resolved
                        if (eventArgs.Interaction.Data.Resolved.Roles != null && eventArgs.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                            args.Add(role);
                        else if (eventArgs.Interaction.Data.Resolved.Members != null && eventArgs.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                            args.Add(member);
                        else if (eventArgs.Interaction.Data.Resolved.Users != null && eventArgs.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
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
                        if (eventArgs.Interaction.Data.Resolved.Attachments?.ContainsKey((ulong)option.Value) ?? false)
                        {
                            var attachment = eventArgs.Interaction.Data.Resolved.Attachments[(ulong)option.Value];
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

        // Runs pre-execution checks
        private async Task RunPreexecutionChecksAsync(MethodInfo method, BaseContext context)
        {
            var attributes = new List<IApplicationCommandExecutionCheck>();

            // get checks
            if (context is InteractionContext ctx)
            {
                // Gets all attributes from parent classes
                attributes.AddRange(method.GetCustomAttributes<SlashCheckBaseAttribute>(true));
                attributes.AddRange(this.GetCustomAttributesRecursively<SlashCheckBaseAttribute>(method.DeclaringType));
            }
            else if (context is ContextMenuContext contextMenuContext)
            {
                attributes.AddRange(method.GetCustomAttributes<ContextMenuCheckBaseAttribute>(true));
                attributes.AddRange(this.GetCustomAttributesRecursively<ContextMenuCheckBaseAttribute>(method.DeclaringType));
            }

            // execute checks

            var dict = new Dictionary<IApplicationCommandExecutionCheck, bool>();
            foreach (var att in attributes)
            {
                // Runs the check and adds the result to a list
                dict.Add(att, await att.ExecuteChecksAsync(context));
            }

            // Checks if any failed, and throws an exception
            // note: this contains legay code, to be removed eventually
#pragma warning disable CS0618 // obsolete exceptions
            if (dict.Any(x => x.Value == false))
            {
                if (context is InteractionContext)
                    throw new SlashExecutionChecksFailedException
                    {
                        FailedChecks = dict.Where(x => x.Value == false)
                            .Select(x => x.Key as SlashCheckBaseAttribute)
                            .Where(x => x != null)
                            .ToList()
                    };
                else
                    throw new ContextMenuExecutionChecksFailedException
                    {
                        FailedChecks = dict.Where(x => x.Value == false)
                        .Select(x => x.Key as ContextMenuCheckBaseAttribute)
                        .Where(x => x != null)
                        .ToList()
                    };
#pragma warning restore CS0618

                throw new ApplicationCommandExecutionChecksFailedException
                {
                    FailedChecks = dict.Where(x => x.Value == false)
                        .Select(x => x.Key)
                        .ToList(),
                    Context = context
                };
            }
        }

        private IEnumerable<TAttribute> GetCustomAttributesRecursively<TAttribute>(Type type)
            where TAttribute : Attribute
        {
            if (type is null)
            {
                return Enumerable.Empty<TAttribute>();
            }

            return type.GetCustomAttributes<TAttribute>(true).Concat(this.GetCustomAttributesRecursively<TAttribute>(type));
        }

        // Actually handles autocomplete interactions
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
                // Gets the provider
                var provider = parameter.GetCustomAttribute<AutocompleteAttribute>()?.Provider;
                if (provider == null) return;
                var providerInstance = (IAutocompleteProvider)Activator.CreateInstance(provider);
                var choices = await providerInstance.Provider(context);

                await interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
                await this._autocompleteExecuted.InvokeAsync(this, new()
                {
                    Context = context,
                    ProviderType = provider
                });
            }
            catch (Exception ex)
            {
                await this._autocompleteErrored.InvokeAsync(this, new AutocompleteErrorEventArgs()
                {
                    Exception = ex,
                    Context = context,
                    ProviderType = parameter.GetCustomAttribute<AutocompleteAttribute>()?.Provider
                });
            }
        }

        /// <summary>
        /// <para>Refreshes your commands, used for refreshing choice providers or applying commands registered after the ready event on the discord client.
        /// Should only be run on the slash command extension linked to shard 0 if sharding.</para>
        /// <para>Not recommended and should be avoided since it can make slash commands be unresponsive for a while.</para>
        /// </summary>
        [Obsolete("Please use RefreshCommandsAsync instead. This method will be removed by DSharpPlus release v4.4.0", true), EditorBrowsable(EditorBrowsableState.Advanced), SuppressMessage("Roslyn", "IDE1006", Justification = "Should be removed by release 4.4.0 or above.")]
        public Task RefreshCommands() => this.RefreshCommandsAsync();

        /// <summary>
        /// <para>Refreshes your commands, used for refreshing choice providers or applying commands registered after the ready event on the discord client.
        /// Should only be run on the slash command extension linked to shard 0 if sharding.</para>
        /// <para>Not recommended and should be avoided since it can make slash commands be unresponsive for a while.</para>
        /// </summary>
        public async Task RefreshCommandsAsync()
        {
            _commandMethods.Clear();
            _groupCommands.Clear();
            _subGroupCommands.Clear();
            _registeredCommands.Clear();

            await this.UpdateApplicationCommandsAsync();
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
    }

    // I'm not sure if creating separate classes is the cleanest thing here but I can't think of anything else so these stay

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
