#pragma warning disable IDE0040

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance.Helpers;

namespace DSharpPlus.Entities;

// contains the implementation details for enumerating permissions
partial struct DiscordPermissions
{
    /// <summary>
    /// Gets an enumerator for the present permission set.
    /// </summary>
    public readonly DiscordPermissionEnumerator GetEnumerator() => new(this.data);

    // implementations for IEnumerable<T>, we'd like to not box by default
    readonly IEnumerator<DiscordPermission> IEnumerable<DiscordPermission>.GetEnumerator() => GetEnumerator();
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Represents an enumerator for permission fields within a permission set.
    /// </summary>
    public struct DiscordPermissionEnumerator : IEnumerator<DiscordPermission>
    {
        private DiscordPermissionContainer data;

        internal DiscordPermissionEnumerator(DiscordPermissionContainer data)
            => this.data = data;

#pragma warning disable DSP0009
        public readonly DiscordPermission Current => (DiscordPermission)(this.block << 5) + this.bit;
#pragma warning restore DSP0009

        readonly object IEnumerator.Current => this.Current;

        private bool hasEnteredBlock = false;
        private short block = 0;
        private int bit = -1;

        public bool MoveNext()
        {
            for (; this.block < ContainerElementCount; this.block++)
            {
                this.bit++;
                uint value = Unsafe.Add(ref Unsafe.As<DiscordPermissionContainer, uint>(ref this.data), this.block);

                if (value == 0)
                {
                    continue;
                }

                if (!this.hasEnteredBlock)
                {
                    this.hasEnteredBlock = true;
                    this.bit = BitOperations.TrailingZeroCount(value);
                }

                for (; this.bit < (32 - BitOperations.LeadingZeroCount(value)); this.bit++)
                {
                    if (BitHelper.HasFlag(value, this.bit))
                    {
                        return true;
                    }
                }

                this.bit = -1;
                this.hasEnteredBlock = false;
            }

            return false;
        }

        public void Reset()
        {
            this.block = 0;
            this.bit = -1;
        }

        public readonly void Dispose()
        {

        }
    }
}
