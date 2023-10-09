// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Models.Serialization.Converters;

/// <summary>
/// Provides serialization for discord's optional null booleans, see
/// <seealso href="https://discord.com/developers/docs/topics/permissions#role-object-role-tags-structure"/>.
/// </summary>
/// <remarks>
/// This needs to be applied to every null boolean property individually.
/// </remarks>
public class NullBooleanJsonConverter : JsonConverter<bool>
{
    // if the token type is False or True we have an actual boolean on our hands and should read it
    // appropriately. if not, we judge by the existence of the token (which is what discord sends).
    public override bool Read
    (
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        bool? value = JsonSerializer.Deserialize<bool?>(ref reader, options);

        // null is a 'true' value.
        return value != false;
    }

    // slightly off, but since we can't ever actually send this to discord we don't need to deal
    // with any of this. we'll serialize it as a correct boolean so it can be read correctly if the
    // end user uses our models for serialization.
    public override void Write
    (
        Utf8JsonWriter writer,
        bool value,
        JsonSerializerOptions options
    )
        => writer.WriteBooleanValue(value);
}
