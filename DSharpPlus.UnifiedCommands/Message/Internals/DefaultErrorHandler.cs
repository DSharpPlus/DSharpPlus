using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Message.Internals;

/// <summary>
/// The default class for error handling. 
/// </summary>
internal class DefaultErrorHandler : IErrorHandler
{
    public Task HandleConversionAsync(InvalidMessageConversionError error, DiscordMessage message)
    {
        DiscordMessageBuilder msgBuilder = new();
        msgBuilder.WithReply(message.Id);
        string argumentName = error.Name.Any() ? string.Empty : $" `{error.Name}`";

        msgBuilder.WithAllowedMentions(Mentions.None);

        // This can be changed to a if else for .IsPositionalArgument
        switch (error.Type)
        {
            case InvalidMessageConversionType.NotAValidNumber:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"You cannot convert value `{error.Value}` to a number for argument{argumentName}"
                    : $"You cannot convert value `{error.Value}` to a number for option `{error.Name}`");

                break;
            case InvalidMessageConversionType.NotAValidInteger:
                msgBuilder.WithContent(
                    error.IsPositionalArgument
                        ? $"You cannot convert value `{error.Value}` to a integer for argument{argumentName}."
                        : $"You cannot convert value `{error.Value}` to a integer for option `{error.Name}`.");

                break;
            case InvalidMessageConversionType.NotAValidUser:
                msgBuilder.WithContent($"`{error.Value}` is not a valid user.");
                break;
            case InvalidMessageConversionType.UserDoesNotExist:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"Couldn't find the user for argument{argumentName}."
                    : $"Couldn't find the user for option `{error.Name}`.");

                break;
            case InvalidMessageConversionType.NotAValidMember:
                msgBuilder.WithContent($"`{error.Value}` is not a valid member.");
                break;
            case InvalidMessageConversionType.MemberDoesNotExist:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"Couldn't find the member for argument{argumentName}."
                    : $"Couldn't find the member for option `{error.Name}`.");

                break;
            case InvalidMessageConversionType.NotAValidChannel:
                msgBuilder.WithContent($"`{error.Value}` is not a valid channel.");
                break;
            case InvalidMessageConversionType.ChannelDoesNotExist:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"Couldn't find the channel for argument{argumentName}."
                    : $"Couldn't find the channel for option `{error.Name}`.");

                break;
            case InvalidMessageConversionType.BoolShouldNotHaveValue:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"Argument{argumentName} shouldn't have a value."
                    : $"Option `{error.Value}` shouldn't have a value.");

                break;
            case InvalidMessageConversionType.NoValueProvided:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"You need to provide a value for argument{argumentName}."
                    : $"You need to provide a value for option `{error.Name}`");

                break;
            case InvalidMessageConversionType.NotAValidRole:
                msgBuilder.WithContent($"`{error.Value}` is not a valid role.");
                break;
            case InvalidMessageConversionType.RoleDoesNotExist:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"Couldn't find the channel for argument{argumentName}."
                    : $"Couldn't find the channel for option `{error.Name}`.");

                break;
            default:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"Argument{argumentName} is invalid."
                    : $"Option `{error.Name}` is invalid.");

                break;
        }

        return message.Channel.SendMessageAsync(msgBuilder);
    }

    public Task HandleUnhandledExceptionAsync(Exception e, DiscordMessage message)
        => Task.CompletedTask;
}
