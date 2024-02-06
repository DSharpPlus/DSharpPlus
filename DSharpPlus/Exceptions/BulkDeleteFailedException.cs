namespace DSharpPlus.Exceptions;

using System;

public class BulkDeleteFailedException : Exception
{
    public BulkDeleteFailedException(int messagesDeleted, Exception innerException)
        : base("Failed to delete all messages. See inner exception", innerException: innerException) =>
        this.MessagesDeleted = messagesDeleted;

    /// <summary>
    /// Number of messages that were deleted successfully.
    /// </summary>
    public int MessagesDeleted { get; init; }
}
