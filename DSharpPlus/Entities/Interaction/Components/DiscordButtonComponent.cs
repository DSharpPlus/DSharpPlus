using DSharpPlus.EventArgs;
using Newtonsoft.Json;
namespace DSharpPlus.Entities;


/// <summary>
/// Represents a button that can be pressed. Fires <see cref="ComponentInteractionCreateEventArgs"/> when pressed.
/// </summary>
public sealed class DiscordButtonComponent : DiscordComponent
{
    /// <summary>
    /// The style of the button.
    /// </summary>
    [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordButtonStyle Style { get; internal set; }

    /// <summary>
    /// The text to apply to the button. If this is not specified <see cref="Emoji"/> becomes required.
    /// </summary>
    [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
    public string Label { get; internal set; }

    /// <summary>
    /// Whether this button can be pressed.
    /// </summary>
    [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool Disabled { get; internal set; }

    /// <summary>
    /// The emoji to add to the button. Can be used in conjunction with a label, or as standalone. Must be added if label is not specified.
    /// </summary>
    [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordComponentEmoji Emoji { get; internal set; }

    /// <summary>
    /// Enables this component if it was disabled before.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordButtonComponent Enable()
    {
        this.Disabled = false;
        return this;
    }

    /// <summary>
    /// Disables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordButtonComponent Disable()
    {
        this.Disabled = true;
        return this;
    }

    /// <summary>
    /// Constructs a new <see cref="DiscordButtonComponent"/>.
    /// </summary>
    internal DiscordButtonComponent() => this.Type = DiscordComponentType.Button;


    /// <summary>
    /// Constucts a new button based on another button.
    /// </summary>
    /// <param name="other">The button to copy.</param>
    public DiscordButtonComponent(DiscordButtonComponent other) : this()
    {
        this.CustomId = other.CustomId;
        this.Style = other.Style;
        this.Label = other.Label;
        this.Disabled = other.Disabled;
        this.Emoji = other.Emoji;
    }

    /// <summary>
    /// Constructs a new button with the specified options.
    /// </summary>
    /// <param name="style">The style/color of the button.</param>
    /// <param name="customId">The Id to assign to the button. This is sent back when a user presses it.</param>
    /// <param name="label">The text to display on the button, up to 80 characters. Can be left blank if <paramref name="emoji"/>is set.</param>
    /// <param name="disabled">Whether this button should be initialized as being disabled. User sees a greyed out button that cannot be interacted with.</param>
    /// <param name="emoji">The emoji to add to the button. This is required if <paramref name="label"/> is empty or null.</param>
    public DiscordButtonComponent(DiscordButtonStyle style, string customId, string label, bool disabled = false, DiscordComponentEmoji emoji = null)
    {
        this.Style = style;
        this.Label = label;
        this.CustomId = customId;
        this.Disabled = disabled;
        this.Emoji = emoji;
        this.Type = DiscordComponentType.Button;
    }
}
