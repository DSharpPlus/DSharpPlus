using System.Reflection;
using DSharpPlus.CH.Message.Internals;
using DSharpPlus.CH.Message;

namespace DSharpPlus.CH.Internals
{
    internal class CommandModuleRegister
    {
        internal static void RegisterMessageCommands(MessageCommandFactory factory, Assembly assembly)
        {
            var classes = assembly.GetTypes()
                .Where(t => t.IsClass &&
                    !t.IsAbstract &&
                    t.GetCustomAttribute<MessageModuleAttribute>() is not null &&
                    t.IsPublic &&
                    t.IsSubclassOf(typeof(MessageCommandModule)));

            foreach (var @class in classes)
            {
                var data = new MessageCommandModuleData(@class);
                var module = new MessageCommandModuleData(@class);

                var methods = @class.GetMethods();
                if (methods is null) continue; // Not worth saving this class if it is has no methods.

                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<MessageCommandAttribute>();
                    if (attribute is null || !method.IsPublic) continue;

                    var parameters = new List<MessageCommandParameterData>();
                    foreach (var parameter in method.GetParameters())
                    {
                        var paramAttribute = parameter.GetCustomAttribute<MessageOptionAttribute>();
                        var parameterData = new MessageCommandParameterData();
                        if (paramAttribute is null)
                        {
                            parameterData.IsArgument = true;
                            parameterData.Name = parameter.Name ?? string.Empty;
                        }
                        else
                        {
                            parameterData.IsArgument = false;
                            parameterData.Name = paramAttribute.Option;
                            parameterData.ShorthandOptionName = paramAttribute.ShorthandOption;
                        }

                        if (Nullable.GetUnderlyingType(parameter.ParameterType) is not null) parameterData.CanBeNull = true;
                        else parameterData.CanBeNull = false;

                        var parameterType = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;

                        // I don't know how I would make this into a switch case. 
                        if (parameterType == typeof(string))
                            parameterData.Type = MessageCommandParameterDataType.String;
                        else if (parameterType == typeof(int))
                            parameterData.Type = MessageCommandParameterDataType.Int;
                        else if (parameterType == typeof(DSharpPlus.Entities.DiscordUser))
                            parameterData.Type = MessageCommandParameterDataType.User;
                        else throw new NotImplementedException();

                        parameters.Add(parameterData);

                    }

                    bool isAsync = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) is not null;
                    factory.AddCommand(attribute.Name, new MessageCommandMethodData
                    {
                        Module = module,
                        Method = method,
                        IsAsync = isAsync,
                        Parameters = parameters,
                    });
                }
            }
        }
    }
}
