using DSharpPlus.Entities;
using Remora.Results;

namespace DSharpPlus.UnifiedCommands;

// TODO: Port support to application commands. And allow custom errors somehow.
/// <summary>
/// A interface used in dependency injection to set the default error handler.
/// </summary>
public interface IErrorHandler
{
    public Task HandleMessageErrorAsync(IResultError error, DiscordMessage message, DiscordClient client);

    public Task HandleInteractionErrorAsync(IResultError error, DiscordInteraction interaction, DiscordClient client);
}
