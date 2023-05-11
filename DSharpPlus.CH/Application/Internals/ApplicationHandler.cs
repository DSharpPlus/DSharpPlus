using System.Reflection;
using System.Runtime.CompilerServices;
using DSharpPlus.CH.Application.Conditions;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.CH.Application.Internals;

internal class ApplicationHandler
{
    private DiscordClient _client;
    private ApplicationModule _module = null!;
    private DiscordInteraction _interaction;
    private ApplicationMethodData _data;
    private readonly IReadOnlyList<Func<IServiceProvider, IApplicationCondition>> _conditionBuilders;
    private object?[]? _args;
    private IServiceScope _scope;

    internal ApplicationHandler(ApplicationMethodData data, DiscordInteraction interaction,
        IReadOnlyList<Func<IServiceProvider, IApplicationCondition>> conditionBuilders, object?[]? args,
        IServiceScope scope, DiscordClient client)
    {
        _data = data;
        _interaction = interaction;
        _conditionBuilders = conditionBuilders;
        _args = args;
        _scope = scope;
        _client = client;
    }

    internal async Task TurnResultIntoActionAsync(IApplicationResult result)
    {
        _client.Logger.LogInformation("Reached here");
        switch (result.Type)
        {
            case ApplicationResultType.Reply:
                DiscordInteractionResponseBuilder builder = new();
                if (result.Content is not null)
                {
                    builder.WithContent(result.Content);
                }

                if (result.Embeds is not null)
                {
                    builder.AddEmbeds(result.Embeds);
                }
                await _module.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    builder);
                break;
            case ApplicationResultType.FollowUp:
                DiscordFollowupMessageBuilder followUpBuilder = new();
                if (result.Content is not null)
                {
                    followUpBuilder.WithContent(result.Content);
                }

                if (result.Embeds is not null)
                {
                    followUpBuilder.AddEmbeds(result.Embeds);
                }
                await _module.Interaction.CreateFollowupMessageAsync(followUpBuilder);
                break;
        }
    }

    internal async Task BuildModuleAndExecuteCommandAsync()
    {
        try
        {
            _module = (ApplicationModule)_data.Module.Factory.Invoke(_scope.ServiceProvider, null);
            _module.Interaction = _interaction;
            _module.Client = _client;
            _module._handler = this;

            try
            {
                if (!_data.ReturnsNothing)
                {
                    if (_data.IsAsync)
                    {
                        Task<IApplicationResult> task =
                            (Task<IApplicationResult>)_data.Method.Invoke(_module, BindingFlags.OptionalParamBinding,
                                null,
                                _args, null)!;
                        await TurnResultIntoActionAsync(await task);
                    }
                    else
                    {
                        _client.Logger.LogInformation("Executing command");
                        IApplicationResult result = (IApplicationResult)_data.Method.Invoke(_module,
                            BindingFlags.OptionalParamBinding, null, _args, null)!;
                        await TurnResultIntoActionAsync(result);
                    }
                }
                else
                {
                    if (_data.IsAsync)
                    {
                        Task task =
                            (Task)_data.Method.Invoke(_module, BindingFlags.OptionalParamBinding, null, _args, null)!;
                        await task;
                    }
                    else
                    {
                        _data.Method.Invoke(_module, BindingFlags.OptionalParamBinding, null, _args, null);
                    }
                }
            }
            catch (TargetInvocationException e)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
                throw e.InnerException;
            }
            catch (AggregateException e)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
                throw e.InnerException;
            }
        }
        catch (Exception e)
        {
            _client.Logger.LogError(
                "Exception was thrown while trying to execute command {MethodName}. Was thrown from {Exception}",
                _data.Method.Name, e.ToString());
        }
        finally
        {
            _scope.Dispose();
        }
    }
}
