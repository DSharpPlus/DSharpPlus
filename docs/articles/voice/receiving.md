---
title: Receiving Audio
uid: articles.voice.receiving
---

Unlike sending, which can change format and mode at any point in the connection if so desired, your desired receive mode must be specified up-front when calling `ConnectAsync`. Both of those methods take a parameter of the shape `Type? receiverType = null`, which, if left unspecified, defaults to `typeof(NullAudioReceiver)`. By default, thus, no audio will be received.

If you desire to receive audio, you must specify a different receiver. DSharpPlus.Voice provides the following receivers:

| Type | Description |
| :--- | :--- |
| `NullAudioReceiver` | Discards all incoming audio. |
| `DefaultAudioReceiver` | Provides incoming audio on a per-user basis to further specified receivers. |


Specify a different receiver like so: `VoiceConnection connection = await channel.ConnectAsync(receiverType: typeof(DefaultAudioReceiver));`

The configured receiver is accessible as `VoiceConnection.Receiver` and needs to be casted to its exact type to access features provided by the receiver. Exactly one receiver instance is created per connection.

## `DefaultAudioReceiver`

This is the default receiver type for actually receiving audio. It provides the events `UserStartedSpeaking`, `UserStoppedSpeaking`, `UserJoined` and `UserLeft`, and the necessary machinery to receive each speaking user's audio from `UserAudioReceiver`s, which expose a `PipeReader` and a bool `IsSpeaking` each. Once again, there are multiple different user audio receiver types:

| Type | Description |
| :--- | :--- |
| `NullUserAudioReceiver` | Discards incoming audio from this user. `IsSpeaking` will always return `false`. |
| `RealtimeUserAudioReceiver` | Provides the incoming audio from this user as signed 16-bit 48khz PCM data. |

User audio receivers default to `RealtimeUserAudioReceiver`, but can be fine-grained using the methods available on `DefaultAudioReceiver`. It is not possible to add custom user audio receivers at this time.

## Creating a custom audio receiver

It is, of course, possible to create a custom audio receiver. It must implement the `AudioReceiver` class and be registered to dependency injection like so: 

~~~cs
public class MyCustomReceiver : AudioReceiver
{
    // Called by the library on connecting or reconnecting to notify the receiver of which users are already present.
    protected internal override Task Initialize(IReadOnlyList<ulong> joinedUsers);

    // Called by the library when a new user joined the call.
    protected internal override Task ProcessUserJoinedAsync(ulong id);

    // Called by the library when a user left the call.
    protected internal override Task ProcessUserLeftAsync(ulong id);

    // Called by the library when a user started speaking and we may now receive audio from them.
    protected internal override Task ProcessUserStartedSpeakingAsync(ulong id);

    // Called by the library when a user stopped speaking and we should no longer expect audio from them.
    protected internal override Task ProcessUserStoppedSpeakingAsync(ulong id);

    // Called by the library when audio is received from a user.
    protected internal override Task ProcessAudioAsync(ulong speakingUserId, uint sequence, AudioTimestamp timestamp, TimeSpan duration, byte[] audio);
}

// registration:

services.AddScoped<MyCustomReceiver>();

// on DiscordClientBuilder, wrap this in a builder.ConfigureServices call to get direct access to the service collection
~~~

>[!NOTE]
> Custom receivers must be registered as scoped, and must not be registered under a particular interface.

`AudioTimestamp` is a special timestamp that refers to how long has passed since the user first spoke to us. The sequence number is a monotonically incrementing number identifying the proper order of audio packets. It is entirely possible for packets to arrive out of order: in such cases, it is up to your receiver to handle this.

After implementing and registering a custom receiver, it can be used like so: 

~~~cs
VoiceConnection connection = await channel.ConnectAsync(receiverType: typeof(MyCustomReceiver));
MyCustomReceiver receiver = (MyCustomReceiver)connection.Receiver;

// you can now access any features your receiver provides
~~~
