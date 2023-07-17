// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json;

using DSharpPlus.Collections;
using DSharpPlus.Converters;

using Xunit;

namespace DSharpPlus.Shared.Tests.Converters;

// here we deal with whether integer keys, positive and negative, are handled correctly
public partial class DictionarySlimCLR
{
    [Fact]
    public void Serialize_PositiveInteger()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        DictionarySlim<int, int> dict = new();
        ref int value = ref dict.GetOrAddValueRef(3);

        value = 7;

        string serialized = JsonSerializer.Serialize(dict, options);

        Assert.Equal
        (
            "{\"3\":7}",
            serialized
        );
    }

    [Fact]
    public void Deserialize_PositiveInteger()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        string serialized = "{\"11\":\"test\"}";

        DictionarySlim<ushort, string> dict = JsonSerializer.Deserialize<DictionarySlim<ushort, string>>
        (
            serialized,
            options
        )!;

        Assert.Equal("test", dict[11]);
    }

    [Fact]
    public void Serialize_NegativeInteger()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        DictionarySlim<int, int> dict = new();
        ref int value = ref dict.GetOrAddValueRef(-9);

        value = 7;

        string serialized = JsonSerializer.Serialize(dict, options);

        Assert.Equal
        (
            "{\"-9\":7}",
            serialized
        );
    }

    [Fact]
    public void Deserialize_NegativeInteger()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        string serialized = "{\"-11\":\"test\"}";

        DictionarySlim<short, string> dict = JsonSerializer.Deserialize<DictionarySlim<short, string>>
        (
            serialized,
            options
        )!;

        Assert.Equal("test", dict[-11]);
    }
}
