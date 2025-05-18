// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.Shared.Tests.Permissions;

public class OperatorTests
{
    public static ReadOnlySpan<byte> AllButFirstBit =>
    [
        0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
    ];

    [Test]
    public async Task TestRemove_SingleBit()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        permissions.Remove(DiscordPermission.CreateInvite);

        await Assert.That(permissions).IsEqualTo(DiscordPermissions.None);

        permissions = new(DiscordPermission.CreateInvite);
        await Assert.That(permissions - DiscordPermission.CreateInvite).IsEqualTo(DiscordPermissions.None);
    }

    [Test]
    public async Task TestRemove_Bulk()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite, DiscordPermission.BanMembers);
        permissions.Remove([DiscordPermission.CreateInvite, DiscordPermission.BanMembers]);

        await Assert.That(permissions).IsEqualTo(DiscordPermissions.None);
    }

    [Test]
    public async Task TestRemove_Set()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite, DiscordPermission.BanMembers);
        DiscordPermissions remove = new(DiscordPermission.CreateInvite);

        await Assert.That(permissions - remove).IsEqualTo(DiscordPermission.BanMembers);
    }

    [Test]
    public async Task TestOr_Flag()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        DiscordPermissions two = permissions | DiscordPermission.BanMembers;

        await Assert.That(two).IsEqualTo(new(DiscordPermission.CreateInvite, DiscordPermission.BanMembers));
    }

    [Test]
    public async Task TestOr_Set()
    {
        DiscordPermissions one = new(DiscordPermission.AddReactions);
        DiscordPermissions two = new(DiscordPermission.Administrator);

        DiscordPermissions actual = one | two;

        await Assert.That(actual).IsEqualTo(new(DiscordPermission.AddReactions, DiscordPermission.Administrator));
    }

    [Test]
    public async Task TestAnd_Flag()
    {
        DiscordPermissions permissions = new(DiscordPermission.Administrator, DiscordPermission.Connect);
        await Assert.That(permissions & DiscordPermission.Administrator).IsEqualTo(DiscordPermission.Administrator);
    }

    [Test]
    public async Task TestAnd_Set()
    {
        DiscordPermissions permissions = new
        (
            DiscordPermission.Administrator,
            DiscordPermission.ChangeNickname,
            DiscordPermission.CreatePublicThreads,
            DiscordPermission.Speak
        );

        DiscordPermissions mask = new
        (
            DiscordPermission.Administrator,
            DiscordPermission.CreateInvite
        );

        await Assert.That(permissions & mask).IsEqualTo(DiscordPermission.Administrator);
    }

    [Test]
    public async Task TestXor_Flag()
    {
        DiscordPermissions permissions = new(DiscordPermission.Administrator, DiscordPermission.Connect);
        await Assert.That(permissions ^ DiscordPermission.Administrator).IsEqualTo(DiscordPermission.Connect);
    }

    [Test]
    public async Task TestXor_Set()
    {
        DiscordPermissions permissions = new
        (
            DiscordPermission.Administrator,
            DiscordPermission.ChangeNickname,
            DiscordPermission.CreatePublicThreads,
            DiscordPermission.Speak
        );

        DiscordPermissions mask = new
        (
            DiscordPermission.Administrator,
            DiscordPermission.CreateInvite
        );

        DiscordPermissions expected = new
        (
            DiscordPermission.ChangeNickname,
            DiscordPermission.CreatePublicThreads,
            DiscordPermission.Speak,
            DiscordPermission.CreateInvite
        );

        await Assert.That(permissions ^ mask).IsEqualTo(expected);
    }

    [Test]
    public async Task TestNot()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        DiscordPermissions expected = new(AllButFirstBit);

        await Assert.That(~permissions).IsEqualTo(expected);
    }

    [Test]
    public async Task TestCombine_NonOverlapping()
    {
        DiscordPermissions set1 = new(DiscordPermission.CreateInvite);
        DiscordPermissions set2 = new(DiscordPermission.Connect);
        DiscordPermissions expected = new(DiscordPermission.CreateInvite, DiscordPermission.Connect);

        await Assert.That(DiscordPermissions.Combine(set1, set2)).IsEqualTo(expected);
    }

    [Test]
    public async Task TestCombine_Overlapping()
    {
        DiscordPermissions set1 = new(DiscordPermission.CreateInvite, DiscordPermission.Connect);
        DiscordPermissions set2 = new(DiscordPermission.Connect);
        DiscordPermissions expected = new(DiscordPermission.CreateInvite, DiscordPermission.Connect);

        await Assert.That(DiscordPermissions.Combine(set1, set2)).IsEqualTo(expected);
    }
}
