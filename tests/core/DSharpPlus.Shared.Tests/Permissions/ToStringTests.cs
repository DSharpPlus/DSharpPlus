// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Xunit;

namespace DSharpPlus.Shared.Tests.Permissions;

public class ToStringTests
{
    [Fact]
    public void TestFirstBit()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        Assert.Equal("1", permissions.ToString());
    }

    [Fact]
    public void TestByteOrder_SecondByte()
    {
        DiscordPermissions permissions = new(DiscordPermission.PrioritySpeaker);
        Assert.Equal("256", permissions.ToString());
    }

    [Fact]
    public void TestInternalElementOrder_SecondElement()
    {
        DiscordPermissions permissions = new(DiscordPermission.RequestToSpeak);
        Assert.Equal("4294967296", permissions.ToString());
    }

    [Fact]
    public void TestRawFirstBit()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);

        Assert.Equal
        (
            "DiscordPermissions - raw value: 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
            permissions.ToString("raw")
        );
    }

    [Fact]
    public void TestRawSecondElement()
    {
        DiscordPermissions permissions = new(DiscordPermission.RequestToSpeak);

        Assert.Equal
        (
            "DiscordPermissions - raw value: 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00",
            permissions.ToString("raw")
        );
    }

    [Fact]
    public void TestNameFirstElement()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);

        Assert.Equal("Create Invites", permissions.ToString("name"));
    }

    [Fact]
    public void TestNameOrder()
    {
        DiscordPermissions permissions = new(DiscordPermission.RequestToSpeak, DiscordPermission.CreateInvite);

        Assert.Equal("Create Invites, Request to Speak", permissions.ToString("name"));
    }
}
