using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message;

/// <summary>
/// This is only used in middlewares
/// </summary>
public class MessageContext
{
    /// <summary>
    /// The Discord message.
    /// </summary>
    public required DiscordMessage Message { get; set; }
    
    public required DiscordClient Client { get; set; }

    /// <summary>
    /// Data related to the command module.
    /// </summary>
    public required MessageData Data { get; set; }
}
