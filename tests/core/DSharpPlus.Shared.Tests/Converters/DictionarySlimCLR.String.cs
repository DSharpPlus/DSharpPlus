// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json;

using DSharpPlus.Collections;
using DSharpPlus.Converters;

using Xunit;

namespace DSharpPlus.Shared.Tests.Converters;

// here we deal with whether string keys are handled correctly
public partial class DictionarySlimCLR
{
    [Fact]
    public void Serialize_String()
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
    public void Deserialize_String()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimJsonConverterFactory());

        string serialized = "{\"test\":8}";

        DictionarySlim<string, ushort> dict = JsonSerializer.Deserialize<DictionarySlim<string, ushort>>
        (
            serialized, 
            options
        )!;

        Assert.Equal(8, dict["test"]);
    }
}
