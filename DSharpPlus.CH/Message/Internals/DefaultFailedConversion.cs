using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message.Internals
{
    /// <summary>
    /// The default class for error handling. 
    /// </summary>
    internal class DefaultFailedConversion : IFailedConvertion
    {
        public async Task HandleErrorAsync(InvalidMessageConvertionError error, DiscordMessage message)
        {
            var msgBuilder = new DiscordMessageBuilder();
            msgBuilder.WithReply(message.Id);
            string argumentName = error.Name.Count() == 0 ? string.Empty : $" `{error.Name}`";
            string safeValue = error.Value.Replace("`", string.Empty);

            // This can be changed to a if else for .IsPositionalArgument
            switch (error.Type)
            {
                case InvalidMessageConvertionType.NotAValidNumber:
                    if (error.IsPositionalArgument)
                        msgBuilder.WithContent($"You cannot convert value `{safeValue}` to a number for argument{argumentName}");
                    else
                        msgBuilder.WithContent($"You cannot convert value `{safeValue}` to a number for option `{error.Name}`");
                    break;
                case InvalidMessageConvertionType.NotAValidInteger:
                    if (error.IsPositionalArgument)
                        msgBuilder.WithContent($"You cannot convert value `{safeValue}` to a integer for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"You cannot convert value `{safeValue}` to a integer for option `{error.Name}`.");
                    break;
                case InvalidMessageConvertionType.NotAValidUser:
                    msgBuilder.WithContent($"`{safeValue}` is not a valid user.");
                    break;
                case InvalidMessageConvertionType.UserDoesNotExist:
                    if (error.IsPositionalArgument)
                        msgBuilder.WithContent($"Couldn't find the user for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"Couldn't find the user for option `{error.Name}`.");
                    break;
                case InvalidMessageConvertionType.NotAValidMember:
                    msgBuilder.WithContent($"`{safeValue}` is not a valid member.");
                    break;
                case InvalidMessageConvertionType.MemberDoesNotExist:
                    if (error.IsPositionalArgument)
                        msgBuilder.WithContent($"Couldn't find the member for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"Couldn't find the member for option `{error.Name}`.");
                    break;
                case InvalidMessageConvertionType.NotAValidChannel:
                    msgBuilder.WithContent($"`{safeValue}` is not a valid channel.");
                    break;
                case InvalidMessageConvertionType.ChannelDoesNotExist:
                    if (error.IsPositionalArgument)
                        msgBuilder.WithContent($"Couldn't find the channel for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"Couldn't find the channel for option `{error.Name}`.");
                    break;
                case InvalidMessageConvertionType.BoolShouldNotHaveValue:
                    if (error.IsPositionalArgument)
                        msgBuilder.WithContent($"Argument{argumentName} shouldn't have a value.");
                    else
                        msgBuilder.WithContent($"Option `{safeValue}` shouldn't have a value.");
                    break;
                case InvalidMessageConvertionType.NoValueProvided:
                    if (error.IsPositionalArgument)
                        msgBuilder.WithContent($"You need to provide a value for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"You need to provide a value for option `{error.Name}`");
                    break;
                case InvalidMessageConvertionType.NotAValidRole:
                    msgBuilder.WithContent($"`{safeValue}` is not a valid role.");
                    break;
                case InvalidMessageConvertionType.RoleDoesNotExist:
                    if (error.IsPositionalArgument)
                        msgBuilder.WithContent($"Couldn't find the channel for argument{argumentName}.");
                    else
                        msgBuilder.WithContent($"Couldn't find the channel for option `{error.Name}`.");
                    break;
                default:
                    if (error.IsPositionalArgument)
                        msgBuilder.WithContent($"Argument{argumentName} is invalid.");
                    else
                        msgBuilder.WithContent($"Option `{error.Name}` is invalid.");
                    break;
            }
            await message.Channel.SendMessageAsync(msgBuilder);
        }
    }
}