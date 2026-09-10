namespace DSharpPlus.Voice.Protocol.RTCP.Payloads;

/// <summary>
/// Represents the type of an item contained within a <see cref="RTCPSourceDescriptionPacket"/>. 
/// </summary>
/// <remarks>
/// Only types CanonicalName (1), DisplayName (2) and ApplicationName (6) are supported by DSharpPlus.Voice, the others 
/// are enumerated and parsed according to the spec, but discarded and not reported to users.
/// </remarks>
internal enum SourceDescriptionItemType
{
    /// <summary>
    /// The canonical, i.e. unique, name of a participant. DSharpPlus.Voice represents itself using the bot's user ID.
    /// </summary>
    CanonicalName = 1,

    /// <summary>
    /// The display name of a participant.
    /// </summary>
    DisplayName = 2,

    /// <summary>
    /// The email address of a participant. Never sent by DSharpPlus.Voice, and ignored if received.
    /// </summary>
    Email = 3,

    /// <summary>
    /// The phone number of a participant. Never sent by DSharpPlus.Voice, and ignored if received.
    /// </summary>
    PhoneNumber = 4,

    /// <summary>
    /// The geographic location of a participant. Never sent by DSharpPlus.Voice, and ignored if received.
    /// </summary>
    GeographicLocation = 5,

    /// <summary>
    /// The name of the application or tool generating the media stream.
    /// </summary>
    ApplicationName = 6,

    /// <summary>
    /// A transient notice about a participant. Never sent by DSharpPlus.Voice, and ignored if received.
    /// </summary>
    Notice = 7,

    /// <summary>
    /// Private, application specific source description. Never sent by DSharpPlus.Voice, but trace-logged if received.
    /// </summary>
    PrivateExtension = 8
}
