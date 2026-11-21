namespace DSharpPlus.Interactivity.Components;

/// <summary>
/// Specifies how DSharpPlus should respond to undesired interactions with components currently being waited on.
/// </summary>
public enum ComponentInteractionResponseBehavior
{
    /// <summary>
    /// Indicates that invalid input should be ignored when waiting for interactions. This will cause the interaction to fail.
    /// </summary>
    Ignore,
    
    /// <summary>
    /// Indicates that invalid input should warrant an ephemeral error message.
    /// </summary>
    Respond
}
