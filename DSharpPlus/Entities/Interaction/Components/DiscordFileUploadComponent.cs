using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// A component wherethrough users can upload files. Only used in modals. The maximum cumulative size of the files depends on the server's boost level and nitro.
/// </summary>
public sealed class DiscordFileUploadComponent : DiscordComponent
{
    /// <summary>
    /// The minimum amount of files to upload, between 0 and 10.
    /// </summary>
    [JsonProperty("min_values", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int? MinValues { get; internal set; }

    /// <summary>
    /// The maximum of files to upload, between 1 and 10.
    /// </summary>
    [JsonProperty("max_values", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int? MaxValues { get; internal set; }

    /// <summary>
    /// Indicates whether a submission is required. This is mutually exclusive with setting <see cref="MinValues"/> to 0.
    /// </summary>
    [JsonProperty("required")]
    public bool IsRequired { get; internal set; }

    /// <summary>
    /// The file types permissible to upload using this component. This takes any dot-prefixed extensions, or one of the following values: <br/>
    /// - <c>"image"</c>: png, gif, jpg, jpeg, jfif, webp, avif, <br/>
    /// - <c>"video"</c>: mp4, mov, qt, webm, <br/>
    /// - <c>"audio"</c>: mp3, m4a, wav, ogg, opus, flac <br/>
    /// <br/>
    /// Note that the above lists are not set in stone and should not be hardcoded.
    /// </summary>
    [JsonProperty("file_types", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public IReadOnlyList<string>? FileTypes { get; internal set; }

    /// <summary>
    /// Internally used for handling responses.
    /// </summary>
    [JsonProperty("values")]
    internal ulong[] Values { get; set; }

    internal DiscordFileUploadComponent() => this.Type = DiscordComponentType.FileUpload;

    public DiscordFileUploadComponent(string customId, int? minValues = null, int? maxValues = null, bool isRequired = true, IReadOnlyList<string>? fileTypes = null)
        : this()
    {
        this.CustomId = customId;
        this.MinValues = minValues;
        this.MaxValues = maxValues;
        this.IsRequired = isRequired;
        this.FileTypes = fileTypes;
    }
}
