// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0058

using System.Text.Json;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Models.Serialization.Converters;

using Xunit;

namespace DSharpPlus.Internal.Models.Tests.Converters;

public class DiscordPermissionTests
{
    private readonly JsonSerializerOptions options;

    public DiscordPermissionTests()
    {
        this.options = new();
        this.options.Converters.Add(new DiscordPermissionConverter());
    }

    [Fact]
    public void DeserializePermissions()
    {
        DiscordPermissions expected = new(DiscordPermission.PrioritySpeaker);
        DiscordPermissions actual = JsonSerializer.Deserialize<DiscordPermissions>("256", this.options);

        Assert.Equal(expected, actual);
    }
}
