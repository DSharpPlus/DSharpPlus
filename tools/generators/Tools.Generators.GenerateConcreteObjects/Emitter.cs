// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools.Generators.GenerateConcreteObjects;

public static class Emitter
{
    public static void Emit
    (
        StringBuilder writer,
        IReadOnlyList<(string, string)> properties,
        IReadOnlyList<string>? overwrites = null
    )
    {
        foreach ((string, string) property in properties)
        {
            bool required = false;

            string type = property.Item1;

            if (!type.StartsWith("Optional") && !type.EndsWith("?"))
            {
                required = true;
            }

            if (overwrites is not null && overwrites.Contains(property.Item2))
            {
                type = type.EndsWith('?') ? $"{type[9..^2]}?" : type[9..^1];
                required = true;
            }

            writer.Append
            (
$$"""
    /// <inheritdoc/>
    public {{(required ? "required " : "")}}{{type}} {{property.Item2}} { get; init; }


"""
            );
        }
    }
}
