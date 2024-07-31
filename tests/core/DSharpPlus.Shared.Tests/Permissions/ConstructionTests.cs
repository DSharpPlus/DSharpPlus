// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Numerics;

using DSharpPlus.Entities;

using Xunit;

namespace DSharpPlus.Shared.Tests.Permissions;

public class ConstructionTests
{
    private static ReadOnlySpan<byte> FirstBit =>
    [
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    ];

    private static ReadOnlySpan<byte> FirstTwoBits =>
    [
        0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    ];

    private static ReadOnlySpan<byte> ThirtyThirdBit =>
    [
        0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    ];

    [Fact]
    public void FirstBitSetCorrectly_Constructor()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        DiscordPermissions expected = new(FirstBit);

        Assert.Equal(expected, permissions);
    }

    [Fact]
    public void FirstBitSetCorrectly_Add()
    {
        DiscordPermissions permissions = DiscordPermissions.None.Add(DiscordPermission.CreateInvite);
        DiscordPermissions expected = new(FirstBit);

        Assert.Equal(expected, permissions);
    }

    [Fact]
    public void FirstTwoBitsSetCorrectly_Constructor()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite, DiscordPermission.KickMembers);
        DiscordPermissions expected = new(FirstTwoBits);

        Assert.Equal(expected, permissions);
    }

    [Fact]
    public void FirstTwoBitsSetCorrectly_Add()
    {
        DiscordPermissions permissions = DiscordPermissions.None
            .Add(DiscordPermission.CreateInvite)
            .Add(DiscordPermission.KickMembers);
        DiscordPermissions expected = new(FirstTwoBits);

        Assert.Equal(expected, permissions);
    }

    [Fact]
    public void FirstTwoBitsSetCorrectly_AddMultiple()
    {
        DiscordPermissions permissions = DiscordPermissions.None
            .Add(DiscordPermission.CreateInvite, DiscordPermission.KickMembers);
        DiscordPermissions expected = new(FirstTwoBits);

        Assert.Equal(expected, permissions);
    }

    [Fact]
    public void TestUnderlyingUInt32Rollover()
    {
        DiscordPermissions permissions = new(DiscordPermission.RequestToSpeak, DiscordPermission.CreateInvite);
        DiscordPermissions expected = new(ThirtyThirdBit);

        Assert.Equal(expected, permissions);
    }

    [Fact]
    public void FirstBitSetCorrectly_BigInteger()
    {
        BigInteger bigint = new(1);
        DiscordPermissions permissions = new(bigint);
        DiscordPermissions expected = new(FirstBit);

        Assert.Equal(expected, permissions);
    }

    [Fact]
    public void ThirtyThirdBitSetCorrectly_BigInteger()
    {
        BigInteger bigint = new(4294967297);
        DiscordPermissions permissions = new(bigint);
        DiscordPermissions expected = new(ThirtyThirdBit);

        Assert.Equal(expected, permissions);
    }

    [Fact]
    public void OpImplicit()
    {
        DiscordPermissions expected = new(FirstBit);

        Assert.Equal(expected, DiscordPermission.CreateInvite);
    }
}
