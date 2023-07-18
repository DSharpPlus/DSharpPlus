// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;

using DSharpPlus.Collections;
using DSharpPlus.Converters;

namespace Tools.IncrementalUtility;

/// <summary>
/// Represents a collection of current file hashes, with support for listing changed files, added files and
/// removed files.
/// </summary>
/// <param name="name">The name of the parent tool invoking this functionality.</param>
public sealed class MetadataCollection
(
    string name
)
    : IReadOnlyDictionary<string, string>
{
    private readonly DictionarySlim<string, string> hashes = new();

    private static readonly JsonSerializerOptions options = new();

    static MetadataCollection() => options.Converters.Add(new DictionarySlimStringStringJsonConverter());

    /// <summary>
    /// Calculates the current hashes of files, updates the cache and 
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public Changes CalculateDifferences
    (
        params string[] files
    )
    {
        // load and deserialize the current hashes
        // DictionarySlim<string, string> is really the only DictionarySlim we can do ahead of time

        if (!Directory.Exists("./artifacts/hashes"))
        {
            Directory.CreateDirectory("./artifacts/hashes");
        }

        // the file does not exist, go back to start and calculate all hashes
        if (!File.Exists($"./artifacts/hashes/{name}.json"))
        {
            File.Create($"./artifacts/hashes/{name}.json").Close();
            this.CalculateAndSaveHashes(files);

            return new Changes
            {
                Added = files,
                Removed = Array.Empty<string>(),
                Modified = Array.Empty<string>()
            };
        }

        // load the saved hashes
        StreamReader reader = new($"./artifacts/hashes/{name}.json");

#pragma warning disable IL3050 // we know these are fine, all involved types are statically linked to in this file.
#pragma warning disable IL2026
        DictionarySlim<string, string> oldHashes = JsonSerializer.Deserialize<DictionarySlim<string, string>>
        (
            reader.ReadToEnd(),
            options
        )!;
#pragma warning restore IL2026
#pragma warning restore IL3050

        reader.Close();

        // calculate the differences

        this.CalculateAndSaveHashes(files);

        IEnumerable<string> added = files.Where
        (
            name => !oldHashes.ContainsKey(name)
        );

        IEnumerable<string> removed = files.Where
        (
            name => oldHashes.ContainsKey(name) && !this.hashes.ContainsKey(name)
        );

        IEnumerable<string> modified = files.Where
        (
            name =>
            {
                return oldHashes.TryGetValue(name, out string? oldHash)
                    && this.hashes.TryGetValue(name, out string? newHash) 
                    && oldHash != newHash;
            }
        );

        return new Changes
        {
            Added = added,
            Removed = removed,
            Modified = modified
        };
    }

    private void CalculateAndSaveHashes
    (
        string[] files
    )
    {
        // note to future, we might want to somewhat parallelize this
        // the infrastructure is there, source files can be constructed without doing the much more
        // expensive load, but...
        foreach (string file in files)
        {
            SourceFile sourceFile = new(file);
            sourceFile.Load();

            ref string hash = ref this.hashes.GetOrAddValueRef(file);
            hash = sourceFile.Hash.ToString();
        }

        using StreamWriter writer = new($"./artifacts/hashes/{name}.json");

        writer.Write
        (
#pragma warning disable IL3050 // we know these are fine, all involved types are statically linked to in this file.
#pragma warning disable IL2026
            JsonSerializer.Serialize(this.hashes, options)
#pragma warning restore IL2026
#pragma warning restore IL3050
        );
    }

    /// <inheritdoc/>
    public string this[string key] => this.hashes[key];

    /// <inheritdoc/>
    public IEnumerable<string> Keys => this.hashes.Select(pair => pair.Key);

    /// <inheritdoc/>
    public IEnumerable<string> Values => this.hashes.Select(pair => pair.Value);

    /// <inheritdoc/>
    public int Count => this.hashes.Count;

    /// <inheritdoc/>
    public bool ContainsKey
    (
        string key
    ) 
        => this.hashes.ContainsKey(key);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() 
        => this.hashes.GetEnumerator();

    /// <inheritdoc/>
    public bool TryGetValue
    (
        string key, 

        [MaybeNullWhen(false)] 
        out string value
    )
    {
        return this.hashes.TryGetValue
        (
            key,
            out value
        );
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() 
        => this.GetEnumerator();
}
