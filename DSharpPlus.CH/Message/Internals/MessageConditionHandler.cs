using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageConditionHandler
{
    private readonly List<Func<IServiceProvider, IMessageCondition>> _conditionConstructor;

    internal MessageConditionHandler(List<Func<IServiceProvider, IMessageCondition>> conditionConstructor)
        => _conditionConstructor = conditionConstructor;

    internal async Task<bool> StartGoingThroughConditionsAsync(MessageContext context, IServiceScope scope)
    {
        for (int i = 0; i < _conditionConstructor.Count; i++)
        {
            Func<IServiceProvider, IMessageCondition> func = _conditionConstructor[i];
            IMessageCondition condition = func(scope.ServiceProvider);
            if (!await condition.InvokeAsync(context))
            {
                return false;
            }
        }

        return true;
    }
}
