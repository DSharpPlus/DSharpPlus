using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

public class StageInstanceEditModel : BaseEditModel
{
    /// <summary>
    /// The new stage instance topic.
    /// </summary>
    public Optional<string> Topic { internal get; set; }

    /// <summary>
    /// The new stage instance privacy level.
    /// </summary>
    public Optional<DiscordStagePrivacyLevel> PrivacyLevel { internal get; set; }
}
