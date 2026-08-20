#pragma warning disable IDE0040

using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.MemoryServices.Channels;

partial class AudioChannel
{
    private class DefaultReader : AudioChannelReader
    {
        private readonly AudioChannel channel;

        internal DefaultReader(AudioChannel channel)
            => this.channel = channel;

        public override async Task<AudioChannelReadResult> ReadAsync(CancellationToken ct)
        {
            Task<AudioBufferLease> readerTask = this.channel.underlying.Reader.ReadAsync(ct).AsTask();
            Task terminationTask = this.channel.terminationTcs.Task;

            await Task.WhenAny(readerTask, terminationTask);

            if (terminationTask.IsCompleted)
            {
                return new()
                {
                    State = AudioChannelState.Terminated,
                    IsAvailable = false
                };
            }
            else
            {
                return new()
                {
                    State = AudioChannelState.Open,
                    IsAvailable = true,
                    Buffer = readerTask.Result
                };
            }
        }

        public override AudioChannelReadResult TryRead()
        {
            if (this.channel.underlying.Reader.TryRead(out AudioBufferLease lease))
            {
                return new()
                {
                    State = AudioChannelState.Open,
                    IsAvailable = true,
                    Buffer = lease
                };
            }
            else if (this.channel.isPaused)
            {
                return new()
                {
                    State = AudioChannelState.Paused,
                    IsAvailable = false
                };
            }
            else if (this.channel.IsTerminationRequested)
            {
                return new()
                {
                    State = AudioChannelState.Terminated,
                    IsAvailable = false
                };
            }
            else
            {
                // the user failed to produce audio in time, but the channel isn't closed
                return new()
                {
                    State = AudioChannelState.Open,
                    IsAvailable = false
                };
            }
        }

        public override async Task WaitToReadAsync(CancellationToken ct)
        {
            Task readerTask = this.channel.underlying.Reader.WaitToReadAsync(ct).AsTask();
            Task terminationTask = this.channel.terminationTcs.Task;

            await Task.WhenAny(readerTask, terminationTask);
        }
    }
}
