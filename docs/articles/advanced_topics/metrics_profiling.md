---
uid: articles.advanced_topics.metrics_profiling
title: Metrics and Profiling
---

# Introduction

Understanding the characteristics of your application is important, particularly for large applications. DSharpPlus provides metrics in the below-mentioned areas to help you better understand your bot and what it needs. DSharpPlus exposes its metrics via `System.Diagnostics.Metrics` meters, which allows you to see these metrics in real-time using, for example, [`dotnet-counters`](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters) (a tool installable via `dotnet tool install -g dotnet-counters`) or [OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet). Refer to their respective documentation on how to consume metrics.

## Rest Metrics

Metrics on REST requests can be obtained through `DiscordClient.GetRestRequestMetrics` or through the `RestMetricsContainer` service. They specify the amount of requests made and what responses were received from them.

Rest metrics are contained in the `DSharpPlus` meter.

## Gateway Metrics

Metrics on the gateway can be obtained through `DiscordClient.GetGatewayMetrics` or through the `GatewayMetricsContainer` service. They specify the amount of payloads sent and received, the amount of bytes sent, received and received after decompressing and information about connections.

Gateway metrics are contained in the `DSharpPlus` meter.
