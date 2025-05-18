// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0058

using System;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Models.Extensions;
using DSharpPlus.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.Internal.Models.Tests.Converters;

// tests one-of deserialization on example payloads for actual models
partial class OneOfConverterTests
{
    private readonly SerializationService<SystemTextJsonFormatMarker> serializer;

    public OneOfConverterTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.RegisterDiscordModelSerialization();
        services.Configure<SerializationOptions>
        (
            options => options.SetFormat<OneOfConverterTests>()
        );

        services.AddLogging
        (
            builder => builder.ClearProviders().AddProvider(NullLoggerProvider.Instance)
        );

        services.AddSingleton(typeof(SerializationService<>));
        services.AddSingleton<SystemTextJsonSerializationBackend>();

        IServiceProvider provider = services.BuildServiceProvider();
        this.serializer = provider.GetRequiredService<SerializationService<SystemTextJsonFormatMarker>>();
    }

    [Test]
    public async Task TestApplicationCommandInteractionDataOptionUnion()
    {
        IApplicationCommandInteractionDataOption value = this.serializer.DeserializeModel<IApplicationCommandInteractionDataOption>
        (
            """
            {
                "name": "testificate",
                "type": 1,
                "options": [
                    {
                        "name": "example",
                        "type": 10,
                        "value": 17
                    }
                ]
            }
            """u8
        );

        await Assert.That(value.Options.Value![0].Value.Value).IsEqualTo(17);
    }

    [Test]
    public async Task TestInteractionResponseUnion()
    {
        IInteractionResponse response = this.serializer.DeserializeModel<IInteractionResponse>
        (
            """
            {
                "type": 8,
                "data": {
                    "choices": [
                        {
                            "name": "deimos",
                            "value": "thingie"
                        }
                    ]
                }
            }
            """u8
        );

        await Assert.That(response.Data.Value.AsT0.Choices[0].Value).IsEqualTo("thingie");
    }
}
