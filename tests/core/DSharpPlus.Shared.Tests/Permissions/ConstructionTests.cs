// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Numerics;
using System.Threading.Tasks;

using DSharpPlus.Entities;

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

    [Test]
    public async Task FirstBitSetCorrectly_Constructor()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        DiscordPermissions expected = new(FirstBit);

        await Assert.That(expected).IsEqualTo(permissions);
    }

    [Test]
    public async Task FirstBitSetCorrectly_Add()
    {
        DiscordPermissions permissions = DiscordPermissions.None;
        permissions.Add(DiscordPermission.CreateInvite);
        DiscordPermissions expected = new(FirstBit);

        await Assert.That(expected).IsEqualTo(permissions);
    }

    [Test]
    public async Task FirstTwoBitsSetCorrectly_Constructor()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite, DiscordPermission.KickMembers);
        DiscordPermissions expected = new(FirstTwoBits);

        await Assert.That(expected).IsEqualTo(permissions);
    }

    [Test]
    public async Task FirstTwoBitsSetCorrectly_Add()
    {
        DiscordPermissions permissions = DiscordPermissions.None;
        permissions.Add(DiscordPermission.CreateInvite);
        permissions.Add(DiscordPermission.KickMembers);
        DiscordPermissions expected = new(FirstTwoBits);

        await Assert.That(expected).IsEqualTo(permissions);
    }

    [Test]
    public async Task FirstTwoBitsSetCorrectly_AddMultiple()
    {
        DiscordPermissions permissions = DiscordPermissions.None;
        permissions.Add(DiscordPermission.CreateInvite, DiscordPermission.KickMembers);
        DiscordPermissions expected = new(FirstTwoBits);

        await Assert.That(expected).IsEqualTo(permissions);
    }

    [Test]
    public async Task TestUnderlyingUInt32Rollover()
    {
        DiscordPermissions permissions = new(DiscordPermission.RequestToSpeak, DiscordPermission.CreateInvite);
        DiscordPermissions expected = new(ThirtyThirdBit);

        await Assert.That(expected).IsEqualTo(permissions);
    }

    [Test]
    public async Task FirstBitSetCorrectly_BigInteger()
    {
        BigInteger bigint = new(1);
        DiscordPermissions permissions = new(bigint);
        DiscordPermissions expected = new(FirstBit);

        await Assert.That(expected).IsEqualTo(permissions);
    }

    [Test]
    public async Task ThirtyThirdBitSetCorrectly_BigInteger()
    {
        BigInteger bigint = new(4294967297);
        DiscordPermissions permissions = new(bigint);
        DiscordPermissions expected = new(ThirtyThirdBit);

        await Assert.That(expected).IsEqualTo(permissions);
    }

    [Test]
    public async Task OpImplicit()
    {
        DiscordPermissions expected = new(FirstBit);

        await Assert.That(expected).IsEqualTo(DiscordPermission.CreateInvite);
    }
}
