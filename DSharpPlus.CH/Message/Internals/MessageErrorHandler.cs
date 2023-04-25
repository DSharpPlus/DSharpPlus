using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message.Internals
{
    /// <summary>
    /// The default class for error handling. 
    /// </summary>
    internal static class MessageErrorHandler
    {
        internal static async Task HandleInvalidOption(InvalidMessageConvertionError error, DiscordMessage message)
        {
            var msgBuilder = new DiscordMessageBuilder();
            msgBuilder.WithReply(message.Id);
            string argumentName = error.Name.Count() == 0 ? string.Empty : $" `{error.Name}`";

            switch (error.Type)
            {
                case InvalidMessageConvertionType.NotAValidNumber:
                    if (error.IsArgument)
                        msgBuilder.WithContent($"You cannot convert value `{error.Value}` to a number for argument{argumentName}");
                    else
                        msgBuilder.WithContent($"You cannot convert value `{error.Value}` to a number for option `{error.Name}`");
                    break;
                case InvalidMessageConvertionType.NotAValidInteger:
                    if (error.IsArgument)
                        msgBuilder.WithContent($"You cannot convert value `{error.Value}` to a integer for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"You cannot convert value `{error.Value}` to a integer for option `{error.Name}`.");
                    break;
                case InvalidMessageConvertionType.NotAValidUser:
                    msgBuilder.WithContent($"`{error.Value}` is not a valid user.");
                    break;
                case InvalidMessageConvertionType.UserDoesNotExist:
                    if (error.IsArgument)
                        msgBuilder.WithContent($"Couldn't find the user for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"Couldn't find the user for option `{error.Name}`.");
                    break;
                case InvalidMessageConvertionType.NotAValidMember:
                    msgBuilder.WithContent($"`{error.Value}` is not a valid member.");
                    break;
                case InvalidMessageConvertionType.MemberDoesNotExist:
                    if (error.IsArgument)
                        msgBuilder.WithContent($"Couldn't find the member for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"Couldn't find the member for option `{error.Name}`.");
                    break;
                case InvalidMessageConvertionType.NotAValidChannel:
                    msgBuilder.WithContent($"`{error.Value}` is not a valid channel.");
                    break;
                case InvalidMessageConvertionType.ChannelDoesNotExist:
                    if (error.IsArgument)
                        msgBuilder.WithContent($"Couldn't find the channel for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"Couldn't find the channel for option `{error.Name}`.");
                    break;
                case InvalidMessageConvertionType.BoolShouldNotHaveValue:
                    if (error.IsArgument)
                        msgBuilder.WithContent($"Argument{argumentName} shouldn't have a value.");
                    else
                        msgBuilder.WithContent($"Option `{error.Value}` shouldn't have a value.");
                    break;
                case InvalidMessageConvertionType.NoValueProvided:
                    if (error.IsArgument)
                        msgBuilder.WithContent($"You need to provide a value for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"You need to provide a value for option `{error.Name}`");
                    break;
                default:
                    msgBuilder.WithContent("One of the options/arguments were invalid");
                    break;
            }
            await message.Channel.SendMessageAsync(msgBuilder);
        }
    }
}