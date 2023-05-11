namespace DSharpPlus.CH.Message;

internal enum MessageParameterDataType
{
    String,
    Int,
    Double,
    User,
    Member,
    Channel,
    Role, // Implement this in the MessageCommandHandler.
    Bool
}
