// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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

namespace DSharpPlus.Net.WebSocket;

// Licensed from Clyde.NET (etc; I don't know how licenses work)

internal sealed class SocketLock : IDisposable
{
    public ulong ApplicationId { get; }

    private SemaphoreSlim LockSemaphore { get; }
    private CancellationTokenSource TimeoutCancelSource { get; set; }
    private CancellationToken TimeoutCancel => TimeoutCancelSource.Token;
    private Task UnlockTask { get; set; }
    private int MaxConcurrency { get; set; }

    public SocketLock(ulong appId, int maxConcurrency)
    {
        ApplicationId = appId;
        TimeoutCancelSource = null;
        MaxConcurrency = maxConcurrency;
        LockSemaphore = new SemaphoreSlim(maxConcurrency);
    }

    public async Task LockAsync()
    {
        await LockSemaphore.WaitAsync().ConfigureAwait(false);

        TimeoutCancelSource = new CancellationTokenSource();
        UnlockTask = Task.Delay(TimeSpan.FromSeconds(30), TimeoutCancel);
        _ = UnlockTask.ContinueWith(InternalUnlock, TaskContinuationOptions.NotOnCanceled);
    }

    public void UnlockAfter(TimeSpan unlockDelay)
    {
        if (TimeoutCancelSource == null || LockSemaphore.CurrentCount > 0)
        {
            return; // it's not unlockable because it's post-IDENTIFY or not locked
        }

        try
        {
            TimeoutCancelSource.Cancel();
            TimeoutCancelSource.Dispose();
        }
        catch { }
        TimeoutCancelSource = null;

        UnlockTask = Task.Delay(unlockDelay, CancellationToken.None);
        _ = UnlockTask.ContinueWith(InternalUnlock);
    }

    public Task WaitAsync()
        => LockSemaphore.WaitAsync();

    public void Dispose()
    {
        try
        {
            TimeoutCancelSource?.Cancel();
            TimeoutCancelSource?.Dispose();
        }
        catch { }
    }

    private void InternalUnlock(Task t)
        => LockSemaphore.Release(MaxConcurrency);
}
