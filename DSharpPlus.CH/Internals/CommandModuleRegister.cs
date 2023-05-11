using System.Reflection;
using DSharpPlus.CH.Application;
using DSharpPlus.CH.Application.Internals;
using DSharpPlus.CH.Message.Internals;
using DSharpPlus.CH.Message;
using DSharpPlus.CH.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Internals;

internal static class CommandModuleRegister
{
    internal static void RegisterMessageCommands(MessageCommandFactory factory, Assembly assembly)
    {
        IEnumerable<Type> classes = assembly.GetTypes()
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        t.GetCustomAttribute<MessageModuleAttribute>() is not null &&
                        t.IsPublic &&
                        t.IsSubclassOf(typeof(MessageCommandModule)));

        NullabilityInfoContext nullabilityContext = new();

        foreach (Type? @class in classes)
        {
            MessageCommandModuleData module = new();
            module.Factory = ActivatorUtilities.CreateFactory(@class, Array.Empty<Type>());

            MethodInfo[] methods = @class.GetMethods();
            if (methods.Length == 0)
            {
                continue; // Not worth saving this class if it is has no methods.
            }

            foreach (MethodInfo method in methods)
            {
                MessageCommandAttribute? attribute = method.GetCustomAttribute<MessageCommandAttribute>();
                if (attribute is null || !method.IsPublic)
                {
                    continue;
                }

                string[] moduleName = @class.GetCustomAttribute<MessageModuleAttribute>()?.Name?.Split(' ') ??
                                      Array.Empty<string>();
                string[] methodName = attribute.Name.Split(' ');

                List<MessageCommandParameterData> parameters = new();
                bool cantConsumeMoreArguments = false;
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    MessageOptionAttribute? paramAttribute = parameter.GetCustomAttribute<MessageOptionAttribute>();
                    MessageCommandParameterData parameterData = new();

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
                                    "You need to use string if you are using RemainingTextAttribute.");
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
                        parameterData.Type = MessageCommandParameterDataType.String;
                    }
                    else if (parameterType == typeof(int))
                    {
                        parameterData.Type = MessageCommandParameterDataType.Int;
                    }
                    else if (parameterType == typeof(DiscordUser))
                    {
                        parameterData.Type = MessageCommandParameterDataType.User;
                    }
                    else if (parameterType == typeof(DiscordRole))
                    {
                        parameterData.Type = MessageCommandParameterDataType.Role;
                    }
                    else if (parameterType == typeof(bool))
                    {
                        parameterData.Type = MessageCommandParameterDataType.Bool;
                    }
                    else if (parameterType == typeof(DiscordMember))
                    {
                        parameterData.Type = MessageCommandParameterDataType.Member;
                    }
                    else if (parameterType == typeof(DiscordChannel))
                    {
                        parameterData.Type = MessageCommandParameterDataType.Channel;
                    }
                    else if (parameterType == typeof(double))
                    {
                        parameterData.Type = MessageCommandParameterDataType.Double;
                    }
                    else
                    {
                        throw new InvalidMessageModuleStructure(
                            $"You cannot use type {parameterType.Name} as a parameter.");
                    }

                    parameters.Add(parameterData);
                }

                bool isAsync = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) is not null;
                bool returnsNothing = method.ReturnType == typeof(void)
                                      || (method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) is not null &&
                                          method.ReturnType.GenericTypeArguments.Length == 0);
                MessageCommandMethodData methodData = new()
                {
                    Module = module,
                    Method = method,
                    IsAsync = isAsync,
                    Parameters = parameters,
                    ReturnsNothing = returnsNothing
                };
                if (moduleName.Length != 0)
                {
                    MessageCommandTree? tree = null;
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
                            MessageCommandTree tempTree = new();
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

    internal static void RegisterApplicationCommands(ApplicationFactory factory, Assembly assembly)
    {
        IEnumerable<Type> classes = assembly.GetTypes()
            .Where(t => t.IsPublic &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        t.IsSubclassOf(typeof(ApplicationModule)) &&
                        t.GetCustomAttribute<ApplicationModuleAttribute>() is not null);

        NullabilityInfoContext nullabilityContext = new();

        foreach (Type @class in classes)
        {
            ObjectFactory objectFactory = ActivatorUtilities.CreateFactory(@class, Array.Empty<Type>());
            ApplicationModuleData moduleData = new(objectFactory);

            foreach (MethodInfo method in @class.GetMethods())
            {
                ApplicationNameAttribute? attribute = method.GetCustomAttribute<ApplicationNameAttribute>();
                if (attribute is null)
                {
                    continue;
                }

                List<ApplicationMethodParameterData> parameters = new();
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    ApplicationNameAttribute? name = parameter.GetCustomAttribute<ApplicationNameAttribute>();
                    if (name is null)
                    {
                        throw new Exception("Parameter needs to have `ApplicationNameAttribute` marked.");
                    }

                    ApplicationMethodParameterData data = new(name.Name);
                    data.IsNullable = nullabilityContext.Create(parameter).WriteState is NullabilityState.Nullable;

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
                    else if (parameter.ParameterType == typeof(DiscordAttachment))
                    {
                        data.Type = ApplicationCommandOptionType.Attachment;
                    }
                    else
                    {
                        throw new Exception("Not a valid parameter type.");
                    }

                    parameters.Add(data);
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
                factory._methods.Add(attribute.Name, methodData);
            }
        }
    }
}
