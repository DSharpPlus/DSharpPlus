using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Tools.CodeBlockLanguageListGen;

public static class Program
{
    private static readonly string TemplateFile;

    static Program()
    {
        // Load the template file
        // The filename could probably be hardcoded.
        Assembly assembly = typeof(Program).Assembly;
        foreach (string filename in assembly.GetManifestResourceNames())
        {
            if (!filename.EndsWith(".template", StringComparison.Ordinal) || assembly.GetManifestResourceStream(filename) is not Stream manifestStream)
            {
                continue;
            }

            TemplateFile = new StreamReader(manifestStream).ReadToEnd();
            return;
        }

        throw new InvalidOperationException("Failed to load the template file.");
    }

    public static async Task Main()
    {
        // Grab the list of languages and their aliases
        IReadOnlyList<string> languages = await GetLanguagesAsync();
        StringBuilder languageListNode = new("new List<string>()\n");
        languageListNode.Append("    {\n");
        for (int i = 0; i < languages.Count; i++)
        {
            languageListNode.Append($"        \"{languages[i]}\"");
            if (i != languages.Count - 1)
            {
                languageListNode.Append(',');
            }

            languageListNode.Append('\n');
        }
        languageListNode.Append("    }.ToFrozenSet()");

        File.WriteAllText($"{Environment.CurrentDirectory}/DSharpPlus.Commands/ContextChecks/FromCode/FromCodeAttribute.LanguageList.cs", TemplateFile
            .Replace("{{Date}}", DateTimeOffset.UtcNow.ToString("F", CultureInfo.InvariantCulture))
            .Replace("{{CodeBlockLanguages}}", languageListNode.ToString()
        ));
    }

    public static async ValueTask<IReadOnlyList<string>> GetLanguagesAsync()
    {
        ProcessStartInfo psi = new("node", "tools/get-highlight-languages")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start the node process.");
        await process.WaitForExitAsync();

        string output = await process.StandardOutput.ReadToEndAsync();
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
