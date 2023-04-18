// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
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
        private int MaxConcurrency { get; set; }

        public SocketLock(ulong appId, int maxConcurrency)
        {
            this.ApplicationId = appId;
            this.TimeoutCancelSource = null;
            this.MaxConcurrency = maxConcurrency;
            this.LockSemaphore = new SemaphoreSlim(maxConcurrency);
        }

        public async Task LockAsync()
        {
            await this.LockSemaphore.WaitAsync();

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
            => this.LockSemaphore.Release(this.MaxConcurrency);
    }
}
