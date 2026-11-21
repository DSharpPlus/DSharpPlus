using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity.InteractiveMessages.Components;

/// <summary>
/// Represents a select menu component that will allow selecting a page for pagination.
/// </summary>
/// <remarks>
/// This type can only be used within a <see cref="InteractiveMessageBuilder"/>. 
/// </remarks>
public sealed class PaginationPageSelectComponent : DiscordComponent;
