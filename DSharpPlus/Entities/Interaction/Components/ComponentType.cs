namespace DSharpPlus.Entities;

/// <summary>
/// Represents a type of component.
/// </summary>
public enum DiscordComponentType
{
    /// <summary>
    /// A row of components.
    /// </summary>
    ActionRow = 1,
    /// <summary>
    /// A button.
    /// </summary>
    Button = 2,
    /// <summary>
    /// A select menu consisting of options.
    /// </summary>
    StringSelect = 3,
    /// <summary>
    /// An input field.
    /// </summary>
    FormInput = 4,
    /// <summary>
    /// A select menu that allows users to be selected.
    /// </summary>
    UserSelect = 5,
    /// <summary>
    /// A select menu that allows roles to be selected.
    /// </summary>
    RoleSelect = 6,
    /// <summary>
    /// A select menu that allows either roles or users to be selected.
    /// </summary>
    MentionableSelect = 7,
    /// <summary>
    /// A select menu that allows channels to be selected.
    /// </summary>
    ChannelSelect = 8,
}
