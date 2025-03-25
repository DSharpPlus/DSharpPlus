using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Portability.Cpu;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace DSharpPlus.Tools.Benchmarks;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await DiscordData.SetupStaticVariablesAsync();

        // Locate all the benchmarks
        Type[] types = FindBenchmarkTypes(typeof(Program).Assembly.GetTypes()).Distinct().ToArray();

        // Run the benchmarks
        IConfig config = ManualConfig
            .CreateMinimumViable()
            .AddColumn([StatisticColumn.Max, StatisticColumn.Min])
            .AddDiagnoser([new MemoryDiagnoser(new())])
            .AddExporter([MarkdownExporter.GitHub])
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

#if DEBUG
        config = config.WithOptions(ConfigOptions.DisableOptimizationsValidator).AddJob(new DebugInProcessConfig().GetJobs().First()).StopOnFirstError(false);
#endif

        Summary[] summaries = BenchmarkRunner.Run(types, config, args: null);
        Summary firstSummary = summaries[0];

        StringBuilder results = new();
        results.AppendLine("### Machine Information:");
        results.AppendLine($"BenchmarkDotNet v{firstSummary.HostEnvironmentInfo.BenchmarkDotNetVersion}, {firstSummary.HostEnvironmentInfo.OsVersion.Value}");
        results.AppendLine($"- {CpuInfoFormatter.Format(firstSummary.HostEnvironmentInfo.CpuInfo.Value)}");
        results.AppendLine($"- Hardware Intrinsics: {firstSummary.Reports[0].GetHardwareIntrinsicsInfo()?.Replace(",", ", ") ?? "Not supported."}");
        results.AppendLine($"- {firstSummary.HostEnvironmentInfo.RuntimeVersion}, {firstSummary.HostEnvironmentInfo.Architecture.ToLowerInvariant()}, {firstSummary.HostEnvironmentInfo.JitInfo}");
        results.AppendLine($"- Total Execution Time: {GetHumanizedNanoSeconds(summaries.Sum(summary => summary.TotalTime.TotalNanoseconds))}");
        await File.WriteAllTextAsync("header.md", results.ToString());
    }

    private static IEnumerable<Type> FindBenchmarkTypes(IEnumerable<Type> types)
    {
        foreach (Type type in types)
        {
            if (type.GetCustomAttribute<BenchmarkAttribute>() is not null)
            {
                yield return type;
            }

            foreach (Type nestedType in FindBenchmarkTypes(type.GetNestedTypes()))
            {
                yield return nestedType;
            }

            foreach (Type nestedType in FindBenchmarkTypes(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)))
            {
                yield return nestedType;
            }
        }
    }

    private static IEnumerable<Type> FindBenchmarkTypes(IEnumerable<MethodInfo> methods)
    {
        foreach (MethodInfo method in methods)
        {
            if (method.GetCustomAttribute<BenchmarkAttribute>() is not null)
            {
                yield return method.DeclaringType!;
            }
        }
    }

    private static string GetHumanizedNanoSeconds(double nanoSeconds) => nanoSeconds switch
    {
        < 1_000 => nanoSeconds.ToString("N0", CultureInfo.InvariantCulture) + "ns",
        < 1_000_000 => (nanoSeconds / 1_000).ToString("N2", CultureInfo.InvariantCulture) + "μs",
        < 1_000_000_000 => (nanoSeconds / 1_000_000).ToString("N2", CultureInfo.InvariantCulture) + "ms",
        _ => GetHumanizedExecutionTime(nanoSeconds / 1_000_000_000)
    };

    private static string GetHumanizedExecutionTime(double seconds)
    {
        StringBuilder stringBuilder = new();
        if (seconds >= 60)
        {
            stringBuilder.Append((seconds / 60).ToString("N0", CultureInfo.InvariantCulture));
            stringBuilder.Append("m and ");
            seconds %= 60;
        }

        stringBuilder.Append(seconds.ToString("N3", CultureInfo.InvariantCulture));
        stringBuilder.Append('s');
        return stringBuilder.ToString();
    }
}
