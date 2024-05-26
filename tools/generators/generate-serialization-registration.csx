#!/usr/bin/env dotnet-script

#nullable enable

#r "nuget:Spectre.Console, 0.47.1-preview.0.26"

using System.IO;

using Spectre.Console;

// Syntax:
// generate-rest-payloads [abstraction path] [output path]
if (Args is ["-h" or "--help" or "-?"])
{
    AnsiConsole.MarkupLine
    (
        """
        [plum1]DSharpPlus Serialization Generator, v0.1.0[/]

          Usage: generate-serialization-registration.csx
        """
    );

    return 0;
}

AnsiConsole.MarkupLine
(
    """
    [plum1]DSharpPlus Serialization Generator, v0.1.0[/]
    """
);

WriteSerializationRegistration
(
    "src/core/DSharpPlus.Internal.Models",
    "src/core/DSharpPlus.Internal.Models/Extensions/ServiceCollectionExtensions.Registration.cs",
    "DSharpPlus.Internal.Abstractions.Models",
    "DSharpPlus.Internal.Models",
    "DSharpPlus.Internal.Models.Extensions"
);

WriteSerializationRegistration
(
    "src/core/DSharpPlus.Internal.Rest/Payloads",
    "src/core/DSharpPlus.Internal.Rest/Extensions/ServiceCollectionExtensions.Registration.cs",
    "DSharpPlus.Internal.Abstractions.Rest.Payloads",
    "DSharpPlus.Internal.Rest.Payloads",
    "DSharpPlus.Internal.Rest.Extensions"
);

public void WriteSerializationRegistration
(
    string input,
    string output,
    string abstractionNamespace,
    string targetNamespace,
    string outputNamespace
)
{
    string[] files = Directory.GetFiles(input, "*.cs", SearchOption.AllDirectories);

    files = files.Where
        (
            x => 
            {
                return !x.Contains("/Extensions/") && !x.Contains("\\Extensions\\") 
                    && !x.Contains("/Serialization/") && !x.Contains("\\Serialization\\");
            }
        )
        .Select
        (
            path => 
            {
                FileInfo file = new(path);
                return file.Name.Replace(".cs", "");
            }
        )
        .ToArray();

    using StreamWriter writer = new(output);

    writer.Write($$"""
// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0058

using {{abstractionNamespace}};
using {{targetNamespace}};
using DSharpPlus.Serialization;

using Microsoft.Extensions.DependencyInjection;

namespace {{outputNamespace}};

partial class ServiceCollectionExtensions
{
    private static void RegisterSerialization(IServiceCollection services)
    {
        services.Configure<SerializationOptions>
        (
            options =>
            {

""");

    foreach (string s in files)
    {
        writer.Write($"                options.AddModel<I{s}, {s}>();\r\n");
    }

    writer.Write("""
            }
        );
    }
}
""");
}