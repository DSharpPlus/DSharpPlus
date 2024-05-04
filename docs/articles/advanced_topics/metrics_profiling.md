---
uid: articles.advanced_topics.metrics_profiling
title: Metrics and Profiling
---

# Introduction

Understanding the characteristics of your application is important, particularly for large applications.
Unfortunately, DSharpPlus does not lend itself very well to conventional profiling, and while we intend
to improve the general usability of the library with respect to performance and profiling, we also intend
to provide our own means to gather insight into what's happening. Currently, we track REST requests and
their outcomes, and we intend to add more insights - feel free to let us know via a 
[feature request](https://github.com/DSharpPlus/DSharpPlus/issues/new?assignees=&labels=enhancement&projects=&template=feature_request.yml)!

## Rest Metrics

Metrics on REST requests can be obtained through `GetRequestMetrics` methods on the respective client.
This is supported on the webhook client, @DSharpPlus.DiscordRestClient and @DSharpPlus.DiscordClient. To
obtain metrics from a @DSharpPlus.DiscordShardedClient , fetch metrics from the first shard.

You may optionally specify to reset the tracking metrics, for example when polling regularly to calculate
statistics:

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

This will not reset the lifetime metrics, and you can both poll regularly and access lifetime metrics, however,
different parts of your application can cause the metrics to reset without the other knowing.
