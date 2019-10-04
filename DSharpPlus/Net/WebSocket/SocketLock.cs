﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Net.WebSocket
{
    // Licensed from Clyde.NET (etc; I don't know how licenses work)

    internal sealed class SocketLock : IDisposable
    {
        public ulong ApplicationId { get; }

        private SemaphoreSlim LockSemaphore { get; }
        private CancellationTokenSource TimeoutCancelSource { get; set; }
        private CancellationToken TimeoutCancel => this.TimeoutCancelSource.Token;
        private Task UnlockTask { get; set; }

        public SocketLock(ulong appId)
        {
            this.ApplicationId = appId;
            this.TimeoutCancelSource = null;
            this.LockSemaphore = new SemaphoreSlim(1);
        }

        public async Task LockAsync()
        {
            await this.LockSemaphore.WaitAsync().ConfigureAwait(false);

            this.TimeoutCancelSource = new CancellationTokenSource();
            this.UnlockTask = Task.Delay(TimeSpan.FromSeconds(30), this.TimeoutCancel);
            _ = this.UnlockTask.ContinueWith(this.InternalUnlock, TaskContinuationOptions.NotOnCanceled);
        }

        public void UnlockAfter(TimeSpan unlockDelay)
        {
            if (this.TimeoutCancelSource == null || this.LockSemaphore.CurrentCount > 0)
                return; // it's not unlockable because it's post-IDENTIFY or not locked

            try
            {
                this.TimeoutCancelSource.Cancel();
                this.TimeoutCancelSource.Dispose();
            }
            catch { }
            this.TimeoutCancelSource = null;

            this.UnlockTask = Task.Delay(unlockDelay, CancellationToken.None);
            _ = this.UnlockTask.ContinueWith(this.InternalUnlock);
        }

        public Task WaitAsync()
            => this.LockSemaphore.WaitAsync();

        public void Dispose()
        {
            try
            {
                this.TimeoutCancelSource?.Cancel();
                this.TimeoutCancelSource?.Dispose();
            }
            catch { }
        }

        private void InternalUnlock(Task t)
            => this.LockSemaphore.Release();
    }
}
