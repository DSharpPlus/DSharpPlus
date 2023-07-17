// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;

using DSharpPlus.Collections;
using DSharpPlus.Converters;

using Xunit;

namespace DSharpPlus.Shared.Tests.Converters;

// here we deal with whether floating-point keys, positive and negative, are handled correctly
public partial class DictionarySlimCLR
{
    [Fact]
    public void Serialize_Float()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        DictionarySlim<Half, int> dict = new();
        ref int value = ref dict.GetOrAddValueRef((Half)(-3.2));

        value = 7;

        string serialized = JsonSerializer.Serialize(dict, options);

        Assert.Equal
        (
            "{\"-3.2\":7}",
            serialized
        );
    }

    [Fact]
    public void Deserialize_Float()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        string serialized = "{\"11.1\":\"test\"}";

        DictionarySlim<float, string> dict = JsonSerializer.Deserialize<DictionarySlim<float, string>>
        (
            serialized,
            options
        )!;

        Assert.Equal("test", dict[11.1f]);
    }
}
