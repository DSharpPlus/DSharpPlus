// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.Shared.Tests.Permissions;

public class EqualityTests
{
    [Test]
    public async Task EqualsCorrect_OneBit()
    {
        DiscordPermissions a = new(DiscordPermission.CreateInvite);
        DiscordPermissions b = new(DiscordPermission.CreateInvite);

        // we explicitly don't want the default equals assertion here
        await Assert.That(a.Equals(b)).IsTrue();
    }

    [Test]
    public async Task EqualsCorrect_ThirtyThirdBit()
    {
        DiscordPermissions a = new(DiscordPermission.RequestToSpeak);
        DiscordPermissions b = new(DiscordPermission.RequestToSpeak);

        await Assert.That(a.Equals(b)).IsTrue();
    }
}
