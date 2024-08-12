// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA1034
#pragma warning disable CA1815

using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using CommunityToolkit.HighPerformance.Helpers;

namespace DSharpPlus.Entities;

// contains the implementation details for EnumeratePermissions
partial struct DiscordPermissions
{
    /// <summary>
    /// Presents a slim enumerable wrapper around a set of permissions.
    /// </summary>
    public readonly struct DiscordPermissionEnumerable : IEnumerable<DiscordPermission>
    {
        private readonly DiscordPermissionContainer data;

        internal DiscordPermissionEnumerable(DiscordPermissionContainer data)
            => this.data = data;

        /// <summary>
        /// Gets an enumerator for the present permission set.
        /// </summary>
        public readonly DiscordPermissionEnumerator GetEnumerator() => new(this.data);

        // implementations for IEnumerable<T>, we'd like to not box by default
        IEnumerator<DiscordPermission> IEnumerable<DiscordPermission>.GetEnumerator() => this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    /// <summary>
    /// Represents an enumerator for permission fields within a permission set.
    /// </summary>
    public struct DiscordPermissionEnumerator : IEnumerator<DiscordPermission>
    {
        private readonly DiscordPermissionContainer data;

        internal DiscordPermissionEnumerator(DiscordPermissionContainer data)
            => this.data = data;

        public readonly DiscordPermission Current => (DiscordPermission)(32 * this.block) + this.bit;

        readonly object IEnumerator.Current => this.Current;

        private int block = 0;
        private int bit = -1;

        public bool MoveNext()
        {
            for (; this.block < 4; this.block++)
            {
                this.bit++;
                uint value = this.data[this.block];

                if (BitOperations.PopCount(value) == 0)
                {
                    continue;
                }

                for (; this.bit < 32; this.bit++)
                {
                    if (BitHelper.HasFlag(value, this.bit))
                    {
                        return true;
                    }
                }

                this.bit = -1;
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
