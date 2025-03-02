namespace DSharpPlus.Entities;

/// <summary>
/// Represents a division between components. Can optionally be rendered as a dividing line.
/// </summary>
public class DiscordSeparatorComponent : DiscordComponent
{
    public new DiscordComponentType ComponentType => DiscordComponentType.Separator;

    /// <summary>
    /// Whether the separator renders as a dividing line.
    /// </summary>
    public bool Divider { get; internal set; }
    
    /// <summary>
    /// The spacing for the separator. Defaults to <see cref="DiscordSeparatorComponent"/>
    /// </summary>
    public DiscordSeparatorSpacing Spacing { get; internal set; }

}
