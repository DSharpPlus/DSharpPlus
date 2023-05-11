using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// A interface used in dependency injection to set the default error handler.
/// </summary>
public interface IErrorHandler
{
    /// <summary>
    /// Method for failed conversion.
    /// </summary>
    /// <param name="error">Object containing error information.</param>
    /// <param name="message">The discord message object</param>
    /// <returns></returns>
    public Task HandleConversionAsync(InvalidMessageConversionError error, DiscordMessage message);

    /// <summary>
    /// Method for unhandled exception. Gets executed if a exception happens during invocation of command method.
    /// </summary>
    /// <param name="exception">The exception that got caught.</param>
    /// <param name="message">The message object.</param>
    /// <returns></returns>
    public Task HandleUnhandledExceptionAsync(Exception exception, DiscordMessage message);
}
