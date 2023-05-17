using System.Reflection;
using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands.Application;
using DSharpPlus.UnifiedCommands.Application.Internals;
using DSharpPlus.UnifiedCommands.Exceptions;
using DSharpPlus.UnifiedCommands.Message;
using DSharpPlus.UnifiedCommands.Message.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.UnifiedCommands.Internals;

internal static class CommandModuleRegister
{
    internal static void RegisterMessageCommands(MessageFactory factory, IReadOnlyCollection<Assembly> assemblies)
    {
        IEnumerable<Type> classes = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        t.GetCustomAttribute<MessageModuleAttribute>() is not null &&
                        t.IsPublic &&
                        t.IsSubclassOf(typeof(MessageModule)));

        NullabilityInfoContext nullabilityContext = new();

        foreach (Type? @class in classes)
        {
            MessageModuleData module = new()
            {
                Factory = ActivatorUtilities.CreateFactory(@class, Array.Empty<Type>())
            };

            MethodInfo[] methods = @class.GetMethods();
            if (methods.Length == 0)
            {
                continue; // Not worth saving this class if it is has no methods.
            }

            foreach (MethodInfo method in methods)
            {
                MessageAttribute? attribute = method.GetCustomAttribute<MessageAttribute>();
                if (attribute is null || !method.IsPublic)
                {
                    continue;
                }

                string[] moduleName = @class.GetCustomAttribute<MessageModuleAttribute>()?.Name?.Split(' ') ??
                                      Array.Empty<string>();
                string[] methodName = attribute.Name.Split(' ');

                List<MessageParameterData> parameters = new();
                bool cantConsumeMoreArguments = false;
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    MessageOptionAttribute? paramAttribute = parameter.GetCustomAttribute<MessageOptionAttribute>();
                    MessageParameterData parameterData = new();

                    if (paramAttribute is null)
                    {
                        if (cantConsumeMoreArguments)
                        {
                            throw new InvalidMessageModuleStructure(
                                $"You cannot have more arguments for method {method.Name}.");
                        }

                        parameterData.IsPositionalArgument = true;
                        parameterData.Name = parameter.Name ?? string.Empty;

                        if (
                            parameter.GetCustomAttribute<RemainingArgumentsAttribute>() is not null)
                        {
                            if (parameter.ParameterType != typeof(string))
                            {
                                throw new InvalidMessageModuleStructure(
                                    "You need to use string if you are using RemainingArgumentsAttribute.");
                            }

                            parameterData.WillConsumeRestOfArguments = true;
                            cantConsumeMoreArguments = true;
                        }
                    }
                    else
                    {
                        parameterData.IsPositionalArgument = false;
                        parameterData.Name = paramAttribute.Option;
                        parameterData.ShorthandOptionName = paramAttribute.ShorthandOption;
                    }


                    parameterData.CanBeNull =
                        nullabilityContext.Create(parameter).WriteState is NullabilityState.Nullable;
                    Type parameterType =
                        Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;
                    parameterData.HasDefaultValue = parameter.HasDefaultValue;

                    // I don't know how I would make this into a switch case. 
                    if (parameterType == typeof(string))
                    {
                        parameterData.Type = MessageParameterDataType.String;
                    }
                    else if (parameterType == typeof(int))
                    {
                        parameterData.Type = MessageParameterDataType.Int;
                    }
                    else if (parameterType == typeof(DiscordUser))
                    {
                        parameterData.Type = MessageParameterDataType.User;
                    }
                    else if (parameterType == typeof(DiscordRole))
                    {
                        parameterData.Type = MessageParameterDataType.Role;
                    }
                    else if (parameterType == typeof(bool))
                    {
                        parameterData.Type = MessageParameterDataType.Bool;
                    }
                    else if (parameterType == typeof(DiscordMember))
                    {
                        parameterData.Type = MessageParameterDataType.Member;
                    }
                    else if (parameterType == typeof(DiscordChannel))
                    {
                        parameterData.Type = MessageParameterDataType.Channel;
                    }
                    else
                    {
                        parameterData.Type = parameterType == typeof(double)
                            ? MessageParameterDataType.Double
                            : throw new InvalidMessageModuleStructure(
                                $"You cannot use type {parameterType.Name} as a parameter.");
                    }

                    parameters.Add(parameterData);
                }

                bool isAsync = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) is not null;
                bool returnsNothing = method.ReturnType == typeof(void)
                                      || (method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) is not null &&
                                          method.ReturnType.GenericTypeArguments.Length == 0);
                MessageMethodData methodData = new()
                {
                    Module = module,
                    Method = method,
                    IsAsync = isAsync,
                    Parameters = parameters,
                    ReturnsNothing = returnsNothing
                };
                if (moduleName.Length != 0)
                {
                    MessageTree? tree = null;
                    foreach (string name in moduleName)
                    {
                        tree = factory.GetBranch(name);
                        if (tree is null)
                        {
                            tree = new();
                            factory.AddBranch(name, tree);
                        }
                    }

                    for (int i = 0; i < methodName.Length; i++)
                    {
                        string name = methodName[i];
                        if (i == methodName.Length - 1)
                        {
                            tree?.Branches?.Add(name, new(methodData));
                        }
                        else
                        {
                            MessageTree tempTree = new();
                            tree?.Branches?.Add(name, tempTree);
                            tree = tempTree;
                        }
                    }
                }
                else
                {
                    factory.AddCommand(attribute.Name, methodData);
                }
            }
        }
    }

    internal static List<DiscordApplicationCommand> RegisterApplicationCommands(ApplicationFactory factory,
        IReadOnlyCollection<Assembly> assemblies,
        DiscordClient client)
    {
        IEnumerable<Type> classes = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.IsPublic &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        t.IsSubclassOf(typeof(ApplicationModule)) &&
                        t.GetCustomAttribute<ApplicationModuleAttribute>() is not null);

        NullabilityInfoContext nullabilityContext = new();

        List<DiscordApplicationCommand> commands = new();
        foreach (Type @class in classes)
        {
            ObjectFactory objectFactory = ActivatorUtilities.CreateFactory(@class, Array.Empty<Type>());
            ApplicationModuleData moduleData = new(objectFactory);

            bool registerAsSubcommands = false;
            ApplicationModuleAttribute? moduleAttribute = @class.GetCustomAttribute<ApplicationModuleAttribute>();
            if (moduleAttribute?.Name is not null)
            {
                registerAsSubcommands = true;
            }

            DiscordApplicationCommandBuilder? commandBuilder = null;
            if (registerAsSubcommands)
            {
                commandBuilder = new();
                commandBuilder.WithName(moduleAttribute!.Name!).WithDescription(moduleAttribute.Description!)
                    .WithType(ApplicationCommandType.SlashCommand);
            }

            foreach (MethodInfo method in @class.GetMethods())
            {
                ApplicationNameAttribute? attribute = method.GetCustomAttribute<ApplicationNameAttribute>();
                if (attribute is null)
                {
                    continue;
                }

                string applicationName =
                    registerAsSubcommands ? $"{moduleAttribute!.Name} {attribute.Name}" : attribute.Name;
                DiscordApplicationCommandOptionBuilder? option = null;
                if (!registerAsSubcommands)
                {
                    commandBuilder = new();
                    commandBuilder.WithType(ApplicationCommandType.SlashCommand).WithName(attribute.Name)
                        .WithDescription(attribute.Description);
                }
                else
                {
                    option = new();
                    option.WithName(attribute.Name).WithDescription(attribute.Description)
                        .WithType(ApplicationCommandOptionType.SubCommand);
                }

                List<ApplicationMethodParameterData> parameters = new();
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    ApplicationOptionAttribute? name = parameter.GetCustomAttribute<ApplicationOptionAttribute>();
                    if (name is null)
                    {
                        throw new Exception("Parameter needs to have `ApplicationNameAttribute` marked.");
                    }

                    ApplicationMethodParameterData data = new(name.Name)
                    {
                        IsNullable = nullabilityContext.Create(parameter).WriteState is NullabilityState.Nullable
                    };

                    if (parameter.ParameterType == typeof(string))
                    {
                        data.Type = ApplicationCommandOptionType.String;
                    }
                    else if (parameter.ParameterType == typeof(long))
                    {
                        data.Type = ApplicationCommandOptionType.Integer;
                    }
                    else if (parameter.ParameterType == typeof(double))
                    {
                        data.Type = ApplicationCommandOptionType.Number;
                    }
                    else if (parameter.ParameterType == typeof(bool))
                    {
                        data.Type = ApplicationCommandOptionType.Boolean;
                    }
                    else if (parameter.ParameterType == typeof(DiscordUser))
                    {
                        data.Type = ApplicationCommandOptionType.User;
                    }
                    else if (parameter.ParameterType == typeof(DiscordChannel))
                    {
                        data.Type = ApplicationCommandOptionType.Channel;
                    }
                    else if (parameter.ParameterType == typeof(DiscordRole))
                    {
                        data.Type = ApplicationCommandOptionType.Role;
                    }
                    else if (parameter.ParameterType.IsSubclassOf(typeof(IMention)) ||
                             parameter.ParameterType == typeof(IMention))
                    {
                        data.Type = ApplicationCommandOptionType.Mentionable;
                    }
                    else
                    {
                        data.Type = parameter.ParameterType == typeof(DiscordAttachment)
                            ? ApplicationCommandOptionType.Attachment
                            : throw new Exception("Not a valid parameter type.");
                    }

                    parameters.Add(data);
                    if (registerAsSubcommands)
                    {
                        option!.AddOption(new DiscordApplicationCommandOption(name.Name, name.Description,
                            data.Type,
                            !data.IsNullable));
                    }
                    else
                    {
                        commandBuilder!.AddOption(new DiscordApplicationCommandOption(name.Name, name.Description,
                            data.Type,
                            !data.IsNullable));
                    }
                }

                if (registerAsSubcommands)
                {
                    commandBuilder!.AddOption(option!.Build());
                }

                bool isAsync = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) is not null;
                bool returnsNothing = method.ReturnType == typeof(void) ||
                                      (method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) is not null &&
                                       method.ReturnType.GenericTypeArguments.Length == 0);

                ApplicationMethodData methodData =
                    new()
                    {
                        IsAsync = isAsync,
                        ReturnsNothing = returnsNothing,
                        Method = method,
                        Parameters = parameters,
                        Module = moduleData
                    };
                factory._methods.Add(applicationName, methodData);

                if (!registerAsSubcommands)
                {
                    commands.Add(commandBuilder!.Build());
                }
            }

            if (registerAsSubcommands)
            {
                commands.Add(commandBuilder!.Build());
            }
        }

        return commands;
    }
}
