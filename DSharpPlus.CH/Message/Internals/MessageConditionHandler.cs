using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageConditionHandler
{
    private readonly List<Type> _conditions;

    internal MessageConditionHandler(List<Type> conditions)
        => _conditions = conditions;

    internal async Task<bool> StartGoingThroughConditionsAsync(MessageContext context, IServiceScope scope)
    {
        IMessageCondition[] conditionQueue = new IMessageCondition[_conditions.Count];

        for (int i = 0; i < _conditions.Count; i++)
        {
            Type type = _conditions[i];

            object?[]? constructorParameters = null;
            if (type.GetConstructors().Length != 0)
            {
                ConstructorInfo constructor = type.GetConstructors()[0];
                if (constructor.GetParameters().Length != 0)
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    constructorParameters = new object?[parameters.Length];

                    for (int ii = 0; ii < parameters.Length; ii++)
                    {
                        constructorParameters[ii] = scope.ServiceProvider.GetService(parameters[ii].ParameterType);
                    }
                }
            }

            IMessageCondition condition = (IMessageCondition)Activator.CreateInstance(type, constructorParameters)!;
            conditionQueue[i] = condition;
        }

        bool executeCommand = true;
        foreach (IMessageCondition condition in conditionQueue)
        {
            if (await condition.InvokeAsync(context))
            {
                executeCommand = false;
            }
        }

        return executeCommand;
    }
}
