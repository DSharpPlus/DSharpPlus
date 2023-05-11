using System.Reflection;
using System.Runtime.CompilerServices;
using DSharpPlus.CH.Application.Conditions;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Application.Internals;

internal class ApplicationHandler
{
    private ApplicationModule _module = null!;
    private DiscordInteraction _interaction;
    private ApplicationMethodData _data;
    private readonly IReadOnlyList<Func<IServiceProvider, IApplicationCondition>> _conditionBuilders;
    private object?[]? _args;
    private IServiceScope _scope;

    internal ApplicationHandler(ApplicationMethodData data, DiscordInteraction interaction,
        IReadOnlyList<Func<IServiceProvider, IApplicationCondition>> conditionBuilders, object?[]? args,
        IServiceScope scope)
    {
        _data = data;
        _interaction = interaction;
        _conditionBuilders = conditionBuilders;
        _args = args;
        _scope = scope;
    }

    internal async Task TurnResultIntoActionAsync(IApplicationResult result)
    {
        DiscordInteractionResponseBuilder builder = new();

        void SetDefaultContent(DiscordInteractionResponseBuilder responseBuilder)
        {
            if (result.Content is not null)
            {
                responseBuilder.WithContent(result.Content);
            }

            if (result.Embeds is not null)
            {
                responseBuilder.AddEmbeds(result.Embeds);
            }
        }

        switch (result.Type)
        {
            case ApplicationResultType.Reply:
                SetDefaultContent(builder);
                await _module.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    builder);
                break;
        }
    }

    internal async Task BuildModuleAndExecuteCommandAsync()
    {
        _module = (ApplicationModule)_data.Module.Factory.Invoke(_scope.ServiceProvider, null);

        if (_data.ReturnsNothing)
        {
            if (_data.IsAsync)
            {
                Task<IApplicationResult> task = (Task<IApplicationResult>)_data.Method.Invoke(_module, _args)!;
                await TurnResultIntoActionAsync(await task);
            }
            else
            {
                IApplicationResult result = (IApplicationResult)_data.Method.Invoke(_module, _args)!;
                await TurnResultIntoActionAsync(result);
            }
        }
        else
        {
            if (_data.IsAsync)
            {
                Task task = (Task)_data.Method.Invoke(_module, _args)!;
                await task;
            }
            else
            {
                _data.Method.Invoke(_module, _args);
            }
        }
    }
}
