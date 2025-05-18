// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.Shared.Tests.Permissions;

public class ToStringTests
{
    [Test]
    public async Task TestFirstBit()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);
        await Assert.That(permissions.ToString()).IsEqualTo("1");
    }

    [Test]
    public async Task TestByteOrder_SecondByte()
    {
        DiscordPermissions permissions = new(DiscordPermission.PrioritySpeaker);
        await Assert.That(permissions.ToString()).IsEqualTo("256");
    }

    [Test]
    public async Task TestInternalElementOrder_SecondElement()
    {
        DiscordPermissions permissions = new(DiscordPermission.RequestToSpeak);
        await Assert.That(permissions.ToString()).IsEqualTo("4294967296");
    }

    [Test]
    public async Task TestRawFirstBit()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);

        await Assert.That(permissions.ToString("raw")).IsEqualTo
        (
            "DiscordPermissions - raw value: 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"
        );
    }

    [Test]
    public async Task TestRawSecondElement()
    {
        DiscordPermissions permissions = new(DiscordPermission.RequestToSpeak);

        await Assert.That(permissions.ToString("raw")).IsEqualTo
        (
            "DiscordPermissions - raw value: 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00"
        );
    }

    [Test]
    public async Task TestNameFirstElement()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite);

        await Assert.That(permissions.ToString("name")).IsEqualTo("Create Invites");
    }

    [Test]
    public async Task TestNameOrder()
    {
        DiscordPermissions permissions = new(DiscordPermission.RequestToSpeak, DiscordPermission.CreateInvite);

        await Assert.That(permissions.ToString("name")).IsEqualTo("Create Invites, Request to Speak");
    }

    [Test]
    public async Task TestUndefinedFlags()
    {
        DiscordPermissions permissions = new((DiscordPermission)48, (DiscordPermission)65, (DiscordPermission)97, (DiscordPermission)127);

        await Assert.That(permissions.ToString("name")).IsEqualTo("48, 65, 97, 127");
    }

    // when updating this test, try to find holes to use for this
    [Test]
    public async Task TestNameOrderUndefinedFlags()
    {
        DiscordPermissions permissions = new(DiscordPermission.ReadMessageHistory, (DiscordPermission)48, DiscordPermission.UseExternalApps);

        await Assert.That(permissions.ToString("name")).IsEqualTo("Read Message History, 48, Use External Apps");
    }

    [Test]
    public async Task TestCustomFormatThrowsIfMalformed()
    {
        DiscordPermissions permissions = new(DiscordPermission.ReadMessageHistory, DiscordPermission.UseExternalApps);

        await Assert.ThrowsAsync<FormatException>(() =>
        {
            _ = permissions.ToString("name:");
            return Task.CompletedTask;
        });

        await Assert.ThrowsAsync<FormatException>(() =>
        {
            _ = permissions.ToString("name:with a format but without the permission marker");
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task TestCustomFormat()
    {
        DiscordPermissions permissions = new(DiscordPermission.ReadMessageHistory, DiscordPermission.UseExternalApps);
        string expected = " - Read Message History\r\n - Use External Apps\r\n";

        await Assert.That(permissions.ToString("name: - {permission}\r\n")).IsEqualTo(expected);
    }
}
