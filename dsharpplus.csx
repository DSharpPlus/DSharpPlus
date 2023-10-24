#!/usr/bin/env dotnet-script

#nullable enable

#r "nuget:Spectre.Console, 0.47.1-preview.0.26"

using System.Diagnostics;
using System.Linq;

using Spectre.Console;

enum ToolType
{
    None,
    Generator,
    Analyzer
}

readonly record struct ToolMetadata
{
    public required string Name { get; init; }

    public required ToolType Type { get; init; }

    public required string Subset { get; init; }
}

ToolMetadata[] tools = 
{
    new() 
    {
        Name = "generate-concrete-objects",
        Subset = "core",
        Type = ToolType.Generator
    }
};

string[] subsets =
{
    "core"
};

// executes a given tool
void ExecuteTool(string tool, ToolType type)
{
    switch (type)
    {
        case ToolType.Generator:
            Process.Start("dotnet", $"script ./tools/generators/{tool}.csx");
            break;
        case ToolType.Analyzer:
            Process.Start("dotnet", $"script ./tools/analyzers/{tool}.csx");
            break;
    }
}

// executes all tools belonging to a subject
void ExecuteSubset(string[] subset, ToolType type)
{
    foreach (ToolMetadata metadata in tools)
    {
        if (type != ToolType.None && metadata.Type != type)
        {
            continue;
        }

        if (!subset.Contains(metadata.Subset))
        {
            continue;
        }

        ExecuteTool(metadata.Name, metadata.Type);
    }
}

// main entrypoint is here, at the big ass help command

if (Args.Count == 1 && Args is ["--help"] or ["-h"] or ["-?"])
{
    AnsiConsole.MarkupLine
    (
        """
        This is the primary script controlling the DSharpPlus build.

        [lightgoldenrod2_2]Usage: dsharpplus [[action]] <group> <options>[/]
        
        [lightgoldenrod2_2]Actions:[/]
          run           Runs the given tools or subset of tools.
          publish       Publishes the given subset of the library.
        
        [lightgoldenrod2_2]Groups:[/]
        [grey50]NOTE: Groups are only valid when operating on tools.[/]
        
          tools         Indiscriminately operates on all kinds of tools.
          generators    Operates on tools intended to generate code or metadata.
          analyzers     Operates on tools intended to analyze and communicate the validity of existing code and metadata
        
        [lightgoldenrod2_2]Options:[/]
        
          -s|--subset    Specifies one or more subsets to operate on.
          -n|--name     Specifies the individual name of a tools to operate on.
        
        [lightgoldenrod2_2]Examples:[/]
        
        The following command will run all core tools:
        [grey50]dsharpplus run tools --subset core[/]
        
        The following command will run all core generators:
        [grey50]dsharpplus run generators --subset core[/]
        
        The following command will only run a single tool, generate-concrete-objects:
        [grey50]dsharpplus run --name generate-concrete-objects[/]
        
        The following command will build the core library as well as the caching logic:
        [grey50]dsharpplus publish --subset core,cache[/]
        """
    );

    return 0;
}

if (Args.Count >= 1 && Args[0] == "publish")
{
    if (Args[1] == "-s" || Args[1] == "--subset")
    {
        if (Args.Count != 3)
        {
            AnsiConsole.MarkupLine("[red]Expected one argument to --subset.[/]");
            return 1;
        }

        string[] loadedSubsets = Args[2].Split(',');

        foreach (string s in loadedSubsets)
        {
            Process.Start("dotnet", $"pack ./src/{s} --tl");
        }

        return 0;
    }

    Process.Start("dotnet", "pack --tl");

    return 0;
}

if (Args[0] != "run")
{
    AnsiConsole.MarkupLine($"[red]The only supported top-level verbs are 'run' and 'publish', found {Args[0]}.[/]");
    return 1;
}

// we're now firmly in tooling territory

switch (Args[1])
{
    case "tools":

        if (Args[2] == "-s" || Args[2] == "--subset")
        {
            ExecuteSubset(Args[3].Split(','), ToolType.None);
        }
        else
        {
            ExecuteSubset(subsets, ToolType.None);
        }
        break;

    case "generators":

        if (Args[2] == "-s" || Args[2] == "--subset")
        {
            ExecuteSubset(Args[3].Split(','), ToolType.Generator);
        }
        else
        {
            ExecuteSubset(subsets, ToolType.Generator);
        }
        break;

    case "analyzers":

        if (Args[2] == "-s" || Args[2] == "--subset")
        {
            ExecuteSubset(Args[3].Split(','), ToolType.Analyzer);
        }
        else
        {
            ExecuteSubset(subsets, ToolType.Analyzer);
        }
        break;

    case "-n":
    case "--name":

        ToolType type = tools.Where(t => t.Name == Args[2]).Select(t => t.Type).FirstOrDefault();

        if (type == ToolType.None)
        {
            AnsiConsole.MarkupLine($"[red]The tool {Args[2]} could not be found.[/]");
            return 1;
        }

        ExecuteTool(Args[2], type);

        break;
}

return 0;
