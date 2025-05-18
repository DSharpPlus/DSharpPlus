// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.Shared.Tests.Permissions;

public class UtilityTests
{
    [Test]
    public async Task HasPermissionRespectsAdministrator()
    {
        DiscordPermissions permissions = new(DiscordPermission.Administrator, DiscordPermission.AddReactions);
        await Assert.That(permissions.HasPermission(DiscordPermission.BanMembers)).IsTrue();

        permissions = new(DiscordPermission.AddReactions);
        await Assert.That(permissions.HasPermission(DiscordPermission.BanMembers)).IsFalse();
    }

    [Test]
    public async Task HasAnyPermissionAlwaysSucceedsWithAdministrator()
    {
        DiscordPermissions permissions = new(DiscordPermission.Administrator, DiscordPermission.AddReactions);
        await Assert.That(permissions.HasAnyPermission([DiscordPermission.BanMembers, DiscordPermission.CreateInvite])).IsTrue();
    }

    [Test]
    public async Task HasAnyPermissionWithoutAdministrator()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite, DiscordPermission.AddReactions);

        using (Assert.Multiple())
        {
            await Assert.That(permissions.HasAnyPermission([DiscordPermission.BanMembers, DiscordPermission.CreateInvite])).IsTrue();
            await Assert.That(permissions.HasAnyPermission([DiscordPermission.AttachFiles, DiscordPermission.Connect])).IsFalse();
        }
    }

    [Test]
    public async Task HasAllPermissionsAlwaysSucceedsWithAdministrator()
    {
        DiscordPermissions permissions = new(DiscordPermission.Administrator);
        await Assert.That(permissions.HasAllPermissions([DiscordPermission.CreatePrivateThreads, DiscordPermission.KickMembers])).IsTrue();
    }

    [Test]
    public async Task HasAllPermissionsWithoutAdministrator()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite, DiscordPermission.AddReactions);
        DiscordPermissions testificate = [DiscordPermission.CreateInvite, DiscordPermission.Connect];
        permissions += DiscordPermission.ManageGuild;

        using (Assert.Multiple())
        {
            await Assert.That(permissions.HasAllPermissions([DiscordPermission.ManageGuild, DiscordPermission.CreateInvite])).IsTrue();
            await Assert.That(permissions.HasAllPermissions([DiscordPermission.CreateInvite, DiscordPermission.Connect])).IsFalse();
            await Assert.That(testificate.HasPermission(DiscordPermission.Connect)).IsTrue();
        }
    }
}
