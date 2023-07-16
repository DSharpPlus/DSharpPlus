// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json;

using DSharpPlus.Collections;
using DSharpPlus.Converters;

using Xunit;

namespace DSharpPlus.Shared.Tests.Converters;

/// <summary>
/// Tests <seealso cref="DictionarySlim{TKey, TValue}"/> serialization for string, string.
/// </summary>
public class DictionarySlimStringString
{
    [Fact]
    public void TestDeserialization()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimStringStringJsonConverter());

        string test = """
        {
            "key": "value",
            "other_key": "other_value"
        }
        """;

        DictionarySlim<string, string> dict = JsonSerializer.Deserialize<DictionarySlim<string, string>>(test, options)!;

        Assert.True(dict["other_key"] == "other_value");
        Assert.True(dict["key"] == "value");
    }

    [Fact]
    public void TestSerialization()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new DictionarySlimStringStringJsonConverter());

        DictionarySlim<string, string> dict = new();

        ref string value = ref dict.GetOrAddValueRef("key");
        value = "value";

        ref string otherValue = ref dict.GetOrAddValueRef("other_key");
        otherValue = "other_value";

        string serialized = JsonSerializer.Serialize(dict, options);

        Assert.Equal
        (
            "{\"key\":\"value\",\"other_key\":\"other_value\"}",
            serialized
        );
    }
}
