// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Xunit;

namespace DSharpPlus.Shared.Tests.Permissions;

public class UtilityTests
{
    [Fact]
    public void HasPermissionRespectsAdministrator()
    {
        DiscordPermissions permissions = new(DiscordPermission.Administrator, DiscordPermission.AddReactions);
        Assert.True(permissions.HasPermission(DiscordPermission.BanMembers));

        permissions = new(DiscordPermission.AddReactions);
        Assert.False(permissions.HasPermission(DiscordPermission.BanMembers));
    }

    [Fact]
    public void HasAnyPermissionAlwaysSucceedsWithAdministrator()
    {
        DiscordPermissions permissions = new(DiscordPermission.Administrator, DiscordPermission.AddReactions);
        Assert.True(permissions.HasAnyPermission(DiscordPermission.BanMembers, DiscordPermission.CreateInvite));
    }

    [Fact]
    public void HasAnyPermissionWithoutAdministrator()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite, DiscordPermission.AddReactions);
        Assert.True(permissions.HasAnyPermission(DiscordPermission.BanMembers, DiscordPermission.CreateInvite));
        Assert.False(permissions.HasAnyPermission(DiscordPermission.AttachFiles, DiscordPermission.Connect));
    }

    [Fact]
    public void HasAllPermissionsAlwaysSucceedsWithAdministrator()
    {
        DiscordPermissions permissions = new(DiscordPermission.Administrator);
        Assert.True(permissions.HasAllPermissions(DiscordPermission.CreatePrivateThreads, DiscordPermission.KickMembers));
    }

    [Fact]
    public void HasAllPermissionsWithoutAdministrator()
    {
        DiscordPermissions permissions = new(DiscordPermission.CreateInvite, DiscordPermission.AddReactions);
        permissions += DiscordPermission.ManageGuild;

        Assert.True(permissions.HasAllPermissions(DiscordPermission.ManageGuild, DiscordPermission.CreateInvite));
        Assert.False(permissions.HasAllPermissions(DiscordPermission.CreateInvite, DiscordPermission.Connect));
    }
}
