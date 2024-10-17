using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying a stage instance.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class StageInstanceEditModel : BaseEditModel
{
    /// <summary>
    /// The new stage instance topic.
    /// </summary>
    public Optional<string> Topic { get; set; }

    /// <summary>
    /// The new stage instance privacy level.
    /// </summary>
    public Optional<DiscordStagePrivacyLevel> PrivacyLevel { get; set; }
}
