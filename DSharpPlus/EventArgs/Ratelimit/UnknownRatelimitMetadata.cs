namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents metadata for opcodes DSharpPlus doesn't know metadata for.
/// </summary>
public sealed class UnknownRatelimitMetadata : RatelimitMetadata
{
    /// <summary>
    /// The JSON payload sent as the metadata object.
    /// </summary>
    public string Json { get; internal set; }
}
