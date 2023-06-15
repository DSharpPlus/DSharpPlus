using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands.Message.Errors;
using Remora.Results;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.UnifiedCommands.Internals;

/// <summary>
/// The default class for error handling. 
/// </summary>
internal class DefaultErrorHandler : IErrorHandler
{
    public Task HandleInteractionErrorAsync(IResultError error, DiscordInteraction interaction, DiscordClient client)
    {
        client.Logger.LogError("Failed interaction with error: {Error}", error);
        return Task.CompletedTask;
    }

    public Task HandleMessageErrorAsync(IResultError error, DiscordMessage message, DiscordClient client)
    {
        client.Logger.LogError("Failed message command with error: {Error}", error);


        if (error is FailedConversionError e)
        {
            return message.Channel.SendMessageAsync($"Option `{e.Name}` is invalid.");
        }
        else
        {
            client.Logger.LogError("Failed message command with error: {Error}", error);
            return Task.CompletedTask;
        }
    }
}
