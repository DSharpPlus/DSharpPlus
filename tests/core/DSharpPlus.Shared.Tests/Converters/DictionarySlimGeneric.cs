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
public class DictionarySlimGeneric
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
    public void Serialize()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        DictionarySlim<string, int> dict = new();
        ref int value = ref dict.GetOrAddValueRef("test");

        value = 7;

        string serialized = JsonSerializer.Serialize(dict, options);

        Assert.Equal
        (
            "{\"test\":7}",
            serialized
        );
    }

    [Fact]
    public void Deserialize()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        string serialized = "{\"8\":\"test\"}";

        DictionarySlim<ushort, string> dict = JsonSerializer.Deserialize<DictionarySlim<ushort, string>>(serialized, options)!;

        Assert.Equal("test", dict[8]);
    }
}

readonly file struct ComplexType : IEquatable<ComplexType>
{
    public int First { get; init; }
    public int Second { get; init; }

    public bool Equals(ComplexType other) 
        => this.First == other.First && this.Second == other.Second;
}
