using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ComponentInteractionCreated"/>.
/// </summary>
public class ComponentInteractionCreateEventArgs : InteractionCreateEventArgs
{
    /// <summary>
    /// The Id of the component that was interacted with.
    /// </summary>
    public string Id => this.Interaction.Data.CustomId;

    /// <summary>
    /// The user that invoked this interaction.
    /// </summary>
    public DiscordUser User => this.Interaction.User;

    /// <summary>
    /// The guild this interaction was invoked on, if any.
    /// </summary>
    public DiscordGuild Guild => this.Channel.Guild;

    /// <summary>
    /// The channel this interaction was invoked in.
    /// </summary>
    public DiscordChannel Channel => this.Interaction.Channel;

    /// <summary>
    /// The value(s) selected. Only applicable to SelectMenu components.
    /// </summary>
    public string[] Values => this.Interaction.Data.Values;

    /// <summary>
    /// The message this interaction is attached to.
    /// </summary>
    public DiscordMessage Message { get; internal set; }

    /// <summary>
    /// The locale of the user that invoked this interaction.
    /// </summary>
    public string Locale => this.Interaction.Locale;

    /// <summary>
    /// The guild's locale that the user invoked in.
    /// </summary>
    public string GuildLocale => this.Interaction.GuildLocale;

    internal ComponentInteractionCreateEventArgs() { }
}
