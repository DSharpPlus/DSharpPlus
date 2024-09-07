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

public class ComponentTests
{
    private readonly ISerializationService<ComponentTests> serializer;

    public ComponentTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.RegisterDiscordModelSerialization();
        services.Configure<SerializationOptions>
        (
            options => options.SetFormat<ComponentTests>()
        );

        services.AddLogging
        (
            builder => builder.ClearProviders().AddProvider(NullLoggerProvider.Instance)
        );

        services.AddSingleton(typeof(ISerializationService<>), typeof(SerializationService<>));

        IServiceProvider provider = services.BuildServiceProvider();
        this.serializer = provider.GetRequiredService<ISerializationService<ComponentTests>>();
    }

    private static ReadOnlySpan<byte> ActionRowTestPayload =>
    """
    {
        "id": 0,
        "content": "This is a message with components",
        "components": [
            {
                "type": 1,
                "components": [
                    {
                        "type": 2,
                        "label": "Click me!",
                        "style": 1,
                        "custom_id": "click_one"
                    }
                ]
            }
        ]
    }
    """u8;

    private static ReadOnlySpan<byte> OtherTopLevelComponentTestPayload =>
    """
    {
        "id": 0,
        "content": "This is a message with weird components!",
        "components": [
            {
                "type": 1,
                "components": [
                    {
                        "type": 2,
                        "label": "Don't click me!",
                        "style": 1,
                        "custom_id": "click_one"
                    }
                ]
            },
            {
                "type": 2784,
                "random_ignored_data": "ekh"
            }
        ]
    }
    """u8;

    [Fact]
    public void TestCorrectActionRowDeserialization()
    {
        IPartialMessage message = this.serializer.DeserializeModel<IPartialMessage>(ActionRowTestPayload);

        Assert.Single(message.Components.Value);
        Assert.IsAssignableFrom<IActionRowComponent>(message.Components.Value[0]);
        Assert.Single(((IActionRowComponent)message.Components.Value[0]).Components);
    }

    [Fact]
    public void TestCorrectUnknownComponentDeserialization()
    {
        IPartialMessage message = this.serializer.DeserializeModel<IPartialMessage>(OtherTopLevelComponentTestPayload);

        Assert.Equal(2, message.Components.Value.Count);
        Assert.IsAssignableFrom<IActionRowComponent>(message.Components.Value[0]);
        Assert.IsAssignableFrom<IUnknownComponent>(message.Components.Value[1]);
    }
}
