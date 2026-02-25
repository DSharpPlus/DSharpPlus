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
    /// A select menu that allows arbitrary, bot-defined strings to be selected.
    /// </summary>
    StringSelect = 3,

    /// <summary>
    /// A text input field in a modal.
    /// </summary>
    TextInput = 4,

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
    
    /// <summary>
    /// A section of text with optional media (button, thumbnail) accessory.
    /// </summary>
    Section = 9,
    
    /// <summary>
    /// A display of text, up to 4000 characters (unified).
    /// </summary>
    TextDisplay = 10,
    
    /// <summary>
    /// A thumbnail.
    /// </summary>
    Thumbnail = 11,
    
    /// <summary>
    /// A gallery of media.
    /// </summary>
    MediaGallery = 12,
    
    /// <summary>
    /// A singular, arbitrary file.
    /// </summary>
    File = 13,
    
    /// <summary>
    /// A separator between other components.
    /// </summary>
    Separator = 14,
    
    /// <summary>
    /// A container for other components; can be styled with an accent color like embeds.
    /// </summary>
    Container = 17,
    
    /// <summary>
    /// A label component containing a title, component, and optionally description. Only used in Modals.
    /// </summary>
    Label = 18,

    /// <summary>
    /// A component for uploading files to a bot. Only used in modals.
    /// </summary>
    FileUpload = 19,

    /// <summary>
    /// A component containing a single-choice group of up to ten options.
    /// </summary>
    RadioGroup = 21,

    /// <summary>
    /// A component containing a multiple-choice group of checkboxes.
    /// </summary>
    CheckboxGroup = 22,

    /// <summary>
    /// A single checkbox component.
    /// </summary>
    Checkbox = 23
}
