---
uid: articles.advanced_topics.metrics_profiling
title: Metrics and Profiling
---

# Introduction

Understanding the characteristics of your application is important, particularly for large applications. DSharpPlus provides metrics in the below-mentioned areas to help you better understand your bot and what it needs.

## Rest Metrics

Metrics on REST requests can be obtained through `GetRequestMetrics` methods on the respective client. This is supported on the webhook client, @DSharpPlus.DiscordRestClient and @DSharpPlus.DiscordClient.

You may optionally specify to reset the tracking metrics, for example when polling regularly to calculate statistics:

~~~cs
using System;
using System.Threading;

using DSharpPlus;

PeriodicTimer timer = new(TimeSpan.FromHours(1));

while (await timer.WaitForNextTickAsync(ct))
{
    Console.WriteLine(client.GetRequestMetrics(sinceLastCall: true));
}
~~~

This will not reset the lifetime metrics, and you can both poll regularly and access lifetime metrics, however, different parts of your application can cause the metrics to reset without the other knowing.

## Voice Metrics

Voice provides metrics via `VoiceConnection.GetVoiceMetrics` detailing the amount of payloads and data sent, and any issues with receiving, if applicable. Voice also provides its metrics via a `System.Diagnostics.Metrics` meter, which allows you to see these metrics in real-time using, for example, [`dotnet-counters`](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters) (a tool installable via `dotnet tool install -g dotnet-counters`) or [OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet). Refer to their respective documentation on how to consume metrics.
