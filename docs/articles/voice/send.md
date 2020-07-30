# Your first digital broadcast - introduction to VoiceNext

So you just dropped your new mixtape and it's absolute fire. Wouldn't it be nice to let your friends listen?

Audio in Discord is a fairly complicated subject, but thankfully, DSharpPlus makes it easy.

## 1. Installing VoiceNext

Using the procedures in the first bot article, install a NuGet package called `DSharpPlus.VoiceNext`.

Now you need to enable VoiceNext module on your DiscordClient. Add a new field to your bot's `Program` class: 
`static VoiceNextClient voice;`

Visual Studio will complain, you also need to add `using DSharpPlus.VoiceNext;` to your usings in both the command module 
and the bot class.

Before you connect, enable the module on your client: 

```cs
voice = discord.UseVoiceNext();
```

This will enable the module.

If you still have your command module, go to it, remove all the commands from previous examples, and add 
`using DSharpPlus.VoiceNext;` to the usings section.

In your bot class, add `EnableDms = false` to your command config.

Remember to follow the setup guide. Make sure you have the required native components and ffmpeg in appropriate directory.

## 2. Connecting and disconnecting

Before the bot can play audio, it needs to connect to a voice channel. The most intuitive way to achieve this is by connecting 
to the same channel as the user who invokes the command.

Create a new command, call it `join`. In it, you will want to do the following:

* Get the VoiceNext client.
* Check if the bot isn't connected already.
* Fail if so.
* Check if the user is in a voice channel.
* Fail if not.
* Connect to voice.
* Let the user know it all went well.

You also need a way to disconnect from voice. Create a command called `leave`, and make it do the following:

* Get the VoiceNext client.
* Check if the bot is connected.
* Fail if not.
* Disconnect.
* Let the user know it all went well.

I encourage you to try and solve both of these issues yourself, however if you get stuck, here's some reference:

```cs
[Command("join")]
public async Task Join(CommandContext ctx)
{
	var chn = ctx.Member?.VoiceState?.Channel;
	if (chn == null)
		throw new InvalidOperationException("You need to be in a voice channel.");

	vnc = await chn.ConnectAsync();
	await ctx.RespondAsync("👌");
}

[Command("leave")]
public async Task Leave(CommandContext ctx)
{
	var vnext = ctx.Client.GetVoiceNextClient();

	var vnc = vnext.GetConnection(ctx.Guild);
	if (vnc == null)
		throw new InvalidOperationException("Not connected in this guild.");

	vnc.Disconnect();
	await ctx.RespondAsync("👌");
}
```

If you run your bot now, join a voice channel, and call `;;join`, the bot should join the voice channel. Conversely, invoking 
`;;leave` will make the bot disconnect.

![Joining](/images/06_01_voice_join.png "Joining voice")

![Leaving](/images/06_02_voice_leave.png "Leaving voice")

## 3. Broadcasting

Your bot can now connect and disconnect, however it still cannot do the most important thing - broadcast audio.

Let's change that. Create a new command called `play`. Give it a string argument called `file`, and mark it with 
[RemainingText attribute](xref:DSharpPlus.CommandsNext.Attributes.RemainingTextAttribute). It will make that 
argument capture all the text after the command's name.

What you want to do right now, is something along these lines:

* Get the VoiceNext client.
* Check if the bot is connected.
* Fail if not.
* Check if the specified file exists.
* Fail if not.

After that, add `using System.IO;` and `using System.Diagnostics;` to your usings. This is where things get really fun.

You cannot send audio encoded with anything but Opus to Discord, and to encode audio to Opus, you need to get it in raw PCM 
form. This is where ffmpeg comes in. It can transcode to and from various audio and video formats, including PCM.

In order to get audio that we can use with Discord, you will need to spawn an ffmpeg instance, feed it your file, and grab 
the PCM data from its output stream.

Then you will be copying from that stream to Discord, one sample at a time. The whole command should look more or less like 
this: 

```cs
[Command("play")]
public async Task Play(CommandContext ctx, [RemainingText] string file)
{
	var vnext = ctx.Client.GetVoiceNextClient();

	var vnc = vnext.GetConnection(ctx.Guild);
	if (vnc == null)
		throw new InvalidOperationException("Not connected in this guild.");

	if (!File.Exists(file))
		throw new FileNotFoundException("File was not found.");
	
	await ctx.RespondAsync("👌");
	await vnc.SendSpeakingAsync(true); // send a speaking indicator

	var psi = new ProcessStartInfo
	{
		FileName = "ffmpeg",
		Arguments = $@"-i ""{file}"" -ac 2 -f s16le -ar 48000 pipe:1",
		RedirectStandardOutput = true,
		UseShellExecute = false
	};
	var ffmpeg = Process.Start(psi);
	var ffout = ffmpeg.StandardOutput.BaseStream;

	var txStream = vnc.GetTransmitStream();
	await ffout.CopyToAsync(txStream);
	await txStream.FlushAsync();

	await vnc.WaitForPlaybackFinishAsync(); // wait until playback finishes
}
```

If you did everything right, your bot should now be playing music. If it is, congratulations.

Of note here is the fact that writing to the transmit stream will not block until audio is finished playing. A transmit 
will split your PCM input into appropriate chunks, prepare them, then enqueue them for sending to Discord. Packets will 
then be sent at an appropriate rate. VoiceNext offers a method, which allows you to finish for the internal queue to be 
exhausted (which effectively means the playback has finished).

It's also worth noting that if the PCM data doesn't end on packet boundary (i.e. 20ms), the remainder might be left in 
the transmit stream's internal buffer. To force the stream to flush the buffer, call `Flush()` (or `FlushAsync()`).

![Playing](/images/06_03_voice_play.png "Playing audio")

![Console](/images/06_04_voice_console.png "Playing audio - console")