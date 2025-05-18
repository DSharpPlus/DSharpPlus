// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Models.Serialization.Converters;

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

    [Test]
    public async Task TestSuccess()
    {
        Dictionary<DiscordApplicationIntegrationType, int> value =
            JsonSerializer.Deserialize<Dictionary<DiscordApplicationIntegrationType, int>>(ValidPayload, this.options)!;

        using (Assert.Multiple())
        {

            await Assert.That(value.Count).IsEqualTo(2);
            await Assert.That(value[DiscordApplicationIntegrationType.UserInstall]).IsEqualTo(28);
        }
    }

    [Test]
    public async Task TestFloatFailure()
    {
        try
        {
            _ = JsonSerializer.Deserialize<Dictionary<DiscordApplicationIntegrationType, int>>(InvalidFloatPayload, this.options);
            Assert.Fail("This should not have been reached.");
        }
        catch (JsonException exception)
        {
            await Assert.That(exception.Message).IsEqualTo("Expected an integer key.");
        }
        catch
        {
            Assert.Fail("Wrong exception type thrown.");
            throw;
        }
    }

    [Test]
    public async Task TestStringFailure()
    {
        try
        {
            _ = JsonSerializer.Deserialize<Dictionary<DiscordApplicationIntegrationType, int>>(InvalidStringPayload, this.options);
            Assert.Fail("This should not have been reached.");
        }
        catch (JsonException exception)
        {
            await Assert.That(exception.Message).IsEqualTo("Expected an integer key.");
        }
        catch
        {
            Assert.Fail("Wrong exception type thrown.");
            throw;
        }
    }
}
