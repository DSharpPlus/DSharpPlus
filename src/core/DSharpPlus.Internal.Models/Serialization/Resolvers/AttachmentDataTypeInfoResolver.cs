// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

namespace DSharpPlus.Internal.Models.Serialization.Resolvers;

public static class AttachmentDataTypeInfoResolver
{
    public static IJsonTypeInfoResolver Default { get; } = new DefaultJsonTypeInfoResolver
    {
        Modifiers =
        {
            (type) =>
            {
                foreach (JsonPropertyInfo property in type.Properties)
                {
                    if
                    (
                        property.PropertyType == typeof(AttachmentData)
                        || property.PropertyType == typeof(AttachmentData?)
                        || property.PropertyType == typeof(Optional<AttachmentData>)
                        || property.PropertyType == typeof(Optional<AttachmentData?>)
                        || property.PropertyType == typeof(IReadOnlyList<AttachmentData>)
                    )
                    {
                        property.ShouldSerialize = (_, _) => false;
                    }
                }
            }
        }
    };
}
