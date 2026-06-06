using System;

namespace DSharpPlus.Voice.Metrics;

/// <summary>
/// Provides metrics about a single voice connection.
/// </summary>
public sealed record VoiceMetricsCollection
{
    /// <summary>
    /// The snowflake identifier of the channel hosting this connection.
    /// </summary>
    public ulong ChannelId { get; internal init; }

    /// <summary>
    /// The amount of time the current user has been connected for.
    /// </summary>
    public TimeSpan CurrentUserConnectionTime { get; internal init; }

    /// <summary>
    /// The amount of time the voice session has been running for.
    /// </summary>
    public TimeSpan TotalSessionTime { get; internal init; }

    /// <summary>
    /// The total amount of bytes sent across the present audio connection.
    /// </summary>
    public ulong AudioBytesSent { get; internal init; }

    /// <summary>
    /// The total amount of bytes received from the present audio connection.
    /// </summary>
    public ulong AudioBytesReceived { get; internal init; }

    /// <summary>
    /// The amount of audio frames sent.
    /// </summary>
    public int AudioFramesSent { get; internal init; }

    /// <summary>
    /// The amount of audio frames received.
    /// </summary>
    public int AudioFramesReceived { get; internal init; }

    /// <summary>
    /// The amount of received audio frames that could not be decrypted.
    /// </summary>
    public int AudioFramesFailedDecryption { get; internal init; }

    /// <summary>
    /// The amount of received empty audio frames.
    /// </summary>
    public int EmptyAudioFramesReceived { get; internal init; }

    /// <summary>
    /// The amount of control packets sent.
    /// </summary>
    public int ControlPacketsSent { get; internal init; }

    /// <summary>
    /// The amount of control packets received.
    /// </summary>
    public int ControlPacketsReceived { get; internal init; }

    /// <summary>
    /// The amount of keepalives sent.
    /// </summary>
    public int KeepalivesSent { get; internal init; }

    /// <summary>
    /// The amount of keepalives received.
    /// </summary>
    public int KeepalivesReceived { get; internal init; }

    /// <summary>
    /// The amount of payloads sent over the voice gateway.
    /// </summary>
    public int GatewayPayloadsSent { get; internal init; }

    /// <summary>
    /// The amount of payloads received from the voice gateway.
    /// </summary>
    public int GatewayPayloadsReceived { get; internal init; }
}
