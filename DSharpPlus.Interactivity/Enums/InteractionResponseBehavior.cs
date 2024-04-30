namespace DSharpPlus.Interactivity.Enums;

public enum InteractionResponseBehavior
{
    /// <summary>
    /// Indicates that invalid input should be ignored when waiting for interactions. This will cause the interaction to fail.
    /// </summary>
    Ignore,
    /// <summary>
    /// Indicates that invalid input should be ACK'd. The interaction will succeed, but nothing will happen.
    /// </summary>
    Ack,
    /// <summary>
    /// Indicates that invalid input should warrant an ephemeral error message.
    /// </summary>
    Respond
}
