using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity.InteractiveMessages.Components;

/// <summary>
/// Represents a text display that will be used to paginate the content passed to a message.
/// </summary>
/// <remarks>
/// This type can only be used within a <see cref="InteractiveMessageBuilder"/>. 
/// </remarks>
public sealed class PaginatedTextDisplayComponent : DiscordComponent;
