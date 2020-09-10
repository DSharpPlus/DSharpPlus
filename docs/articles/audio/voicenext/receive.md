# Wiretapping for dummies - receiving and saving voice data

So you want your bot to listen to what others say, and then maybe do something with that.

DSharpPlus supports incoming voice, and, much like with outgoing voice, the data you get is raw PCM data.

## 1. Setting it up

You can get rid of the `play` command from the previous example. Other than that, you need to enable incoming voice in your 
voice client configuration. Replace the line where you enable voice with the following:

```cs
voice = discord.UseVoiceNext(new VoiceNextConfiguration
{
	EnableIncoming = true
});
```

## 2. Hooking it up

The first thing to do when you want to listen is hooking the appropriate events. `VoiceNextConnection` offers 2: 
@DSharpPlus.VoiceNext.VoiceNextConnection.UserSpeaking and @DSharpPlus.VoiceNext.VoiceNextConnection.VoiceReceived.

First, in your command module, you will want to create a non-command method for this handler:

```cs
public async Task OnVoiceReceived(VoiceReceiveEventArgs ea)
{

}
```

Visual Studio will complain about missing classes, add `using DSharpPlus.EventArgs;` to your usings section.

Then, in your `join` command, you will want to attach it to an appropriate event, and in the `leave` command, you will 
want to detach it.

Next up, you want to add `using System.Collections.Concurrent` and `using System.Linq;` to your usings. You will also need to 
create a dictionary for `source -> stream` mapping if you want to separate the incoming voice: 
`private ConcurrentDictionary<uint, Process> ffmpegs;`. Make sure you initialize it in your `join` command and null it in 
`leave`. Additionally, before you null it, you will need to deinitialize all the ffmpeg instances there.

Finally, in `OnVoiceReceived`, you need to do your processing logic. It will look more or less like this:

* Check if the source has an ffmpeg source.
* Create one if not. Use existing otherwise.
* Pipe the data to the instance.

Once all this is done, the entire class should look more or less like this:

```cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;

namespace MyFirstBot
{
    public class MyCommands
    {
        private ConcurrentDictionary<uint, Process> ffmpegs;

        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                throw new InvalidOperationException("Already connected in this guild.");

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("You need to be in a voice channel.");

            vnc = await vnext.ConnectAsync(chn);

            this.ffmpegs = new ConcurrentDictionary<uint, Process>();
            vnc.VoiceReceived += OnVoiceReceived;

            await ctx.RespondAsync("👌");
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");
            
            vnc.VoiceReceived -= OnVoiceReceived;
            foreach (var kvp in this.ffmpegs)
            {
                await kvp.Value.StandardInput.BaseStream.FlushAsync();
                kvp.Value.StandardInput.Dispose();
                kvp.Value.WaitForExit();
            }
            this.ffmpegs = null;

            vnc.Disconnect();

            await ctx.RespondAsync("👌");
        }

        public async Task OnVoiceReceived(VoiceReceiveEventArgs ea)
        {
            if (!this.ffmpegs.ContainsKey(ea.SSRC))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $@"-ac 2 -f s16le -ar 48000 -i pipe:0 -ac 2 -ar 44100 {ea.SSRC}.wav",
                    RedirectStandardInput = true
                };

                this.ffmpegs.TryAdd(ea.SSRC, Process.Start(psi));
            }

            var buff = ea.Voice.ToArray();

            var ffmpeg = this.ffmpegs[ea.SSRC];
            await ffmpeg.StandardInput.BaseStream.WriteAsync(buff, 0, buff.Length);
        }
    }
}
```

If you run the bot, connect to a voice channel now, and start speaking, you will notice wav files popping up in the bot 
project directory. If they are, congratulations, your bot is recording, and you are on a list somewhere!