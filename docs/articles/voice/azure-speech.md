# DSharpPlus + Azure Speech Services

## 1. Introduction

With a little bit of effort, it is entirely possible to leverage Microsoft Azure's Cognitive Services for your Discord bot.  Of particular interest are the speech services, which will allow you to do [text-to-speech](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/text-to-speech) (the focus of this article) and [speech-to-text](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/speech-to-text).  This article will show you how to create a command to have your bot say something back to you, like Simon says.  Once you've got that under your belt, you can make modifications to have your bot speak to you based on whatever trigger and content your heart desires.  If you are feeling ambitious, you can even have your bot respond to voice messages using speech-to-text.  Just parse the returned text as if it were a regular Channel message or pass it through your Interactivity module for some real fun.

## 2. Prerequisites

* DSharpPlus >= 4.0
* DSharpPlus.VoiceNext
  * This article assumes that you've already followed the [VoiceNext](https://dsharpplus.emzi0767.com/articles/vnext_setup.html) article and have a bot that works and that can play music or some other audio.
* NAudio [[GitHub]](https://github.com/naudio/NAudio) [[NuGet]](https://www.nuget.org/packages/NAudio/)
* An account for [Microsoft Azure](https://portal.azure.com)

## 3. Text-to-Speech

Jumping right in, the process is as follows:

1. Follow along with [Try Speech Services for free](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/get-started) to get an API key for Speech Services.  The Free Tier pricing includes up to 5M characters free per month.  After that, it is $4 per 1M characters.  Not bad, really.

2. Add the [Azure Speech SDK NuGet package](https://aka.ms/csspeech/nuget) to your project.

3. Create a new class to do the Azure Speech processing using the Speech SDK.  It needs to request audio data from the Speech Synthesizer and then use NAudio to resample to Discord/Opus's 48kHz, 16bit stereo PCM requirement.  

    ```c#
    using DSharpPlus;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;
    using NAudio.Wave;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class SpeechModule
    {
        private DebugLogger debugLogger;

        public SpeechModule(DebugLogger debugLogger)
        {
            this.debugLogger = debugLogger;
        }

        public async Task<byte[]> SynthesisToSpeakerAsync(string text)
        {
            debugLogger.LogMessage(LogLevel.Info, Constants.ApplicationName, $"Azure Speech: Synthesizing speech for text [{text}]", DateTime.Now);

            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            var config = SpeechConfig.FromSubscription("subscription-key", "westus");

            var audioConfig = AudioConfig.FromStreamOutput(AudioOutputStream.CreatePullStream(AudioStreamFormat.GetDefaultOutputFormat()));
            //var audioConfig = AudioConfig.FromDefaultSpeakerOutput(); // if you want to hear it before processing

            // Creates a speech synthesizer using the default speaker as audio output.
            using (var synthesizer = new SpeechSynthesizer(config, audioConfig))
            using (var result = await synthesizer.SpeakTextAsync(text))
            {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    debugLogger.LogMessage(LogLevel.Info, Constants.ApplicationName, "Azure Speech: Speech synthesized.", DateTime.Now);
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    debugLogger.LogMessage(LogLevel.Error, Constants.ApplicationName, $"Azure Speech: CANCELED: Reason={cancellation.Reason}", DateTime.Now);

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        debugLogger.LogMessage(LogLevel.Error, Constants.ApplicationName, $"Azure Speech: CANCELED: ErrorCode={cancellation.ErrorCode}", DateTime.Now);
                        debugLogger.LogMessage(LogLevel.Error, Constants.ApplicationName, $"Azure Speech: CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]", DateTime.Now);
                        debugLogger.LogMessage(LogLevel.Error, Constants.ApplicationName, $"Azure Speech: CANCELED: Did you update the subscription info?", DateTime.Now);
                    }
                }

                // NAudio resampling from Azure Speech default to Opus default
                using (var output = new MemoryStream())
                using (var ms = new MemoryStream(result.AudioData))
                using (var rs = new RawSourceWaveStream(ms, new WaveFormat(16000, 16, 1)))
                using (var resampler = new MediaFoundationResampler(rs, new WaveFormat(48000, 16, 2)))
                {
                    byte[] bytes = new byte[rs.WaveFormat.AverageBytesPerSecond * 4];
                    while (true)
                    {
                        int bytesRead = resampler.Read(bytes, 0, bytes.Length);
                        if (bytesRead == 0)
                            break;
                        output.Write(bytes, 0, bytesRead);
                    }

                    return output.GetBuffer();
                }
            }
        }
    }
    ```

4. Add a new command to your CommandsModule.cs from the previous articles to use VoiceNext to send the resampled audio data to Discord through your bot.

    ```c#
    [Command("speak")]
    public async Task Speak(CommandContext ctx, string text)
    {
        if (String.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("No text to speak.");

        var vnext = ctx.Client.GetVoiceNext();

        var vnc = vnext.GetConnection(ctx.Guild);
        if (vnc == null)
            throw new InvalidOperationException("Not connected in this guild.");

        await ctx.RespondAsync("👌");

        var buffer = await new SpeechModule(vnext.Client.DebugLogger).SynthesisToSpeakerAsync(text);

        vnc.SendSpeaking(); // send a speaking indicator
        await vnc.GetTransmitStream().WriteAsync(buffer);
        await vnc.GetTransmitStream().FlushAsync();

        vnc.SendSpeaking(false);
    }
    ```

5. With your bot connected to your Guild and joined to a voice channel, like you did in the previous article, send the speak command to your bot (e.g., `-> speak "testing testing testing"`) and listen as your text is spoken back to you.

## 4. Speech-to-Text

TBD

## 5. References

[DSharpPlus Voice Next Setup](https://dsharpplus.emzi0767.com/articles/vnext_setup.html)

[Quickstart: Synthesize speech with the Speech SDK for .NET Core](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/quickstart-text-to-speech-dotnetcore)