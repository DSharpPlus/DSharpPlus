// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0058

using System;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Models.Extensions;
using DSharpPlus.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace DSharpPlus.Internal.Models.Tests.Converters;

// tests one-of deserialization on example payloads for actual models
partial class OneOfConverterTests
{
    private readonly ISerializationService<AuditLogChangeTests> serializer;

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

        services.AddSingleton(typeof(ISerializationService<>), typeof(SerializationService<>));

        IServiceProvider provider = services.BuildServiceProvider();
        this.serializer = provider.GetRequiredService<ISerializationService<AuditLogChangeTests>>();
    }

    [Fact]
    public void TestApplicationCommandInteractionDataOptionUnion()
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

        Assert.Equal(17, value.Options.Value![0].Value.Value);
    }

    [Fact]
    public void TestInteractionResponseUnion()
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

        Assert.Equal("thingie", response.Data.Value.AsT0.Choices[0].Value);
    }
}
