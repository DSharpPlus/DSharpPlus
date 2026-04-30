using System;
using System.Threading;

namespace DSharpPlus.Voice.Metrics;

/// <summary>
/// Tracks per-connection voice metrics.
/// </summary>
public sealed class VoiceMetrics
{
    private ulong channelId;
    private DateTimeOffset connectionStartTime = default;
    private DateTimeOffset sessionStartTime = default;
    private LiveVoiceMetrics metrics = default;

    public VoiceMetrics() 
        => GlobalVoiceMeter.RecordConnectionCreated();

    internal void SetChannelId(ulong channelId)
        => this.channelId = channelId;

    internal void SetConnectionStartTime(DateTimeOffset startTime)
        => this.connectionStartTime = startTime;

    internal void SetSessionStartTime(DateTimeOffset startTime)
        => this.sessionStartTime = startTime;

    internal void RecordAudioFrameSent(int bytes)
    {
        Interlocked.Increment(ref this.metrics.audioFramesSent);
        Interlocked.Add(ref this.metrics.audioBytesSent, (ulong)bytes);

        GlobalVoiceMeter.RecordAudioFrameSent(bytes);
    }

    internal void RecordAudioFrameReceived(int bytes)
    {
        Interlocked.Increment(ref this.metrics.audioFramesReceived);
        Interlocked.Add(ref this.metrics.audioBytesReceived, (ulong)bytes);

        GlobalVoiceMeter.RecordAudioFrameReceived(bytes);
    }

    internal void RecordAudioFrameFailedDecryption()
    {
        Interlocked.Increment(ref this.metrics.audioFramesFailedDecryption);

        GlobalVoiceMeter.RecordAudioFrameFailedDecryption();
    }

    internal void RecordEmptyAudioFrameReceived()
    {
        Interlocked.Increment(ref this.metrics.emptyAudioFramesReceived);

        GlobalVoiceMeter.RecordEmptyAudioFrameReceived();
    }

    internal void RecordKeepaliveSent()
    {
        Interlocked.Increment(ref this.metrics.keepalivesSent);
        Interlocked.Add(ref this.metrics.audioBytesSent, 8);

        GlobalVoiceMeter.RecordKeepaliveSent();
    }

    internal void RecordKeepaliveReceived()
    {
        Interlocked.Increment(ref this.metrics.keepalivesReceived);
        Interlocked.Add(ref this.metrics.audioBytesReceived, 8);

        GlobalVoiceMeter.RecordKeepaliveReceived();
    }

    internal void RecordControlPacketSent(int bytes, int packets)
    {
        Interlocked.Add(ref this.metrics.controlPacketsSent, packets);
        Interlocked.Add(ref this.metrics.audioBytesSent, (ulong)bytes);

        GlobalVoiceMeter.RecordControlPacketSent(bytes, packets);
    }

    internal void RecordControlPacketReceived(int bytes, int packets)
    {
        Interlocked.Add(ref this.metrics.controlPacketsReceived, packets);
        Interlocked.Add(ref this.metrics.audioBytesReceived, (ulong)bytes);

        GlobalVoiceMeter.RecordControlPacketReceived(bytes, packets);
    }

    internal void RecordGatewayPayloadSent()
    {
        Interlocked.Increment(ref this.metrics.voiceGatewayPayloadsSent);

        GlobalVoiceMeter.RecordGatewayPayloadSent();
    }

    internal void RecordGatewayPayloadReceived()
    {
        Interlocked.Increment(ref this.metrics.voiceGatewayPayloadsReceived);

        GlobalVoiceMeter.RecordGatewayPayloadReceived();
    }

    internal void CloseConnection()
        => GlobalVoiceMeter.RecordConnectionClosed();

    public VoiceMetricsCollection GetVoiceMetrics()
    {
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;

        return new VoiceMetricsCollection()
        {
            ChannelId = this.channelId,
            CurrentUserConnectionTime = currentTime - this.connectionStartTime,
            TotalSessionTime = currentTime - this.sessionStartTime,
            AudioFramesSent = this.metrics.audioFramesSent,
            AudioBytesSent = this.metrics.audioBytesSent,
            AudioFramesReceived = this.metrics.audioFramesReceived,
            AudioBytesReceived = this.metrics.audioBytesReceived,
            AudioFramesFailedDecryption = this.metrics.audioFramesFailedDecryption,
            EmptyAudioFramesReceived = this.metrics.emptyAudioFramesReceived,
            KeepalivesSent = this.metrics.keepalivesSent,
            KeepalivesReceived = this.metrics.keepalivesReceived,
            ControlPacketsSent = this.metrics.controlPacketsSent,
            ControlPacketsReceived = this.metrics.controlPacketsReceived,
            GatewayPayloadsSent = this.metrics.voiceGatewayPayloadsSent,
            GatewayPayloadsReceived = this.metrics.voiceGatewayPayloadsReceived
        };
    }

    private struct LiveVoiceMetrics
    {
        public int audioFramesSent;
        public ulong audioBytesSent;
        public int audioFramesReceived;
        public ulong audioBytesReceived;
        public int audioFramesFailedDecryption;
        public int emptyAudioFramesReceived;
        public int keepalivesSent;
        public int keepalivesReceived;
        public int controlPacketsSent;
        public int controlPacketsReceived;
        public int voiceGatewayPayloadsSent;
        public int voiceGatewayPayloadsReceived;
    }
}
