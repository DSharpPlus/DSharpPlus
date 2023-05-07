using System;
using System.Threading.Tasks;
using DSharpPlus.CH.Message;
using DSharpPlus.Entities;

namespace DSharpPlus.Test;

public class CHErrorHandler : IErrorHandler
{
    public async Task HandleUnhandledExceptionAsync(Exception exception, DiscordMessage message)
    {
        DiscordMessageBuilder builder = new();
        builder.WithContent("Something unexpected happened when executing the command.");
        builder.WithReply(message.Id);

        await message.Channel.SendMessageAsync(builder);
    }

    public async Task HandleConversionAsync(InvalidMessageConvertionError error, DiscordMessage message)
    {
        DiscordMessageBuilder builder = new();
        builder.WithContent($"Value `{error.Value}` is invalid.");
        builder.WithAllowedMentions(Mentions.None);
        
        await message.Channel.SendMessageAsync(builder);
    }
}
