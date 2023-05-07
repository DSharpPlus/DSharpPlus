using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message.Internals;

/// <summary>
/// The default class for error handling. 
/// </summary>
internal class DefaultFailedErrors : IFailedErrors
{
    public async Task HandleConversionAsync(InvalidMessageConvertionError error, DiscordMessage message)
    {
        DiscordMessageBuilder msgBuilder = new();
        msgBuilder.WithReply(message.Id);
        string argumentName = error.Name.Count() == 0 ? string.Empty : $" `{error.Name}`";

        msgBuilder.WithAllowedMentions(Mentions.None);

        // This can be changed to a if else for .IsPositionalArgument
        switch (error.Type)
        {
            case InvalidMessageConvertionType.NotAValidNumber:
                if (error.IsPositionalArgument)
                {
                    msgBuilder.WithContent(
                        $"You cannot convert value `{error.Value}` to a number for argument{argumentName}");
                }
                else
                {
                    msgBuilder.WithContent(
                        $"You cannot convert value `{error.Value}` to a number for option `{error.Name}`");
                }

                break;
            case InvalidMessageConvertionType.NotAValidInteger:
                msgBuilder.WithContent(
                    error.IsPositionalArgument
                        ? $"You cannot convert value `{error.Value}` to a integer for argument{argumentName}."
                        : $"You cannot convert value `{error.Value}` to a integer for option `{error.Name}`.");

                break;
            case InvalidMessageConvertionType.NotAValidUser:
                msgBuilder.WithContent($"`{error.Value}` is not a valid user.");
                break;
            case InvalidMessageConvertionType.UserDoesNotExist:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"Couldn't find the user for argument{argumentName}."
                    : $"Couldn't find the user for option `{error.Name}`.");

                break;
            case InvalidMessageConvertionType.NotAValidMember:
                msgBuilder.WithContent($"`{error.Value}` is not a valid member.");
                break;
            case InvalidMessageConvertionType.MemberDoesNotExist:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"Couldn't find the member for argument{argumentName}."
                    : $"Couldn't find the member for option `{error.Name}`.");

                break;
            case InvalidMessageConvertionType.NotAValidChannel:
                msgBuilder.WithContent($"`{error.Value}` is not a valid channel.");
                break;
            case InvalidMessageConvertionType.ChannelDoesNotExist:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"Couldn't find the channel for argument{argumentName}."
                    : $"Couldn't find the channel for option `{error.Name}`.");

                break;
            case InvalidMessageConvertionType.BoolShouldNotHaveValue:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"Argument{argumentName} shouldn't have a value."
                    : $"Option `{error.Value}` shouldn't have a value.");

                break;
            case InvalidMessageConvertionType.NoValueProvided:
                msgBuilder.WithContent(error.IsPositionalArgument
                    ? $"You need to provide a value for argument{argumentName}."
                    : $"You need to provide a value for option `{error.Name}`");

                break;
            case InvalidMessageConvertionType.NotAValidRole:
                msgBuilder.WithContent($"`{error.Value}` is not a valid role.");
                break;
            case InvalidMessageConvertionType.RoleDoesNotExist:
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

        await message.Channel.SendMessageAsync(msgBuilder);
    }

    public Task HandleUnhandledExceptionAsync(Exception e, DiscordMessage message)
        => Task.CompletedTask;
}
