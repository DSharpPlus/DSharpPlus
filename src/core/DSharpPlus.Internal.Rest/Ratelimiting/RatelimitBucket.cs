// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace DSharpPlus.Internal.Rest.Ratelimiting;

/// <summary>
/// Represents a full ratelimiting bucket.
/// </summary>
// this is a class because we use a ConditionalWeakTable to hold it, but there's room for improvement here.
internal sealed class RatelimitBucket
{
    public required float Expiry { get => this.expiry; set => this.expiry = value; }
    private float expiry;

    public required int Limit { get => this.limit; set => this.limit = value; }
    private int limit;

    public required int Remaining { get => this.remaining; set => this.remaining = value; }
    private int remaining;

    public required int Reserved { get => this.reserved; set => this.reserved = value; }
    private int reserved;

    public void UpdateFromResponse(float expiry, int limit, int remaining)
    {
        Interlocked.Exchange(ref this.expiry, expiry);
        Interlocked.Exchange(ref this.limit, limit);
        Interlocked.Exchange(ref this.remaining, remaining);
        Interlocked.Decrement(ref this.reserved);
    }

    public void Reserve() => Interlocked.Increment(ref this.reserved);
    public void CancelReservation() => Interlocked.Decrement(ref this.reserved);
}
