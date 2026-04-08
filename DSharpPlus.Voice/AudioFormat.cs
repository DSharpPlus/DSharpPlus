using System.Diagnostics.CodeAnalysis;

namespace DSharpPlus.Voice;

/// <summary>
/// Represents what format audio will passed in to a <see cref="AudioWriter"/>. 
/// </summary>
public readonly record struct AudioFormat
{
    /// <summary>
    /// The format identifier string.
    /// </summary>
    public required string Identifier { get; init; }

    /// <summary>
    /// Creates a new instance of this struct.
    /// </summary>
    [SetsRequiredMembers]
    public AudioFormat(string identifier) 
        => this.Identifier = identifier;

    /// <summary>
    /// Represents s16 little-endian 48khz stereo (two-channel) PCM audio.
    /// </summary>
    public static AudioFormat S16LE48KHzStereoPCM => new("s16le-48khz-stereo-pcm");

    /// <summary>
    /// Represents s16 little-endian 48khz mono (single-channel) PCM audio.
    /// </summary>
    public static AudioFormat S16LE48KHzMonoPCM => new("s16le-48khz-mono-pcm");

    /// <summary>
    /// Represents s16 little-endian 44.1khz stereo (two-channel) PCM audio.
    /// </summary>
    public static AudioFormat S16LE44KHzStereoPCM => new("s16le-44khz-stereo-pcm");

    /// <summary>
    /// Represents s16 little-endian 44.1khz mono (single-channel) PCM audio.
    /// </summary>
    public static AudioFormat S16LE44KHzMonoPCM => new("s16le-44khz-mono-pcm");

    /// <summary>
    /// Represents s24 little-endian 48khz stereo (two-channel) PCM audio.
    /// </summary>
    public static AudioFormat S24LE48KHzStereoPCM => new("s24le-48khz-stereo-pcm");

    /// <summary>
    /// Represents float32 little-endian 48khz stereo (two-channel) PCM audio.
    /// </summary>
    public static AudioFormat Float32LE48KHzStereoPCM => new("float32le-48khz-stereo-pcm");
}