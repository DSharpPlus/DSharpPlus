using System.Diagnostics.Metrics;

namespace DSharpPlus.Voice.Metrics;

internal static class GlobalVoiceMeter
{
    private static readonly Counter<long> audioFramesSent;
    private static readonly Counter<long> audioBytesSent;
    private static readonly Counter<long> audioFramesReceived;
    private static readonly Counter<long> audioBytesReceived;
    private static readonly Counter<long> audioFramesFailedDecryption;
    private static readonly Counter<long> emptyAudioFramesReceived;
    private static readonly Counter<long> keepalivesSent;
    private static readonly Counter<long> keepalivesReceived;
    private static readonly Counter<long> controlPacketsSent;
    private static readonly Counter<long> controlPacketsReceived;
    private static readonly Counter<long> voiceGatewayPayloadsSent;
    private static readonly Counter<long> voiceGatewayPayloadsReceived;
    private static readonly UpDownCounter<int> concurrentConnections;

    private static readonly Meter voiceMeter;

    static GlobalVoiceMeter()
    {
        voiceMeter = new(new MeterOptions("DSharpPlus.Voice")
        {
            Version = Utilities.Version
        });

        audioFramesSent = voiceMeter.CreateCounter<long>("dsharpplus.voice.audio_frames_sent");

        audioBytesSent = voiceMeter.CreateCounter<long>
        (
            "dsharpplus.voice.audio_bytes_sent",
            description: "Bytes sent over the audio connection, including protocol overhead, keepalives and control packets."
        );
        
        audioFramesReceived = voiceMeter.CreateCounter<long>("dsharpplus.voice.audio_frames_received");

        audioBytesReceived = voiceMeter.CreateCounter<long>
        (
            "dsharpplus.voice.audio_bytes_received",
            description: "Bytes received from the audio connection, including protocol overhead, keepalives and control packets."
        );

        audioFramesFailedDecryption = voiceMeter.CreateCounter<long>("dsharpplus.voice.audio_frames_failed_decryption");
        emptyAudioFramesReceived = voiceMeter.CreateCounter<long>("dsharpplus.voice.empty_audio_frames_received");
        keepalivesSent = voiceMeter.CreateCounter<long>("dsharpplus.voice.keepalives_sent");
        keepalivesReceived = voiceMeter.CreateCounter<long>("dsharpplus.voice.keepalives_received");
        controlPacketsSent = voiceMeter.CreateCounter<long>("dsharpplus.voice.control_packets_sent");
        controlPacketsReceived = voiceMeter.CreateCounter<long>("dsharpplus.voice.control_packets_received");
        voiceGatewayPayloadsSent = voiceMeter.CreateCounter<long>("dsharpplus.voice.gateway_payloads_sent");
        voiceGatewayPayloadsReceived = voiceMeter.CreateCounter<long>("dsharpplus.voice.gateway_payloads_received");
        concurrentConnections = voiceMeter.CreateUpDownCounter<int>("dsharpplus.voice.concurrent_connections");
    }

    public static void RecordAudioFrameSent(int bytes)
    {
        audioFramesSent.Add(1);
        audioBytesSent.Add(bytes);
    }

    public static void RecordAudioFrameReceived(int bytes)
    {
        audioFramesReceived.Add(1);
        audioBytesReceived.Add(bytes);
    }

    public static void RecordAudioFrameFailedDecryption() 
        => audioFramesFailedDecryption.Add(1);

    public static void RecordEmptyAudioFrameReceived() 
        => emptyAudioFramesReceived.Add(1);

    public static void RecordKeepaliveSent()
    {
        keepalivesSent.Add(1);
        audioBytesSent.Add(8);
    }

    public static void RecordKeepaliveReceived()
    {
        keepalivesReceived.Add(1);
        audioBytesReceived.Add(8);
    }

    public static void RecordControlPacketSent(int bytes, int packets)
    {
        controlPacketsSent.Add(packets);
        audioBytesSent.Add(bytes);
    }

    public static void RecordControlPacketReceived(int bytes, int packets)
    {
        controlPacketsReceived.Add(packets);
        audioBytesReceived.Add(bytes);
    }

    public static void RecordGatewayPayloadSent() 
        => voiceGatewayPayloadsSent.Add(1);

    public static void RecordGatewayPayloadReceived() 
        => voiceGatewayPayloadsReceived.Add(1);

    public static void RecordConnectionCreated() 
        => concurrentConnections.Add(1);

    public static void RecordConnectionClosed() 
        => concurrentConnections.Add(-1);
}
