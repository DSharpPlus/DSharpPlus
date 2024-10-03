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

public class AuditLogChangeTests
{
    private readonly SerializationService<SystemTextJsonFormatMarker> serializer;

    public AuditLogChangeTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.RegisterDiscordModelSerialization();
        services.Configure<SerializationOptions>
        (
            options => options.SetFormat<AuditLogChangeTests>()
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

    private static readonly byte[] IntPayload =
    """
    {
        "key": "$test",
        "new_value": 17,
        "old_value": 83
    }
    """u8.ToArray();

    private static readonly byte[] StringPayload =
    """
    {
        "key": "$test",
        "new_value": "this is the new value",
        "old_value": "this was the old value"
    }
    """u8.ToArray();

    private static readonly byte[] NewValueMissingPayload =
    """
    {
        "key": "$test",
        "old_value": "this was the old value"
    }
    """u8.ToArray();

    private static readonly byte[] OldValueMissingPayload =
    """
    {
        "key": "$test",
        "new_value": "this is the new value"
    }
    """u8.ToArray();

    [Fact]
    public void TestIntegerPayload()
    {
        IAuditLogChange change = this.serializer.DeserializeModel<IAuditLogChange>(IntPayload);

        Assert.True(change.NewValue.HasValue);
        Assert.True(change.OldValue.HasValue);

        Assert.Equal("17", change.NewValue.Value);
    }

    [Fact]
    public void TestStringPayload()
    {
        IAuditLogChange change = this.serializer.DeserializeModel<IAuditLogChange>(StringPayload);

        Assert.True(change.NewValue.HasValue);
        Assert.True(change.OldValue.HasValue);

        Assert.Equal("\"this was the old value\"", change.OldValue.Value);
    }

    [Fact]
    public void TestNewValueMissing()
    {
        IAuditLogChange change = this.serializer.DeserializeModel<IAuditLogChange>(NewValueMissingPayload);

        Assert.False(change.NewValue.HasValue);
        Assert.True(change.OldValue.HasValue);

        Assert.Equal("\"this was the old value\"", change.OldValue.Value);
    }

    [Fact]
    public void TestOldValueMissing()
    {
        IAuditLogChange change = this.serializer.DeserializeModel<IAuditLogChange>(OldValueMissingPayload);

        Assert.True(change.NewValue.HasValue);
        Assert.False(change.OldValue.HasValue);

        Assert.Equal("\"this is the new value\"", change.NewValue.Value);
    }
}
