// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;

using Xunit;

namespace DSharpPlus.Shared.Tests.Permissions;

public class OperatorTests
{
    public static ReadOnlySpan<byte> AllButFirstBit =>
    [
        0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
    ];

    [Fact]
    public void TestRemove_SingleBit()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        permissions = permissions.Remove(DiscordPermission.CreateInvite);

        Assert.Equal(DiscordPermissions.None, permissions);

        permissions = new(DiscordPermission.CreateInvite);
        Assert.Equal(DiscordPermissions.None, permissions - DiscordPermission.CreateInvite);
    }

    [Fact]
    public void TestRemove_Bulk()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite, DiscordPermission.BanMembers);
        permissions = permissions.Remove(DiscordPermission.CreateInvite, DiscordPermission.BanMembers);

        Assert.Equal(DiscordPermissions.None, permissions);
    }

    [Fact]
    public void TestRemove_Set()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite, DiscordPermission.BanMembers);
        DiscordPermissions remove = new(DiscordPermission.CreateInvite);

        Assert.Equal(DiscordPermission.BanMembers, permissions - remove);
    }

    [Fact]
    public void TestOr_Flag()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        DiscordPermissions two = permissions | DiscordPermission.BanMembers;

        Assert.Equal(new(DiscordPermission.CreateInvite, DiscordPermission.BanMembers), two);
    }

    [Fact]
    public void TestOr_Set()
    {
        DiscordPermissions one = new(DiscordPermission.AddReactions);
        DiscordPermissions two = new(DiscordPermission.Administrator);

        DiscordPermissions actual = one | two;

        Assert.Equal(new(DiscordPermission.AddReactions, DiscordPermission.Administrator), actual);
    }

    [Fact]
    public void TestAnd_Flag()
    {
        DiscordPermissions permissions = new(DiscordPermission.Administrator, DiscordPermission.Connect);
        Assert.Equal(DiscordPermission.Administrator, permissions & DiscordPermission.Administrator);
    }

    [Fact]
    public void TestAnd_Set()
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

        Assert.Equal(DiscordPermission.Administrator, permissions & mask);
    }

    [Fact]
    public void TestXor_Flag()
    {
        DiscordPermissions permissions = new(DiscordPermission.Administrator, DiscordPermission.Connect);
        Assert.Equal(DiscordPermission.Connect, permissions ^ DiscordPermission.Administrator);
    }

    [Fact]
    public void TestXor_Set()
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

        Assert.Equal(expected, permissions ^ mask);
    }

    [Fact]
    public void TestNot()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        DiscordPermissions expected = new(AllButFirstBit);

        Assert.Equal(expected, ~permissions);
    }
}
