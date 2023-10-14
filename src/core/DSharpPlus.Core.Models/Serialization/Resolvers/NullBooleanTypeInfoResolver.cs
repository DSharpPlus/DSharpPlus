// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json.Serialization.Metadata;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Core.Models.Serialization.Converters;

namespace DSharpPlus.Core.Models.Serialization.Resolvers;

public static class NullBooleanTypeInfoResolver
{
    public static IJsonTypeInfoResolver Default { get; } = new DefaultJsonTypeInfoResolver
    {
        Modifiers =
        {
            (type) =>
            {
                if (type.Type != typeof(IRoleTags))
                {
                    return;
                }

                foreach (JsonPropertyInfo property in type.Properties)
                {
                    if (property.PropertyType == typeof(bool))
                    {
                        property.IsRequired = false;
                        property.CustomConverter = new NullBooleanJsonConverter();
                    }
                }
            }
        }
    };
}
