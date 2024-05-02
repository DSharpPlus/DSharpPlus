
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;
/// <summary>
/// Represents an invite to a stage channel.
/// </summary>
public class DiscordStageInvite
{
    /// <summary>
    /// Gets the members that are currently speaking in the stage channel.
    /// </summary>
    [JsonProperty("members")]
    public IReadOnlyCollection<DiscordMember> Members { get; internal set; }

    /// <summary>
    /// Gets the number of participants in the stage channel.
    /// </summary>
    [JsonProperty("participant_count")]
    public int ParticipantCount { get; internal set; }

    /// <summary>
    /// Gets the number of speakers in the stage channel.
    /// </summary>
    [JsonProperty("speaker_count")]
    public int SpeakerCount { get; internal set; }

    /// <summary>
    /// Gets the topic of the stage channel.
    /// </summary>
    [JsonProperty("topic")]
    public string Topic { get; internal set; }
}
