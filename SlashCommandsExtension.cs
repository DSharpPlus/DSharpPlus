using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using DSharpPlus.Entities;
using System.Linq;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Emzi0767.Utilities;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.Serialization;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// A class that handles slash commands for a client
    /// </summary>
    public class SlashCommandsExtension : BaseExtension
    {
        internal List<CommandMethod> CommandMethods { get; set; } = new List<CommandMethod>();
        internal List<GroupCommand> GroupCommands { get; set; } = new List<GroupCommand>();
        internal List<SubGroupCommand> SubGroupCommands { get; set; } = new List<SubGroupCommand>();

        internal SlashCommandsExtension() { }

        protected override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = client;

            _error = new AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs>("SLASHCOMMAND_ERRORED", TimeSpan.Zero, null);
            _executed = new AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs>("SLASHCOMMAND_EXECUTED", TimeSpan.Zero, null);

            Client.InteractionCreated += InteractionHandler;
        }

        //Register

        /// <summary>
        /// Registers a command class
        /// </summary>
        /// <typeparam name="T">The command class to register</typeparam>
        public void RegisterCommands<T>(ulong? guildid = null) where T : SlashCommandModule
        {
            RegisterCommands(typeof(T), guildid);
        }

        private void RegisterCommands(Type t, ulong? guildid)
        {
            CommandMethod[] InternalCommandMethods = Array.Empty<CommandMethod>();
            GroupCommand[] InternalGroupCommands = Array.Empty<GroupCommand>();
            SubGroupCommand[] InternalSubGroupCommands = Array.Empty<SubGroupCommand>();

            Client.Ready += (s, e) =>
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        List<DiscordApplicationCommand> ToUpdate = new List<DiscordApplicationCommand>();

                        var ti = t.GetTypeInfo();

                        var classes = ti.DeclaredNestedTypes;
                        foreach (var tti in classes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null))
                        {
                            var groupatt = tti.GetCustomAttribute<SlashCommandGroupAttribute>();
                            var submethods = tti.DeclaredMethods;
                            var subclasses = tti.DeclaredNestedTypes;
                            if (subclasses.Any(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null) && submethods.Any(x => x.GetCustomAttribute<SlashCommandAttribute>() != null))
                            {
                                throw new ArgumentException("Slash command groups cannot have both subcommands and subgroups!");
                            }
                            var payload = new DiscordApplicationCommand(groupatt.Name, groupatt.Description);


                            var commandmethods = new Dictionary<string, MethodInfo>();
                            foreach (var submethod in submethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null))
                            {
                                var commandattribute = submethod.GetCustomAttribute<SlashCommandAttribute>();

                                var options = new List<DiscordApplicationCommandOption>();

                                var parameters = submethod.GetParameters();
                                if (!ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                    throw new ArgumentException($"The first argument must be an InteractionContext!");
                                parameters = parameters.Skip(1).ToArray();
                                foreach (var parameter in parameters)
                                {
                                    var optionattribute = parameter.GetCustomAttribute<OptionAttribute>();
                                    if (optionattribute == null)
                                        throw new ArgumentException("Arguments must have the Option attribute!");

                                    var type = parameter.ParameterType;
                                    var parametertype = GetParameterType(type);                           

                                    DiscordApplicationCommandOptionChoice[] choices = null;
                                    var choiceattributes = parameter.GetCustomAttributes<ChoiceAttribute>();
                                    if (choiceattributes.Any())
                                    {
                                        choices = Array.Empty<DiscordApplicationCommandOptionChoice>();
                                        foreach (var att in choiceattributes)
                                        {
                                            choices = choices.Append(new DiscordApplicationCommandOptionChoice(att.Name, att.Value)).ToArray();
                                        }
                                    }
                                    options.Add(new DiscordApplicationCommandOption(optionattribute.Name, optionattribute.Description, parametertype, !parameter.IsOptional, choices));
                                }
                                var subpayload = new DiscordApplicationCommandOption(commandattribute.Name, commandattribute.Description, ApplicationCommandOptionType.SubCommand, null, null, options);

                                commandmethods.Add(commandattribute.Name, submethod);

                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options != null ? payload.Options.ToArray().Append(subpayload) : new DiscordApplicationCommandOption[] { subpayload });

                                InternalGroupCommands = InternalGroupCommands.Append(new GroupCommand { Name = groupatt.Name, ParentClass = tti, Methods = commandmethods }).ToArray();
                            }
                            foreach (var subclass in subclasses.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null))
                            {
                                var subgroupatt = subclass.GetCustomAttribute<SlashCommandGroupAttribute>();
                                var subsubmethods = subclass.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

                                var command = new SubGroupCommand { Name = groupatt.Name };

                                var options = new List<DiscordApplicationCommandOption>();

                                foreach (var subsubmethod in subsubmethods)
                                {
                                    var suboptions = new List<DiscordApplicationCommandOption>();
                                    var commatt = subsubmethod.GetCustomAttribute<SlashCommandAttribute>();
                                    var parameters = subsubmethod.GetParameters();
                                    if (!ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                        throw new ArgumentException($"The first argument must be an InteractionContext!");
                                    parameters = parameters.Skip(1).ToArray();
                                    foreach (var parameter in parameters)
                                    {
                                        var optionattribute = parameter.GetCustomAttribute<OptionAttribute>();
                                        if (optionattribute == null)
                                            throw new ArgumentException("Arguments must have the Option attribute!");

                                        var type = parameter.ParameterType;
                                        ApplicationCommandOptionType parametertype;
                                        if (ReferenceEquals(type, typeof(string)))
                                            parametertype = ApplicationCommandOptionType.String;
                                        else if (ReferenceEquals(type, typeof(long)))
                                            parametertype = ApplicationCommandOptionType.Integer;
                                        else if (ReferenceEquals(type, typeof(bool)))
                                            parametertype = ApplicationCommandOptionType.Boolean;
                                        else if (ReferenceEquals(type, typeof(DiscordChannel)))
                                            parametertype = ApplicationCommandOptionType.Channel;
                                        else if (ReferenceEquals(type, typeof(DiscordUser)))
                                            parametertype = ApplicationCommandOptionType.User;
                                        else if (ReferenceEquals(type, typeof(DiscordRole)))
                                            parametertype = ApplicationCommandOptionType.Role;
                                        else
                                            throw new ArgumentException("Cannot convert type! Argument types must be string, long, bool, DiscordChannel, DiscordUser or DiscordRole.");

                                        DiscordApplicationCommandOptionChoice[] choices = null;
                                        var choiceattributes = parameter.GetCustomAttributes<ChoiceAttribute>();
                                        if (choiceattributes.Any())
                                        {
                                            choices = Array.Empty<DiscordApplicationCommandOptionChoice>();
                                            foreach (var att in choiceattributes)
                                            {
                                                choices = choices.Append(new DiscordApplicationCommandOptionChoice(att.Name, att.Value)).ToArray();
                                            }
                                        }

                                        suboptions.Add(new DiscordApplicationCommandOption(optionattribute.Name, optionattribute.Description, parametertype, !parameter.IsOptional, choices));
                                    }
                                    var subsubpayload = new DiscordApplicationCommandOption(commatt.Name, commatt.Description, ApplicationCommandOptionType.SubCommand, null, null, suboptions);
                                    options.Add(subsubpayload);
                                    commandmethods.Add(commatt.Name, subsubmethod);
                                }

                                var subpayload = new DiscordApplicationCommandOption(subgroupatt.Name, subgroupatt.Description, ApplicationCommandOptionType.SubCommandGroup, null, null, options);
                                command.SubCommands.Add(new GroupCommand { Name = subgroupatt.Name, ParentClass = subclass, Methods = commandmethods });
                                InternalSubGroupCommands = InternalSubGroupCommands.Append(command).ToArray();
                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options != null ? payload.Options.ToArray().Append(subpayload) : new DiscordApplicationCommandOption[] { subpayload });
                            }
                            ToUpdate.Add(payload);
                        }

                        var methods = ti.DeclaredMethods;
                        foreach (var method in methods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null))
                        {
                            var commandattribute = method.GetCustomAttribute<SlashCommandAttribute>();

                            var options = new List<DiscordApplicationCommandOption>();

                            var parameters = method.GetParameters();
                            if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(InteractionContext)))
                                throw new ArgumentException($"The first argument must be an InteractionContext!");
                            parameters = parameters.Skip(1).ToArray();
                            foreach (var parameter in parameters)
                            {
                                var optionattribute = parameter.GetCustomAttribute<OptionAttribute>();
                                if (optionattribute == null)
                                    throw new ArgumentException("Arguments must have the SlashOption attribute!");

                                var type = parameter.ParameterType;
                                ApplicationCommandOptionType parametertype = GetParameterType(type);

                                DiscordApplicationCommandOptionChoice[] choices = null;
                                var choiceattributes = parameter.GetCustomAttributes<ChoiceAttribute>();
                                if (choiceattributes.Any())
                                {
                                    choices = Array.Empty<DiscordApplicationCommandOptionChoice>();
                                    foreach (var att in choiceattributes)
                                    {
                                        choices = choices.Append(new DiscordApplicationCommandOptionChoice(att.Name, att.Value)).ToArray();
                                    }
                                }

                                options.Add(new DiscordApplicationCommandOption(optionattribute.Name, optionattribute.Description, parametertype, !parameter.IsOptional, choices));
                            }
                            InternalCommandMethods = InternalCommandMethods.Append(new CommandMethod { Method = method, Name = commandattribute.Name, ParentClass = t }).ToArray();

                            var payload = new DiscordApplicationCommand(commandattribute.Name, commandattribute.Description, options);
                            ToUpdate.Add(payload);
                        }

                        IEnumerable<DiscordApplicationCommand> commands;
                        if(guildid == null)
                        {
                            commands = await Client.BulkOverwriteGlobalApplicationCommandsAsync(ToUpdate);
                        }
                        else
                        {
                            commands = await Client.BulkOverwriteGuildApplicationCommandsAsync(guildid.Value, ToUpdate);
                        }
                        foreach (var command in commands)
                        {
                            if (InternalCommandMethods.Any(x => x.Name == command.Name))
                                InternalCommandMethods.First(x => x.Name == command.Name).Id = command.Id;

                            else if (InternalGroupCommands.Any(x => x.Name == command.Name))
                                InternalGroupCommands.First(x => x.Name == command.Name).Id = command.Id;

                            else if (InternalSubGroupCommands.Any(x => x.Name == command.Name))
                                InternalSubGroupCommands.First(x => x.Name == command.Name).Id = command.Id;
                        }
                        CommandMethods.AddRange(InternalCommandMethods);
                        GroupCommands.AddRange(InternalGroupCommands);
                        SubGroupCommands.AddRange(InternalSubGroupCommands);
                    }
                    catch (Exception ex)
                    {
                        Client.Logger.LogError(ex, $"There was an error registering slash commands");
                        Environment.Exit(-1);
                    }
                });

                return Task.CompletedTask;
            };
        }

        //Handler
        internal Task InteractionHandler(DiscordClient client, InteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
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
                    Token = e.Interaction.Token
                };

                try
                {
                    var methods = CommandMethods.Where(x => x.Id == e.Interaction.Data.Id);
                    var groups = GroupCommands.Where(x => x.Id == e.Interaction.Data.Id);
                    var subgroups = SubGroupCommands.Where(x => x.Id == e.Interaction.Data.Id);
                    if (!methods.Any() && !groups.Any() && !subgroups.Any())
                        throw new Exception("An interaction was created, but no command was registered for it");
                    if (methods.Any())
                    {
                        var method = methods.First();

                        List<object> args = new List<object> { context };
                        var parameters = method.Method.GetParameters().Skip(1);

                        for (int i = 0; i < parameters.Count(); i++)
                        {
                            var parameter = parameters.ElementAt(i);
                            if (parameter.IsOptional && (e.Interaction.Data.Options == null || e.Interaction.Data.Options?.ElementAtOrDefault(i) == default))
                                args.Add(parameter.DefaultValue);
                            else
                            {
                                var option = e.Interaction.Data.Options.ElementAt(i);

                                if (ReferenceEquals(parameter.ParameterType, typeof(string)))
                                    args.Add(option.Value.ToString());
                                else if (ReferenceEquals(parameter.ParameterType, typeof(long)))
                                    args.Add((long)option.Value);
                                else if (ReferenceEquals(parameter.ParameterType, typeof(bool)))
                                    args.Add((bool)option.Value);
                                else if (ReferenceEquals(parameter.ParameterType, typeof(DiscordUser)))
                                {
                                    if (e.Interaction.Data.Resolved.Members != null && e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                                    {
                                        args.Add(member);
                                    }
                                    else if (e.Interaction.Data.Resolved.Users != null && e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
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
                                    if (e.Interaction.Data.Resolved.Channels != null && e.Interaction.Data.Resolved.Channels.TryGetValue((ulong)option.Value, out var channel))
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
                                    if (e.Interaction.Data.Resolved.Roles != null && e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                                    {
                                        args.Add(role);
                                    }
                                    else
                                    {
                                        args.Add(e.Interaction.Guild.GetRole((ulong)option.Value));
                                    }
                                }
                                else
                                    throw new ArgumentException($"How on earth did that happen");
                            }
                        }
                        var classinstance = Activator.CreateInstance(method.ParentClass);
                        var task = (Task)method.Method.Invoke(classinstance, args.ToArray());
                        await task;
                    }
                    else if (groups.Any())
                    {
                        var command = e.Interaction.Data.Options.First();
                        var method = groups.First().Methods.First(x => x.Key == command.Name).Value;

                        List<object> args = new List<object> { context };
                        var parameters = method.GetParameters().Skip(1);

                        for (int i = 0; i < parameters.Count(); i++)
                        {
                            var parameter = parameters.ElementAt(i);
                            if (parameter.IsOptional && (command.Options == null || command.Options?.ElementAtOrDefault(i) == default))
                                args.Add(parameter.DefaultValue);
                            else
                            {
                                var option = command.Options.ElementAt(i);

                                if (ReferenceEquals(parameter.ParameterType, typeof(string)))
                                    args.Add(option.Value.ToString());
                                else if (ReferenceEquals(parameter.ParameterType, typeof(long)))
                                    args.Add((long)option.Value);
                                else if (ReferenceEquals(parameter.ParameterType, typeof(bool)))
                                    args.Add((bool)option.Value);
                                else if (ReferenceEquals(parameter.ParameterType, typeof(DiscordUser)))
                                {
                                    if (e.Interaction.Data.Resolved.Members != null && e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                                    {
                                        args.Add(member);
                                    }
                                    else if (e.Interaction.Data.Resolved.Users != null && e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
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
                                    if (e.Interaction.Data.Resolved.Channels != null && e.Interaction.Data.Resolved.Channels.TryGetValue((ulong)option.Value, out var channel))
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
                                    if (e.Interaction.Data.Resolved.Channels != null && e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                                    {
                                        args.Add(role);
                                    }
                                    else
                                    {
                                        args.Add(e.Interaction.Guild.GetRole((ulong)option.Value));
                                    }
                                }
                                else
                                    throw new ArgumentException($"How on earth did that happen");
                            }
                        }
                        var classinstance = Activator.CreateInstance(groups.First().ParentClass);
                        var task = (Task)method.Invoke(classinstance, args.ToArray());
                        await task;
                    }
                    else if (subgroups.Any())
                    {
                        var command = e.Interaction.Data.Options.First();
                        var group = subgroups.First(x => x.SubCommands.Any(y => y.Name == command.Name)).SubCommands.First(x => x.Name == command.Name);

                        var method = group.Methods.First(x => x.Key == command.Options.First().Name).Value;

                        List<object> args = new List<object> { context };
                        var parameters = method.GetParameters().Skip(1);

                        for (int i = 0; i < parameters.Count(); i++)
                        {
                            var parameter = parameters.ElementAt(i);
                            if (parameter.IsOptional && (command.Options == null || command.Options?.ElementAtOrDefault(i) == default))
                                args.Add(parameter.DefaultValue);
                            else
                            {
                                var option = command.Options.ElementAt(i);

                                if (ReferenceEquals(parameter.ParameterType, typeof(string)))
                                    args.Add(option.Value.ToString());
                                else if (ReferenceEquals(parameter.ParameterType, typeof(long)))
                                    args.Add((long)option.Value);
                                else if (ReferenceEquals(parameter.ParameterType, typeof(bool)))
                                    args.Add((bool)option.Value);
                                else if (ReferenceEquals(parameter.ParameterType, typeof(DiscordUser)))
                                {
                                    if (e.Interaction.Data.Resolved.Members != null && e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                                    {
                                        args.Add(member);
                                    }
                                    else if (e.Interaction.Data.Resolved.Users != null && e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
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
                                    if (e.Interaction.Data.Resolved.Channels != null && e.Interaction.Data.Resolved.Channels.TryGetValue((ulong)option.Value, out var channel))
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
                                    if (e.Interaction.Data.Resolved.Channels != null && e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                                    {
                                        args.Add(role);
                                    }
                                    else
                                    {
                                        args.Add(e.Interaction.Guild.GetRole((ulong)option.Value));
                                    }
                                }
                                else
                                    throw new ArgumentException($"How on earth did that happen");
                            }
                        }
                        var classinstance = Activator.CreateInstance(group.ParentClass);
                        var task = (Task)method.Invoke(classinstance, args.ToArray());
                        await task;
                    }

                    await _executed.InvokeAsync(this, new SlashCommandExecutedEventArgs { Context = context });
                }
                catch (Exception ex)
                {
                    await _error.InvokeAsync(this, new SlashCommandErrorEventArgs { Context = context, Exception = ex });
                }
            });
            return Task.CompletedTask;
        }

        //Events

        /// <summary>
        /// Fires whenver the execution of a slash command fails
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandErrorEventArgs> SlashCommandErrored
        {
            add { _error.Register(value); }
            remove { _error.Unregister(value); }
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs> _error;

        /// <summary>
        /// Fires when the execution of a slash command is successful
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandExecutedEventArgs> SlashCommandExecuted
        {
            add { _executed.Register(value); }
            remove { _executed.Unregister(value); }
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs> _executed;

        internal ApplicationCommandOptionType GetParameterType(Type type)
        {
            ApplicationCommandOptionType parametertype;
            if (ReferenceEquals(type, typeof(string)))
                parametertype = ApplicationCommandOptionType.String;
            else if (ReferenceEquals(type, typeof(long)))
                parametertype = ApplicationCommandOptionType.Integer;
            else if (ReferenceEquals(type, typeof(bool)))
                parametertype = ApplicationCommandOptionType.Boolean;
            else if (ReferenceEquals(type, typeof(DiscordChannel)))
                parametertype = ApplicationCommandOptionType.Channel;
            else if (ReferenceEquals(type, typeof(DiscordUser)))
                parametertype = ApplicationCommandOptionType.User;
            else if (ReferenceEquals(type, typeof(DiscordRole)))
                parametertype = ApplicationCommandOptionType.Role;
            else
                throw new ArgumentException("Cannot convert type! Argument types must be string, long, bool, DiscordChannel, DiscordUser or DiscordRole.");

            return parametertype;
        }
    }

    /*internal class CommandCreatePayload
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("options")]
        public List<DiscordApplicationCommandOption> Options = new List<DiscordApplicationCommandOption>();
    }*/

    internal class CommandMethod
    {
        public ulong Id;

        public string Name;
        public MethodInfo Method;
        public Type ParentClass;
    }

    internal class GroupCommand
    {
        public ulong Id;

        public string Name;
        public Dictionary<string, MethodInfo> Methods = null;
        public Type ParentClass;
    }

    internal class SubGroupCommand
    {
        public ulong Id;

        public string Name;
        public List<GroupCommand> SubCommands = new List<GroupCommand>();
    }
}