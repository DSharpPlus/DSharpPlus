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

public class ComponentTests
{
    private readonly SerializationService<SystemTextJsonFormatMarker> serializer;

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

        services.AddSingleton(typeof(SerializationService<>));
        services.AddSingleton<SystemTextJsonSerializationBackend>();

        IServiceProvider provider = services.BuildServiceProvider();
        this.serializer = provider.GetRequiredService<SerializationService<SystemTextJsonFormatMarker>>();
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

    [Test]
    public async Task TestCorrectActionRowDeserialization()
    {
        IPartialMessage message = this.serializer.DeserializeModel<IPartialMessage>(ActionRowTestPayload);

        using (Assert.Multiple())
        {
            await Assert.That(message.Components.Value).HasSingleItem();
            await Assert.That(message.Components.Value[0]).IsAssignableTo<IActionRowComponent>();
            await Assert.That(((IActionRowComponent)message.Components.Value[0]).Components).HasSingleItem();
        }
    }

    [Test]
    public async Task TestCorrectUnknownComponentDeserialization()
    {
        IPartialMessage message = this.serializer.DeserializeModel<IPartialMessage>(OtherTopLevelComponentTestPayload);

        using (Assert.Multiple())
        {
            await Assert.That(message.Components.Value.Count).IsEqualTo(2);
            await Assert.That(message.Components.Value[0]).IsAssignableTo<IActionRowComponent>();
            await Assert.That(message.Components.Value[1]).IsAssignableTo<IUnknownComponent>();
        }
    }
}
