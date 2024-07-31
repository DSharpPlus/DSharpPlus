// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;

using Xunit;

namespace DSharpPlus.Shared.Tests.Permissions;

public class EqualityTests
{
    private static ReadOnlySpan<byte> FirstBit =>
    [
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    ];

    [Fact]
    public void EqualsCorrect_OneBit()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        DiscordPermissions expected = new(FirstBit);

        Assert.True(expected.Equals(permissions));
    }
}
