using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

public sealed class UnknownComponentModalSubmission : IModalSubmission
{
    /// <inheritdoc/>
    public DiscordComponentType ComponentType { get; internal set; }

    /// <inheritdoc/>
    public string CustomId { get; internal set; }

    /// <summary>
    /// The received component. Please note that this component will be partial and may not contain the desired data.
    /// </summary>
    public DiscordComponent Component { get; internal set; }

    internal UnknownComponentModalSubmission(DiscordComponentType componentType, string customId, DiscordComponent component)
    {
        this.ComponentType = componentType;
        this.CustomId = customId;
        this.Component = component;
    }
}
