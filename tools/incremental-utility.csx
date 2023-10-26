#!/usr/bin/env dotnet-script

#nullable enable

#r "nuget:System.IO.Hashing, 8.0.0-rc.2.23479.6"

using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Contains the changes made to files since the last run of the incremental utility.
/// </summary>
public sealed record Changes
{
    /// <summary>
    /// The list of files that have been modified.
    /// </summary>
    public required IReadOnlyList<string> Modified { get; init; }

    /// <summary>
    /// The list of files that have been added.
    /// </summary>
    public required IReadOnlyList<string> Added { get; init; }

    /// <summary>
    /// The list of files that have been removed.
    /// </summary>
    public required IReadOnlyList<string> Removed { get; init; }
}

/// <summary>
/// Gets the changes made to the provided files since the last run.
/// </summary>
/// <param name="toolName">The name of the parent tool invoking this functionality.</param>
/// <param name="files">The files covered by the parent tool.</param>
/// <returns>A summary of modified, added and removed file paths.</returns>
public Changes GetFileChanges(string toolName, params string[] files)
{
    // load and deserialize the current hashes
    if (!Directory.Exists("./artifacts/hashes"))
    {
        Directory.CreateDirectory("./artifacts/hashes");
    }

    if (!File.Exists($"./artifacts/hashes/{toolName}.json"))
    {
        File.Create($"./artifacts/hashes/{toolName}.json").Close();
        CalculateAndSaveHashes(files, toolName);

        return new Changes
        {
            Added = files,
            Removed = Array.Empty<string>(),
            Modified = Array.Empty<string>()
        };
    }

    StreamReader reader = new($"./artifacts/hashes/{toolName}.json");

    Dictionary<string, string> oldHashes = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd())!;

    reader.Close();

    Dictionary<string, string> newHashes = CalculateAndSaveHashes(files, toolName);

    string[] added = files.Where(name => !oldHashes.ContainsKey(name)).ToArray();
    string[] removed = oldHashes.Where(candidate => !newHashes.ContainsKey(candidate.Key)).Select(kvp => kvp.Key).ToArray();
    string[] modified = files.Where
    (
        name =>
        {
            return oldHashes.TryGetValue(name, out string? oldHash)
                && newHashes.TryGetValue(name, out string? newHash)
                && oldHash != newHash;
        }
    ).ToArray();

    return new Changes
    {
        Added = added,
        Removed = removed,
        Modified = modified
    };
}

private Dictionary<string, string> CalculateAndSaveHashes(string[] files, string name)
{
    Dictionary<string, string> dictionary = new();

    foreach (string file in files)
    {
        XxHash3 xxh = new();
        xxh.Append(File.ReadAllBytes(file));
        
        string hash = xxh.GetCurrentHashAsUInt64().ToString();
        dictionary.Add(file, hash);
    }

    using StreamWriter writer = new($"./artifacts/hashes/{name}.json");

    writer.Write(JsonSerializer.Serialize(dictionary));

    return dictionary;
}