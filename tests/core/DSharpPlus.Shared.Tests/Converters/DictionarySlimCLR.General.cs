// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;

using DSharpPlus.Collections;
using DSharpPlus.Converters;

using Xunit;

namespace DSharpPlus.Shared.Tests.Converters;

/// <summary>
/// Contains generic tests for <see cref="DictionarySlim{TKey, TValue}"/> de/serialization.
/// </summary>
// in this file, we look at whether we correctly evaluate CanConvert
public partial class DictionarySlimCLR
{
    [Fact]
    public void InvalidKeyType()
    {
        DictionarySlim<ComplexType, string> invalid = new();

        ref string value = ref invalid.GetOrAddValueRef
        (
            new() 
            { 
                First = 7, 
                Second = 8 
            }
        );

        value = "test";

        try
        {
            JsonSerializerOptions options = new();
            options.Converters.Add(new DictionarySlimJsonConverterFactory());

            string? serialized = JsonSerializer.Serialize(invalid, options);

            Assert.Null(serialized);
        }
        catch
        {
            Assert.True(true);
        }
    }

    [Fact]
    public void ValidKeyType_String()
    {
        DictionarySlim<string, string> valid = new();

        ref string value = ref valid.GetOrAddValueRef
        (
            "testificate"
        );

        value = "test";

        try
        {
            JsonSerializerOptions options = new();
            options.Converters.Add(new DictionarySlimJsonConverterFactory());

            string? serialized = JsonSerializer.Serialize(valid, options);

            Assert.NotNull(serialized);
        }
        catch
        {
            Assert.True(false);
        }
    }

    [Fact]
    public void ValidKeyType_Guid()
    {
        DictionarySlim<Guid, string> valid = new();

        ref string value = ref valid.GetOrAddValueRef
        (
            Guid.NewGuid()
        );

        value = "test";

        try
        {
            JsonSerializerOptions options = new();
            options.Converters.Add(new DictionarySlimJsonConverterFactory());

            string? serialized = JsonSerializer.Serialize(valid, options);

            Assert.NotNull(serialized);
        }
        catch
        {
            Assert.True(false);
        }
    }

    [Fact]
    public void ValidKeyType_UInt128()
    {
        DictionarySlim<UInt128, string> valid = new();

        ref string value = ref valid.GetOrAddValueRef
        (
            (UInt128)ulong.MaxValue + 2
        );

        value = "test";

        try
        {
            JsonSerializerOptions options = new();
            options.Converters.Add(new DictionarySlimJsonConverterFactory());

            string? serialized = JsonSerializer.Serialize(valid, options);

            Assert.NotNull(serialized);
        }
        catch
        {
            Assert.True(false);
        }
    }
}

#pragma warning disable CA1067 // we don't care about implementing Object.Equals here, it's a test type
readonly file struct ComplexType : IEquatable<ComplexType>
{
    public int First { get; init; }
    public int Second { get; init; }

    public bool Equals(ComplexType other) 
        => this.First == other.First && this.Second == other.Second;
}
