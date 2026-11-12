using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity.InteractiveMessages.Components;

/// <summary>
/// Represents a row of pagination buttons, as specified in <see cref="InteractivityOptions.PaginationButtons"/>. 
/// </summary>
/// <remarks>
/// This type can only be used within a <see cref="InteractiveMessageBuilder"/>. 
/// </remarks>
public sealed class PaginationButtonRowComponent : DiscordComponent;
