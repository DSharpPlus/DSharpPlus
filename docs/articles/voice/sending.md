---
title: Sending Audio
uid: articles.voice.sending
---

To send audio to a voice connection, we must first acquire an `AudioWriter` from `VoiceConnection.CreateAudioWriter`. There, we specify the format our audio data has, which by default supports the following:

| Property | Description |
| :--- | :--- |
| `AudioFormat.S16LE48KHzStereoPCM` | Signed 16-bit little-endian 48khz stereo (two-channel) PCM audio. This should be your default choice if you don't see any particular reason to choose anything else. |
| `AudioFormat.S16LE48KHzMonoPCM` | Signed 16-bit little-endian 48khz mono (single-channel) PCM audio. |
| `AudioFormat.S16LE44KHzStereoPCM` | Signed 16-bit little-endian 44.1khz stereo (two-channel) PCM audio. |
| `AudioFormat.S16LE44KHzMonoPCM` | Signed 16-bit little-endian 44.1khz mono (single-channel) PCM audio. |
| `AudioFormat.S24LE48KHzStereoPCM` | Signed 24-bit little-endian 48khz stereo (two-channel) PCM audio. |
| `AudioFormat.Float32LE48KHzStereoPCM` | Signed single-precision float little-endian 48khz stereo (two-channel) PCM audio. |

It is possible, albeit somewhat complex, to add custom audio formats, see [the documentation for doing so](#adding-custom-audio-formats).

Once we have an `AudioWriter`, we can begin sending. `AudioWriter` is a kind of `PipeWriter`, which means data can be submitted to it by either requesting a `Span<T>` or `Memory<T>` using `GetSpan(int sizeHint)`/`GetMemory(int sizeHint)` respectively, writing to that buffer and then advancing the writer using `Advance(int bytesWritten)`, or alternatively by getting a `Stream` using the `AsStream()` method and writing to that stream. In both cases, the audio writer will take the data and automatically enqueue it for sending.

When all is said and done, and there is nothing left to be sent, you must call `SignalSilence()`, or `SignalCompletion()` if you do not intend to send any more audio through this connection at all. 

DSharpPlus contains a feature to try to infer when you have no more audio to send: if you fail to submit audio for x consecutive 20ms 'ticks' in a row, where x is the value of `VoiceOptions.SilenceTicksBeforePausingConnection` (default: 5), DSharpPlus will initiate the same routine. This behaviour exists to save bandwidth, not to replace missed `SignalSilence()` calls, and may lead to distorted or stuttering audio if relied on as a replacement for `SignalSilence()`. If your bot is encountering stuttering and isn't running on a good machine, consider raising this configuration value. Vice versa, if your bot is running on a perfectly real-time capable machine, you aren't missing any `SignalSilence()` calls and are encountering stuttering when stopping or starting to send audio, consider lowering this value to 1.

After calling `SignalSilence()`, you can reawaken the audio writer by simply submitting more audio, but `SignalCompletion()` is final.

If you need to change audio format in the middle of a connection, you can simply request a new `AudioWriter` from the connection. Continuing to use the old writer is invalid after that: you can only have one `AudioWriter` object at any given time, and you cannot reuse old writers.

## Adding custom audio formats

It is possible, if a little bit involved, to implement custom audio formats. First, you need to create an `AudioWriter` implementation for your format that generates opus packets from your audio input and enqueues them into `AudioWriter.PacketWriter`. The buffer containing the packet must be provisioned by an `AudioBufferManager`; `AudioBufferManager.Shared` is provided as a sane default, but it is possible to bring your own pooling if necessary.

Second, we need to choose a string identifier for this audio format. This should be as specific as possible to prevent conflicts: `"pcm"` is a bad identifier, `"s24le-96khz-stereo-pcm"` is a good identifier. This string is what will be passed to the `AudioFormat` we use to create your custom `AudioWriter`, and it is recommended to add it to `AudioFormat` like so:

~~~cs
extension(AudioFormat)
{
    public static AudioFormat MyCustomAudio => new("my-custom-audio");
}
~~~

Lastly, we need to create the machinery for creating your audio writer. Create a decorator for `IAudioWriterFactory` like so:

~~~cs
public class MyCustomAudioWriterFactory(IAudioWriterFactory underlying) : IAudioWriterFactory
{
    /// <inheritdoc/>
    public AudioWriter CreateAudioWriter(AudioFormat format, AudioChannelWriter writer)
    {
        if (format == AudioFormat.MyCustomAudio)
        {
            return new MyCustomAudioWriter(writer);
        }
        else
        {
            return underlying.CreateAudioWriter(format, writer);
        }
    }
}

// registration:

services.Decorate<IAudioWriterFactory, MyCustomAudioWriterFactory>();

// on DiscordClientBuilder, wrap this in a builder.ConfigureServices call to get direct access to the service collection
~~~

Adding formats is perfectly composable, and many different components can add custom formats.

>[!NOTE]
> Decorating `IAudioWriterFactory` MUST occur after the initial registration of DSharpPlus.Voice using `DiscordClientBuilder.UseVoice` or `IServiceCollection.AddVoiceExtension`.
