namespace DSharpPlus.UnifiedCommands.Message;

// TODO: Allow user errors. In general.
/// <summary>
/// Enum representing the possibilities for conversion failure.
/// </summary>
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
