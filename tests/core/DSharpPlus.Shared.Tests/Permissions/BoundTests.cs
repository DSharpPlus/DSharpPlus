// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

// if you find youself here, my condolences.
// here we test whether we read or write out of bounds at any point. currently, these tests work only on windows
// (because i was too lazy to write allocation code for linux, this just uses terrafx on windows), and they're not very
// pleasant to run, but avoiding out of bounds memory accesses is probably worth it.
// also, yes, i am aware this leaks memory. for some reason beyond my comprehension, VirtualFree fails with the error message
// "success". and since this is a test type that will be torn down soon enough, and it's only 12kb, it's not the end of the
// world to leak this here.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

using DSharpPlus.Entities;
using DSharpPlus.Shared.Tests.Util;

using TerraFX.Interop.Windows;

using Xunit;

namespace DSharpPlus.Shared.Tests.Permissions;

[SupportedOSPlatform("windows")]
public sealed unsafe class BoundTests
{
    private readonly byte* usableAllocation;

    public BoundTests()
    {
        nuint zero = nuint.Zero;
        uint _;

        // allocate three pages, immediately commit them and mark them as NO_ACCESS
        void* allocHandle = Windows.VirtualAlloc
        (
            lpAddress: (void*)zero,
            dwSize: (nuint)(3 * Environment.SystemPageSize),
            flAllocationType: 0x3000, // RESERVE and COMMIT
            flProtect: 0x1 // NO_ACCESS
        );

        if (allocHandle == null || allocHandle == (void*)nuint.Zero)
        {
            throw new InvalidOperationException($"Failed to allocate three sequential pages. Handle: {(nuint)allocHandle}");
        }

        // we allocated successfully, now mark the middle page as READWRITE

        BOOL success = Windows.VirtualProtect
        (
            lpAddress: (byte*)allocHandle + Environment.SystemPageSize,
            dwSize: (nuint)Environment.SystemPageSize,
            flNewProtect: 0x4, // READWRITE
            lpflOldProtect: &_
        );

        if (success != BOOL.TRUE)
        {
            throw new InvalidOperationException("Failed to patch protection status of the middle page.");
        }

        // everything is successful, store the pointers for use in tests

        this.usableAllocation = (byte*)allocHandle + Environment.SystemPageSize;
    }

    private static int GetLength() => 16;

    private ref DiscordPermissions AllocateStart()
        => ref Unsafe.As<byte, DiscordPermissions>(ref Unsafe.AsRef<byte>(this.usableAllocation));

    private ref DiscordPermissions AllocateEnd()
    {
        return ref Unsafe.As<byte, DiscordPermissions>
        (
            ref Unsafe.AsRef<byte>(this.usableAllocation + Environment.SystemPageSize - GetLength())
        );
    }

    [FactExceptOnlyOnWindows]
    public void VectorOpsInBounds_MemoryRegionStart()
    {
        try
        {
            // this includes the OR operator
            scoped ref DiscordPermissions permissionsAdd = ref this.AllocateStart();
            permissionsAdd += (DiscordPermission)117;

            scoped ref DiscordPermissions permissionsRemove = ref this.AllocateStart();
            permissionsRemove -= (DiscordPermission)117;

            scoped ref DiscordPermissions permissionsNegate = ref this.AllocateStart();
            permissionsNegate = ~permissionsNegate;

            scoped ref DiscordPermissions permissionsAnd = ref this.AllocateStart();
            permissionsAnd &= DiscordPermissions.All;

            scoped ref DiscordPermissions permissionsXor = ref this.AllocateStart();
            permissionsXor ^= DiscordPermissions.All;
        }
        catch (AccessViolationException)
        {
            Assert.Fail("Access violation thrown, a starting boundary has been violated.");
        }
    }

    [FactExceptOnlyOnWindows]
    public void VectorOpsInBounds_MemoryRegionEnd()
    {
        try
        {
            // this includes the OR operator
            scoped ref DiscordPermissions permissionsAdd = ref this.AllocateEnd();
            permissionsAdd += (DiscordPermission)117;

            scoped ref DiscordPermissions permissionsRemove = ref this.AllocateEnd();
            permissionsRemove -= (DiscordPermission)117;

            scoped ref DiscordPermissions permissionsNegate = ref this.AllocateEnd();
            permissionsNegate = ~permissionsNegate;

            scoped ref DiscordPermissions permissionsAnd = ref this.AllocateEnd();
            permissionsAnd &= DiscordPermissions.All;

            scoped ref DiscordPermissions permissionsXor = ref this.AllocateEnd();
            permissionsXor ^= DiscordPermissions.All;
        }
        catch (AccessViolationException)
        {
            Assert.Fail("Access violation thrown, an ending boundary has been violated.");
        }
    }
}
