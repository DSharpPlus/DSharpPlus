using DSharpPlus;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestBot
{
    /// <summary>
    /// AudioWrapper for DSharpPlus
    /// Original code by Axiom (suicvne)
    /// Rewritten by Nick 'Tiaqo' Strohm
    /// </summary>
    public class AudioTools
    {
        private DiscordClient client;

        private WaveFormat waveFormat;
        private BufferedWaveProvider bufferedWaveProvider;

        private Thread SendAudioThread, ReceiveAudioThread;

        public AudioTools(DiscordClient client)
        {
            this.client = client;

            client.VoiceClientConnected += (sender, e) =>
            {
                // ---[ Receiving ]---
                bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
                bufferedWaveProvider.BufferDuration = new TimeSpan(0, 0, 50);
            };
        }

        public void SendAudioFile(string file)
        {
            DiscordVoiceClient vc = client.GetVoiceClient();
            try
            {
                int ms = vc.VoiceConfig.FrameLengthMs;
                int channels = vc.VoiceConfig.Channels;
                int sampleRate = 48000;

                var outFormat = new WaveFormat(sampleRate, 16, channels);
                //int blockSize = 48 * 2 * channels * ms;
                int blockSize = 48 * 2 * channels * ms;
                byte[] buffer = new byte[blockSize];

                vc.SetSpeaking(true);

                string fileFormat = file.Split(new char[] { '.' }).Last();
                if (fileFormat == "mp3")
                {
                    using (var mp3Reader = new Mp3FileReader(file))
                    using (var resampler = new MediaFoundationResampler(mp3Reader, outFormat))
                    {
                        resampler.ResamplerQuality = 60;

                        int byteCount;
                        while ((byteCount = resampler.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (byteCount < blockSize)
                                for (int i = byteCount; i < blockSize; i++)
                                    buffer[i] = 0;
                            SendBytes(buffer);
                        }
                        resampler.Dispose();
                        mp3Reader.Close();
                    }
                }
                else if (fileFormat == "wav")
                {
                    using (var wavReader = new WaveFileReader(file))
                    using (var resampler = new MediaFoundationResampler(wavReader, outFormat))
                    {
                        resampler.ResamplerQuality = 60;

                        int byteCount;
                        while ((byteCount = resampler.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (byteCount < blockSize)
                                for (int i = byteCount; i < blockSize; i++)
                                    buffer[i] = 0;
                            SendBytes(buffer);
                        }
                        resampler.Dispose();
                        wavReader.Close();
                    }
                }
                else
                    throw new NotSupportedException($"File format \"{fileFormat}\" is not supported.");
            }
            catch (Exception ex) { client.GetTextClientLogger.Log(ex.Message, MessageLevel.Critical); }
        }

        public void SendBytes(byte[] voiceBytes)
        {
            DiscordVoiceClient vc = client.GetVoiceClient();
            try
            {
                if (vc.Connected)
                    vc.SendVoice(voiceBytes);
            }
            catch (Exception ex) { client.GetTextClientLogger.Log(ex.Message, MessageLevel.Critical); }
        }
    }
}