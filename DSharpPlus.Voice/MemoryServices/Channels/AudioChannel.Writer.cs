#pragma warning disable IDE0040

namespace DSharpPlus.Voice.MemoryServices.Channels;

partial class AudioChannel
{
    private class DefaultWriter : AudioChannelWriter
    {
        private readonly AudioChannel channel;

        internal DefaultWriter(AudioChannel channel)
            => this.channel = channel;
        
        public override void Terminate()
        {
            this.channel.IsTerminationRequested = true;
            this.channel.terminationTcs.TrySetResult();
        }

        public override bool TryPause()
        {
            if (this.channel.IsTerminationRequested)
            {
                return false;
            }

            this.channel.isPaused = true;
            return true;
        }

        public override bool TryWrite(AudioBufferLease buffer)
        {
            if (this.channel.IsTerminationRequested)
            {
                return false;
            }

            this.channel.isPaused = false;
            return this.channel.underlying.Writer.TryWrite(buffer);
        }
    }
}
