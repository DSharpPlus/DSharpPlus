// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Core.Models.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xunit;

namespace DSharpPlus.Core.Models.Tests.Converters;

public class AuditLogChangeTests
{
    private readonly JsonSerializerOptions options;

    public AuditLogChangeTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.RegisterDiscordModelSerialization();

        IServiceProvider provider = services.BuildServiceProvider();
        this.options = provider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("DSharpPlus");
    }

    private const string IntPayload = 
    """
    {
        "key": "$test",
        "new_value": 17,
        "old_value": 83
    }
    """;
    
    private const string StringPayload =
    """
    {
        "key": "$test",
        "new_value": "this is the new value",
        "old_value": "this was the old value"
    }
    """;

    private const string NewValueMissingPayload =
    """
    {
        "key": "$test",
        "old_value": "this was the old value"
    }
    """;

    private const string OldValueMissingPayload =
    """
    {
        "key": "$test",
        "new_value": "this is the new value"
    }
    """;
    
    [Fact]
    public void TestIntegerPayload()
    {
        IAuditLogChange change = JsonSerializer.Deserialize<IAuditLogChange>(IntPayload, this.options)!;

        Assert.True(change.NewValue.HasValue);
        Assert.True(change.OldValue.HasValue);

        Assert.Equal("17", change.NewValue.Value);
    }

    [Fact]
    public void TestStringPayload()
    {
        IAuditLogChange change = JsonSerializer.Deserialize<IAuditLogChange>(StringPayload, this.options)!;

        Assert.True(change.NewValue.HasValue);
        Assert.True(change.OldValue.HasValue);

        Assert.Equal("\"this was the old value\"", change.OldValue.Value);
    }

    [Fact]
    public void TestNewValueMissing()
    {
        IAuditLogChange change = JsonSerializer.Deserialize<IAuditLogChange>(NewValueMissingPayload, this.options)!;

        Assert.False(change.NewValue.HasValue);
        Assert.True(change.OldValue.HasValue);

        Assert.Equal("\"this was the old value\"", change.OldValue.Value);
    }

    [Fact]
    public void TestOldValueMissing()
    {
        IAuditLogChange change = JsonSerializer.Deserialize<IAuditLogChange>(OldValueMissingPayload, this.options)!;

        Assert.True(change.NewValue.HasValue);
        Assert.False(change.OldValue.HasValue);

        Assert.Equal("\"this is the new value\"", change.NewValue.Value);
    }
}
