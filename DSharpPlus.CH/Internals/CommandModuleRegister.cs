using System.Reflection;
using DSharpPlus.CH.Message.Internals;
using DSharpPlus.CH.Message;
using DSharpPlus.Entities;

namespace DSharpPlus.CH.Internals;

internal class CommandModuleRegister
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
            MessageCommandModuleData data = new(@class);
            MessageCommandModuleData module = new(@class);

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
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    MessageOptionAttribute? paramAttribute = parameter.GetCustomAttribute<MessageOptionAttribute>();
                    MessageCommandParameterData parameterData = new();
                    if (paramAttribute is null)
                    {
                        parameterData.IsPositionalArgument = true;
                        parameterData.Name = parameter.Name ?? string.Empty;
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
                        throw new NotImplementedException();
                    }

                    parameters.Add(parameterData);
                }

                bool isAsync = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) is not null;
                MessageCommandMethodData methodData = new()
                {
                    Module = module, Method = method, IsAsync = isAsync, Parameters = parameters
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
}
