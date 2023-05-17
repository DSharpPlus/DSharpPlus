using DSharpPlus.UnifiedCommands.Message.Conditions;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.UnifiedCommands.Message.Internals;

internal class MessageConditionHandler
{
    private readonly IReadOnlyList<Func<IServiceProvider, IMessageCondition>> _conditionBuilders;

    internal MessageConditionHandler(IReadOnlyList<Func<IServiceProvider, IMessageCondition>> conditionBuilders)
        => _conditionBuilders = conditionBuilders;

    internal async Task<bool> IterateConditionsAsync(MessageContext context, IServiceScope scope)
    {
        for (int i = 0; i < _conditionBuilders.Count; i++)
        {
            Func<IServiceProvider, IMessageCondition> func = _conditionBuilders[i];
            IMessageCondition condition = func(scope.ServiceProvider);
            ValueTask<bool> task = condition.InvokeAsync(context);
            bool result = task.IsCompletedSuccessfully ? task.Result : await task;
            if (!result)
            {
                return false;
            }
        }

        return true;
    }
}
