# DSharpPlus + Azure Speech Services

## 1. Introduction
With a little bit of effort, it is entirely possible to leverage Microsoft Azure's Cognitive Services for your Discord
bot. Of particular interest are the speech services, which will allow you to do
[text-to-speech](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/text-to-speech)
(the focus of this article) and
[speech-to-text](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/speech-to-text).
This article will show you how to create a command to have your bot say something back to you, like Simon says. Once you've
got that under your belt, you can make modifications to have your bot speak to you based on whatever trigger and content
your heart desires. If you are feeling ambitious, you can even have your bot respond to voice messages using speech-to-text.
Just parse the returned text as if it were a regular Channel message or pass it through your Interactivity module for some
real fun.

## 2. Prerequisites
* DSharpPlus >= 4.0
* DSharpPlus.VoiceNext
  * This article assumes that you've already followed the [VoiceNext](/articles/vnext_setup.html) article and have a bot
    that works and that can play music or some other audio.
* NAudio [[GitHub]](https://github.com/naudio/NAudio) [[NuGet]](https://www.nuget.org/packages/NAudio/)
* An account for [Microsoft Azure](https://portal.azure.com)

## 3. Text-to-Speech
Jumping right in, the process is as follows:

1. Follow along with [Try Speech Services for free](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/get-started)
   to get an API key for Speech Services. The Free Tier pricing includes up to 5M characters free per month. After that,
   it is $4 per 1M characters. Not bad, really.
2. Add the [Azure Speech SDK NuGet package](https://aka.ms/csspeech/nuget) to your project.
3. Create a new class to do the Azure Speech processing using the Speech SDK. It needs to request audio data from the
   Speech Synthesizer and then use NAudio to resample to Discord/Opus's 48kHz, 16bit stereo PCM requirement.

    ```c#
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using DSharpPlus;

    public class SpeechModule
    {
        private DebugLogger debugLogger;

        public SpeechModule(DebugLogger debugLogger)
        {
            this.debugLogger = debugLogger;
        }
    }
    ```
    Ok, now that we have the bare bones of a class ready to go with the D#+ DebugLogger, let's start building out the
    functionality. The first thing that the class needs to do is to build out the Azure Speech SDK speech synthesizer.
    In order to keep things concise and properly encapsulated, let's create a new method to just that. Since we are
    not intending for the Azure Speech SDK to be used outside of this module, let's mark it as private. Don't forget
    to add the new using statements to the top of the class file. When building out a SpeechSynthesizer, you need to 
    tell it how to connect to Azure and in what manner to play audio. What we want here is a stream, so that we can
    send it along to Voice Next, through memory, for playback in Discord.
    ```c#
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;

    private SpeechSynthesizer BuildAzureSpeechSynthesizer()
    {
        // Creates an instance of a speech config with specified subscription key and service region.
        // Replace with your own subscription key and service region (e.g., "westus").
        //var config = SpeechConfig.FromSubscription("subscription-key", "region");
        var config = SpeechConfig.FromSubscription("4cc709274fd2478294430b2cf66d3a91", "northcentralus");

        // Create an audio config to tell Azure Speech SDK to return speech output as a memory stream
        // using its default output format (16kHz, 16bit, mono).
        var audioConfig = AudioConfig.FromStreamOutput(AudioOutputStream.CreatePullStream(AudioStreamFormat.GetDefaultOutputFormat()));

        // Create an instance of the Azure Speech SDK speech synthesizer
        return new SpeechSynthesizer(config, audioConfig);
    }
    ```
    Now that we have a way to get the speech synthesizer, let's put it to use. There really isn't much to it at
    all: the speech synthesizer has an asynchronous method on it that will do the speaking for you. You'll see
    below that there is a method called `SpeechWasSynthesized` that we haven't defined yet, but in short we want to
    make sure that everything went smoothly inside the Azure Speech SDK before we try to send the audio stream on to
    Voice Next. Similarly, you'll see a method called `ConvertAudioToSupportedFormat`, used to make sure that the audio
    stream is sent in a format that Discord can handle. More details about those, next.
    ```c#
    public async Task<byte[]> SynthesisToStreamAsync(string text)
        {
            debugLogger.LogMessage(LogLevel.Info, Constants.ApplicationName, $"Synthesizing speech for text [{text}]", DateTime.Now);

            // Creates a speech synthesizer using the default speaker as audio output.
            using (var synthesizer = BuildAzureSpeechSynthesizer())
            using (var result = await synthesizer.SpeakTextAsync(text))
            {
                if (SpeechWasSynthesized(result))
                {
                    debugLogger.LogMessage(LogLevel.Info, Constants.ApplicationName, $"Speech synthesized for text [{text}]", DateTime.Now);
                    return ConvertAudioToSupportedFormat(result.AudioData);
                }
                else
                    debugLogger.LogMessage(LogLevel.Error, Constants.ApplicationName, $"Speech synthesized failed for text [{text}]", DateTime.Now);

                return new byte[0];
            }
        }
    ```
    If you followed along with the Azure Speech SDK, you'll recognize the following code. We're tweaking it a little so
    that the logging goes through D#+ instead of through the application console. That'll add nice timestamps and coloration.
    Also, instead of building our speech processing logic around the full complexities of Azure Speech SDK, we're just bundling
    up the result into either success or failure.
    ```c#
    private bool SpeechWasSynthesized(SpeechSynthesisResult result)
    {
        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            return true;
        }
        else if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            debugLogger.LogMessage(LogLevel.Error, Constants.ApplicationName, $"CANCELED: Reason={cancellation.Reason}", DateTime.Now);

            if (cancellation.Reason == CancellationReason.Error)
            {
                debugLogger.LogMessage(LogLevel.Error, Constants.ApplicationName, $"CANCELED: ErrorCode={cancellation.ErrorCode}", DateTime.Now);
                debugLogger.LogMessage(LogLevel.Error, Constants.ApplicationName, $"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]", DateTime.Now);
                debugLogger.LogMessage(LogLevel.Error, Constants.ApplicationName, $"CANCELED: Did you update the subscription info?", DateTime.Now);
            }
        }

        return false;
    }
    ```
    And for the final part of the speech module, we need to fill out the details of converting the audio stream into something
    that Discord can handle (48kHz, 16bit, stereo). The Azure Speech Service currently only supports PCM at 16kHz, 16bit, mono.
    In order to do the audio format translation, i.e. resampling, we're going to pull in another library called NAudio. It is
    considered to be on par with using ffmpeg, performance wise, so the resampling ought to happen about as fast as you would
    expect. Not being an audio expert, myself, I found the core algorithm used here and adapted it to our purpose.  Apparently,
    the method was originally from MonoGame, when it used NAudio to convert WAV formats.
    ```c#
    using NAudio.Wave;

    private static byte[] ConvertAudioToSupportedFormat(byte[] audioData)
        {
            // NAudio resampling from Azure Speech default to Opus default
            using (var output = new MemoryStream())
            using (var ms = new MemoryStream(audioData))
            using (var rs = new RawSourceWaveStream(ms, new WaveFormat(16000, 16, 1)))
            using (var resampler = new MediaFoundationResampler(rs, new WaveFormat(48000, 16, 2)))
            {
                // thanks https://csharp.hotexamples.com/examples/NAudio.Wave/MediaFoundationResampler/Read/php-mediafoundationresampler-read-method-examples.html#0xe8c3188aa82ab5c60c681c14b7336b52f1b3546fd75d133baef6572074b6028c-125,,155,
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
    ```
    Phew, now that all of that is done, we are ready for the next step.
4. Add a new command to your CommandsModule.cs from the previous articles to use VoiceNext to send the resampled audio
   data to Discord through your bot.
    ```c#
    [Command("speak")]
    public async Task Speak(CommandContext ctx, [RemainingText] string text)
    {
        // Guard inputs
        if (String.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("No text to speak.");

        // Get a VoiceNextConnection object
        var vnc = ctx.Client.GetVoiceNext().GetConnection(ctx.Guild);
        if (vnc == null)
            throw new InvalidOperationException("Not connected in this guild.");

        // Process the speech request
        await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":thinking:"));

        var buffer = await new SpeechModule(ctx.Client.DebugLogger).SynthesisToStreamAsync(text);

        if (buffer.Length == 0)
        {
            await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsdown:"));
            return;
        }
        else
        {
            await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsup:"));

            vnc.SendSpeaking(); // send a speaking indicator

            await vnc.GetTransmitStream().WriteAsync(buffer);
            await vnc.GetTransmitStream().FlushAsync();

            vnc.SendSpeaking(false); // end the speaking indicator
        }
    }
    ```
5. With your bot connected to your Guild and joined to a voice channel, like you did in the previous article, send the
   speak command to your bot (e.g., `;;speak "testing testing testing"`) and listen as your text is spoken back to you.

## 4. Speech-to-Text
TODO

## 5. References
[DSharpPlus Voice Next Setup](/articles/vnext_setup.html)
[Quickstart: Synthesize speech with the Speech SDK for .NET Core](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/quickstart-text-to-speech-dotnetcore)