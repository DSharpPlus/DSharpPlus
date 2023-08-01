// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json.Serialization;

using Bundles;
using Bundles.Converters;

namespace Tools.IncrementalUtility;

/// <summary>
/// Represents a serialization model for the hashes.
/// </summary>
public partial record Hashes
{
    [JsonConverter(typeof(DictionarySlimStringStringJsonConverter))]
    public required DictionarySlim<string, string> Values { get; init; }    
}

[JsonSerializable(typeof(Hashes))]
internal partial class SerializationContext : JsonSerializerContext
{

}
