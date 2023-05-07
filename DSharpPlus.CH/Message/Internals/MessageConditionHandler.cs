using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.CH.Message.Conditions;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageConditionHandler
{
    private readonly IReadOnlyList<Func<IServiceProvider, IMessageCondition>> _conditionBuilders;

    internal MessageConditionHandler(IReadOnlyList<Func<IServiceProvider, IMessageCondition>> conditionBuilders)
        => _conditionBuilders = conditionBuilders;

    internal async Task<bool> StartGoingThroughConditionsAsync(MessageContext context, IServiceScope scope)
    {
        for (int i = 0; i < _conditionBuilders.Count; i++)
        {
            Func<IServiceProvider, IMessageCondition> func = _conditionBuilders[i];
            IMessageCondition condition = func(scope.ServiceProvider);
            Task<bool> task = condition.InvokeAsync(context);
            bool result;
            if (task.IsCompletedSuccessfully)
            {
                result = task.Result;
            }
            else
            {
                result = await task;
            }

            if (!result)
            {
                return false;
            }
        }

        return true;
    }
}
