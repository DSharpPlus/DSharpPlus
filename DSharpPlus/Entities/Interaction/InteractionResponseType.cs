namespace DSharpPlus.Entities;

/// <summary>
/// Represents the type of interaction response
/// </summary>
public enum InteractionResponseType
{
    /// <summary>
    /// Acknowledges a Ping.
    /// </summary>
    Pong = 1,

    /// <summary>
    /// Responds to the interaction with a message.
    /// </summary>
    ChannelMessageWithSource = 4,

    /// <summary>
    /// Acknowledges an interaction to edit to a response later. The user sees a "thinking" state.
    /// </summary>
    DeferredChannelMessageWithSource = 5,

    /// <summary>
    /// Acknowledges a component interaction to allow a response later.
    /// </summary>
    DeferredMessageUpdate = 6,

    /// <summary>
    /// Responds to a component interaction by editing the message it's attached to.
    /// </summary>
    UpdateMessage = 7,

    /// <summary>
    /// Responds to an auto-complete request.
    /// </summary>
    AutoCompleteResult = 8,

    /// <summary>
    /// Respond to an interaction with a modal popup.
    /// </summary>
    Modal = 9,
}
