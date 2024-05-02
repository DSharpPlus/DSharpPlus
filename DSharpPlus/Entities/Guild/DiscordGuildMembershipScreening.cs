using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a guild's membership screening form.
/// </summary>
public class DiscordGuildMembershipScreening
{
    /// <summary>
    /// Gets when the fields were last updated.
    /// </summary>
    [JsonProperty("version")]
    public DateTimeOffset Version { get; internal set; }

    /// <summary>
    /// Gets the steps in the screening form.
    /// </summary>
    [JsonProperty("form_fields")]
    public IReadOnlyList<DiscordGuildMembershipScreeningField> Fields { get; internal set; }

    /// <summary>
    /// Gets the server description shown in the screening form.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; internal set; }
}
