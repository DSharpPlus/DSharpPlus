---
uid: articles.voice.introduction
title: Introduction to Voice
---

## Choosing the right extension

DSharpPlus provides an extension for sending and receiving audio, DSharpPlus.Voice. Conversely, [Lavalink4NET](https://github.com/angelobreuer/Lavalink4NET) provides a Lavalink client for DSharpPlus.

[Lavalink](https://github.com/lavalink-devs/Lavalink) can only send audio and runs in a separate process, whereas DSharpPlus.Voice can receive audio and runs in the same process. Lavalink also provides Twitch/YouTube streaming and volume controls. We recommend you consider using Lavalink if you are writing a music bot or something comparable.

## Setting up Voice

If you haven't been convinced to use Lavalink, it's time to set up for DSharpPlus.Voice. The extension requires a number of native libraries to work, which are provided out of the box on the following platforms: `win-x64`, `win-arm64`, `linux-x64`, `linux-musl-x64`, `linux-arm64`, `linux-musl-arm64`, `osx-x64` and `osx-arm64`. If you are not running on one of those platforms, you will have to provide the libraries manually: refer to the [BUILDING.md](https://github.com/DSharpPlus/DSharpPlus/blob/master/BUILDING.md) file for more information.

>[!NOTE]
> Our native builds for x86-64 target `x86-64-v2`, which is any CPU that supports SSE4.2, POPCNT and CMPXCHG16B at the very least, or any Intel/AMD CPU built after 2008. If you are using an older CPU that does not support these features, you will also need to self-build the natives according to the above instructions. Additionally, you must remove the native libraries provided by DSharpPlus, since they will be incompatible with your CPU. With .NET 11, .NET will require the same baseline.

>[!NOTE]
> We do not test whether DSharpPlus.Voice works on 32-bit platforms and we do not take bug reports if something is incorrect on 32-bit, nor on any other platform save the supported eight.

Now equipped with the necessary native libraries, we can set up DSharpPlus.Voice in code:

# [DiscordClientBuilder](#tab/discordclientbuilder)

```cs
DiscordClientBuilder builder = ...;

builder.UseVoice();

// optional further configuration, do not use these values as an example, they exist to illustrate the syntax:
builder.ConfigureVoice(config => 
{
    // this prevents the extension from inferring you meant to fall silent when you did. this is generally not a useful setting to set.
    config.SilenceTicksBeforePausingConnection = int.MaxValue;

    // attempt to reconnect fifteen times instead of ten
    config.MaxReconnects = 15;
})

// or, if you're just setting a single option:
builder.ConfigureVoice(config => config.MaxReconnects = 15);
```

# [Service Collection](#tab/service-collection)

```cs
IServiceCollection services = ...;

services.AddDiscordClient(...);
services.AddVoiceExtension();

// optional further configuration, do not use these values as an example, they exist to illustrate the syntax:
services.Configure<VoiceOptions>(config => 
{
    // this prevents the extension from inferring you meant to fall silent when you did. this is generally not a useful setting to set.
    config.SilenceTicksBeforePausingConnection = int.MaxValue;

    // attempt to reconnect fifteen times instead of ten
    config.MaxReconnects = 15;
})

// or, if you're just setting a single option:
services.Configure<VoiceOptions>(config => config.MaxReconnects = 15);
```
---

With that done, we can connect to a channel using `ConnectAsync`. There are two overloads: one that takes an audio type and a receiver type, and one that takes a service scope, user ID and the aforementioned two other parameters. When connecting to a channel acquired from `DiscordClient`, use the first overload; when connecting to a channel acquired by other means, you must use the second overload. Realistically, this means you should rarely to never need the second overload. An `InvalidOperationException` will be thrown if you're using the first overload on a channel it cannot connect to.

`ConnectAsync` will return control and a `VoiceConnection` instance to you as soon as the connection is initialized and ready for sending and receiving audio.

## Sending Audio

To send audio, we have to acquire an audio writer from `VoiceConnection.CreateAudioWriter(AudioFormat format)`. The supported formats are provided as static fields on `AudioFormat`, for example `AudioFormat.S16LE48KHzStereoPCM` for 16-bit, 48khz, two-channel PCM audio. If you want to change the format you're sending in, you can create a new audio writer at any time. Only one audio writer may be active at any given time.

AudioWriters are PipeWriters, which means you can use the PipeWriter API to submit audio or call `AsStream` to get a `System.IO.Stream`. 

When you temporarily want to stop sending audio, use `SignalSilence` on your AudioWriter to communicate this. If you simply stop submitting audio, DSharpPlus will eventually infer that you probably forgot to signal silence, according to `VoiceOptions.SilenceTicksBeforePausingConnection`, to save on bandwidth, but you should ideally always call `SignalSilence` yourself, both for performance and to prevent audio distortion and stuttering at the end.

todo: write more docs on send

## Receiving Audio

Unlike sending audio, receiving audio requires your intent to be specified upfront. When calling `ConnectAsync`, there is a `Type? receiver` parameter which defaults to `NullAudioReceiver` (thus by default discarding all audio you receive). To receive audio, specify `receiver: typeof(DefaultAudioReceiver)` when connecting.

You can access the configured receiver from `VoiceConnection.Receiver`. The default `NullAudioReceiver` will simply ignore all audio and does not expose anything usable to you, but other receivers may provide features of their own. `DefaultAudioReceiver` provides events for users joining, leaving, starting tos peak and falling silent as well as per-user audio streams with the received audio.

todo: write more docs on receive
