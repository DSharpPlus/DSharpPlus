using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DSharpPlus.Tools.ShardedEventHandlingGen;

public static class Program
{
    private static readonly ReadOnlyDictionary<string, string> templateFiles;

    static Program()
    {
        // Load all resource template files into the dictionary
        Assembly assembly = typeof(Program).Assembly;
        Dictionary<string, string> templateFiles = [];
        foreach (string filename in assembly.GetManifestResourceNames())
        {
            if (!filename.EndsWith(".template", StringComparison.Ordinal))
            {
                continue;
            }

            string templateFile = new StreamReader(assembly.GetManifestResourceStream(filename)!).ReadToEnd();
            templateFiles.Add(Path.GetFileNameWithoutExtension(filename.Replace("DSharpPlus.Tools.ShardedEventHandlingGen.", "")), templateFile);
        }

        templateFiles = new(templateFiles);
    }

    public static void Main()
    {
        StringBuilder eventStringBuilder = new();
        StringBuilder eventHandlerStringBuilder = new();
        StringBuilder hookMethodLogic = new();
        StringBuilder unhookMethodLogic = new();

        // Generate the events
        foreach (EventInfo eventInfo in typeof(DiscordClient).GetEvents().OrderBy(x => !x.Name.Equals("ClientErrored", StringComparison.Ordinal)).ThenBy(x => x.Name))
        {
            // EventName to eventName
            string eventNameCamelCased = $"_{char.ToLowerInvariant(eventInfo.Name[0])}{eventInfo.Name[1..]}";
            eventStringBuilder.AppendLine(templateFiles["DiscordShardedClient.Event"]
                .Replace("{{EventArgumentType}}", eventInfo.EventHandlerType!.GenericTypeArguments[1].Name)
                .Replace("{{EventName}}", eventInfo.Name)
                .Replace("{{EventNameCamelCased}}", eventNameCamelCased)
            );

            // this.Goof should be renamed to EverythingFuckedUp imo
            string errorHandler = eventInfo.Name.Equals("ClientErrored", StringComparison.Ordinal) ? "Goof" : "EventErrorHandler";

            // Convert the EventName into the EVENT_NAME format
            StringBuilder eventNameScreamingSnakeCase = new();
            for (int i = 0; i < eventInfo.Name.Length; i++)
            {
                char character = eventInfo.Name[i];
                if (char.IsUpper(character) && i != 0)
                {
                    eventNameScreamingSnakeCase.Append('_');
                }

                eventNameScreamingSnakeCase.Append(char.ToUpperInvariant(character));
            }

            eventHandlerStringBuilder.AppendLine($"        {eventNameCamelCased} = new(\"{eventNameScreamingSnakeCase}\", {errorHandler});");
            hookMethodLogic.AppendLine($"        client.{eventInfo.Name} += this.{eventInfo.Name}Delegator;");
            unhookMethodLogic.AppendLine($"        client.{eventInfo.Name} -= this.{eventInfo.Name}Delegator;");
        }

        File.WriteAllText($"{Environment.CurrentDirectory}/DSharpPlus/Clients/DiscordShardedClient.Events.cs", templateFiles["DiscordShardedClient"]
            .Replace("{{Date}}", DateTimeOffset.UtcNow.ToString("F", CultureInfo.InvariantCulture))
            .Replace("    // {{EventHandlers}}", eventStringBuilder.ToString().TrimEnd())
            .Replace("        // {{EventHandlerSetters}}", eventHandlerStringBuilder.ToString().TrimEnd())
            .Replace("        // {{HookEventHandlers}}", hookMethodLogic.ToString().TrimEnd())
            .Replace("        // {{UnhookEventHandlers}}", unhookMethodLogic.ToString().TrimEnd())
        );
    }
}
