namespace DSharpPlus.UnifiedCommands.Message;

public enum InvalidMessageConversionType
{
    NotAValidNumber,
    NotAValidInteger,
    NotAValidUser,
    UserDoesNotExist,
    NotAValidMember,
    MemberDoesNotExist,
    NotAValidChannel,
    ChannelDoesNotExist,
    BoolShouldNotHaveValue,
    NoValueProvided,
    NotAValidRole,
    RoleDoesNotExist,
    IsInDMs
}
