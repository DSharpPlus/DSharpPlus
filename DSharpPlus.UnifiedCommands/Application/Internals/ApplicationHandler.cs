using System.Reflection;
using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands.Application.Conditions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.ExceptionServices;

namespace DSharpPlus.UnifiedCommands.Application.Internals;

internal class ApplicationHandler
{
    private readonly DiscordClient _client;
    private ApplicationModule _module = null!;
    private readonly DiscordInteraction _interaction;
    private readonly ApplicationMethodData _data;
    private readonly object?[]? _args;
    private readonly IServiceScope _scope;
    private readonly IReadOnlyList<Type> _conditions;

    internal ApplicationHandler(ApplicationMethodData data, DiscordInteraction interaction, object?[]? args,
        IServiceScope scope, DiscordClient client, IReadOnlyList<Type> conditions)
    {
        _data = data;
        _interaction = interaction;
        _args = args;
        _scope = scope;
        _client = client;
        _conditions = conditions;
    }

    internal Task TurnResultIntoActionAsync(IApplicationResult result)
    {
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

                return _module.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
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

                return _module.Interaction.CreateFollowupMessageAsync(followUpBuilder);
                break;
        }

        return
            Task.FromException(
                new Exception()); // This is here as a place holder until all ApplicationResultType's is implemented into the swtich case
    }

    internal async ValueTask BuildModuleAndExecuteCommandAsync()
    {
        try
        {
            _module = (ApplicationModule)_data.Module.Factory.Invoke(_scope.ServiceProvider, null);
            _module.Interaction = _interaction;
            _module.Client = _client;
            _module._handler = this;

            foreach (Type type in _conditions)
            {
                object obj = _scope.ServiceProvider.GetRequiredService(type);
                ValueTask<bool> task = ((IApplicationCondition)obj).InvokeAsync(_interaction, _client);
                await task;
            }

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
                        IApplicationResult result = (IApplicationResult)_data.Method.Invoke(_module,
                            BindingFlags.OptionalParamBinding, null, _args, null)!;
                        await TurnResultIntoActionAsync(result);
                    }
                }
                else
                {
                    if (_data.IsAsync)
                    {
                        // TODO: Support ValueTask invoking.
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
                // ExceptionDispatchInfo can be used to rethrow a error and perceive all the info in it.
                // This is done here to rethrow the inner error and show where it is so people who don't read in depth their stacktrace doesn't blame us.
                ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
                throw e.InnerException;
            }
            catch (AggregateException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
                throw e.InnerException;
            }
        }
        catch (Exception e)
        {
            _client.Logger.LogError(
                e,
                "Exception was thrown while trying to execute command {MethodName}. Was thrown from:",
                _data.Method.Name);
        }
        finally
        {
            _scope.Dispose();
        }
    }
}
