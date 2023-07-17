// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;

using DSharpPlus.Collections;
using DSharpPlus.Converters;

using Xunit;

namespace DSharpPlus.Shared.Tests.Converters;

// here we deal with whether guid keys are handled correctly
public partial class DictionarySlimCLR
{
    [Fact]
    public void Serialize_Guid()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        DictionarySlim<Guid, int> dict = new();
        ref int value = ref dict.GetOrAddValueRef(Guid.Parse("20aa0594-1579-42b5-af9c-3295705239dd"));

        value = 7;

        string serialized = JsonSerializer.Serialize(dict, options);

        Assert.Equal
        (
            "{\"20aa0594-1579-42b5-af9c-3295705239dd\":7}",
            serialized
        );
    }

    [Fact]
    public void Deserialize_Guid()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        string serialized = "{\"1107720a-fa7d-437d-8347-b96986589d6e\":\"test\"}";

        DictionarySlim<Guid, string> dict = JsonSerializer.Deserialize<DictionarySlim<Guid, string>>
        (
            serialized,
            options
        )!;

        Assert.Equal("test", dict[Guid.Parse("1107720a-fa7d-437d-8347-b96986589d6e")]);
    }
}
