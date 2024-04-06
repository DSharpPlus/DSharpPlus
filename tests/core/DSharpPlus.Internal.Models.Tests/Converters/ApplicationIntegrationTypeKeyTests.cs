// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text.Json;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Models.Serialization.Converters;

using Xunit;

namespace DSharpPlus.Internal.Models.Tests.Converters;

public class ApplicationIntegrationTypeKeyTests
{
    private static ReadOnlySpan<byte> ValidPayload =>
    """
    {
        "0": 2748,
        "1": 28
    }
    """u8;

    private static ReadOnlySpan<byte> InvalidFloatPayload =>
    """
    {
        "0": 237,
        "1.1": 47
    }
    """u8;

    private static ReadOnlySpan<byte> InvalidStringPayload =>
    """
    {
        "1": 83,
        "fail": 23
    }
    """u8;

    private readonly JsonSerializerOptions options;

    public ApplicationIntegrationTypeKeyTests()
    {
        this.options = new();
        this.options.Converters.Add(new ApplicationIntegrationTypeKeyConverter());
    }

    [Fact]
    public void TestSuccess()
    {
        Dictionary<DiscordApplicationIntegrationType, int> value =
            JsonSerializer.Deserialize<Dictionary<DiscordApplicationIntegrationType, int>>(ValidPayload, this.options)!;

        Assert.Equal(2, value.Count);
        Assert.Equal(28, value[DiscordApplicationIntegrationType.UserInstall]);
    }

    [Fact]
    public void TestFloatFailure()
    {
        try
        {
            _ = JsonSerializer.Deserialize<Dictionary<DiscordApplicationIntegrationType, int>>(InvalidFloatPayload, this.options);
            Assert.Fail("This should not have been reached.");
        }
        catch (JsonException exception)
        {
            Assert.Equal("Expected an integer key.", exception.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception type thrown.");
            throw;
        }
    }

    [Fact]
    public void TestStringFailure()
    {
        try
        {
            _ = JsonSerializer.Deserialize<Dictionary<DiscordApplicationIntegrationType, int>>(InvalidStringPayload, this.options);
            Assert.Fail("This should not have been reached.");
        }
        catch (JsonException exception)
        {
            Assert.Equal("Expected an integer key.", exception.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception type thrown.");
            throw;
        }
    }
}
