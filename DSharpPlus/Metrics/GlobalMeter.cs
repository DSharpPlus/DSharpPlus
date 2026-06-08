using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net;

namespace DSharpPlus.Metrics;

internal static class GlobalMeter
{
    private static readonly Counter<long> gatewayEventsSent;
    private static readonly Counter<long> gatewayEventsReceived;
    private static readonly Counter<long> gatewayBytesSent;
    private static readonly Counter<long> gatewayBytesReceived;
    private static readonly Counter<long> gatewayBytesReceivedDecompressed;
    private static readonly Counter<long> reconnects;
    private static readonly Counter<long> restRequests;
    private static readonly Counter<long> successfulRestRequests;
    private static readonly Counter<long> failedRestRequests;
    private static readonly Counter<long> ratelimitedScope;

    private static readonly Meter meter;

    static GlobalMeter()
    {
        meter = new(new MeterOptions("DSharpPlus")
        {
            Version = Utilities.Version
        });

        gatewayEventsSent = meter.CreateCounter<long>("dsharpplus.gateway_events_sent");
        gatewayEventsReceived = meter.CreateCounter<long>("dsharpplus.gateway_events_received");
        gatewayBytesSent = meter.CreateCounter<long>("dsharpplus.gateway_bytes_sent");
        gatewayBytesReceived = meter.CreateCounter<long>("dsharpplus.gateway_bytes_received");
        gatewayBytesReceivedDecompressed = meter.CreateCounter<long>("dsharpplus.gateway_bytes_received_decompressed");
        reconnects = meter.CreateCounter<long>("dsharpplus.gateway_reconnects");
        restRequests = meter.CreateCounter<long>("dsharpplus.rest_requests");
        successfulRestRequests = meter.CreateCounter<long>("dsharpplus.successful_rest_requests");
        failedRestRequests = meter.CreateCounter<long>("dsharpplus.failed_rest_requests");
        ratelimitedScope = meter.CreateCounter<long>("dsharpplus.hit_ratelimit_scopes");
    }

    public static void RecordGatewayEventSent(int size)
    {
        gatewayBytesSent.Add(size);
        gatewayEventsSent.Add(1);
    }

    public static void RecordGatewayEventReceived(int size)
    {
        gatewayBytesReceived.Add(size);
        gatewayEventsReceived.Add(1);
    }

    public static void RecordGatewayEventDecompressed(int size)
        => gatewayBytesReceivedDecompressed.Add(size);

    public static void RecordReconnect()
        => reconnects.Add(1);

    public static void RecordSuccessfulRestRequest()
    {
        restRequests.Add(1);
        successfulRestRequests.Add(1);
    }

    public static void RecordFailedRestRequest(HttpStatusCode statusCode, string? scope = null)
    {
        restRequests.Add(1);
        failedRestRequests.Add(1, new KeyValuePair<string, object?>("status_code", (int)statusCode));

        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            ArgumentNullException.ThrowIfNull(scope);
            ratelimitedScope.Add(1, new KeyValuePair<string, object?>("ratelimit_scope", scope));
        }
    }
}
