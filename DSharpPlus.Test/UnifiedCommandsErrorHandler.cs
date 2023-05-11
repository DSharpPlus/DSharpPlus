using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands.Message;

namespace DSharpPlus.Test;

public class UnifiedCommandsErrorHandler : IErrorHandler
{
    public async Task HandleUnhandledExceptionAsync(Exception exception, DiscordMessage message)
    {
        DiscordMessageBuilder builder = new();
        builder.WithContent("Something unexpected happened when executing the command.");
        builder.WithReply(message.Id);

        await message.Channel.SendMessageAsync(builder);
    }

    public async Task HandleConversionAsync(InvalidMessageConversionError error, DiscordMessage message)
    {
        DiscordMessageBuilder builder = new();
        builder.WithContent($"Value `{error.Value}` is invalid.");
        builder.WithAllowedMentions(Mentions.None);
        
        await message.Channel.SendMessageAsync(builder);
    }
}
